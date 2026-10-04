using System.Data;
using FamilySafety.Application.Invitations;
using FamilySafety.Domain.Profiles;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Infrastructure.Persistence;

public sealed class EfMembershipInvitationRepository(FamilySafetyDbContext dbContext)
    : IMembershipInvitationRepository
{
    public async Task<IReadOnlyList<AdministeredFamilyGroupView>> GetAdministeredGroupsAsync(
        Guid accountId,
        CancellationToken cancellationToken) =>
        await (from membership in dbContext.FamilyGroupMemberships.AsNoTracking()
               join assignment in dbContext.FamilyAdministratorAssignments.AsNoTracking()
                   on membership.Id equals assignment.MembershipId
               join familyGroup in dbContext.FamilyGroups.AsNoTracking()
                   on membership.FamilyGroupId equals familyGroup.Id
               where membership.AccountId == accountId && membership.IsActive
               orderby familyGroup.Name
               select new AdministeredFamilyGroupView(familyGroup.Id, familyGroup.Name))
            .ToListAsync(cancellationToken);

    public async Task<InvitationRepositoryOutcome> IssueAsync(
        Guid accountId,
        Guid familyGroupId,
        string recipientEmail,
        string normalizedRecipientEmail,
        string tokenHash,
        DateTimeOffset issuedAt,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        await LockFamilyGroupAsync(familyGroupId, cancellationToken);
        var authorization = await GetAdministrationAsync(accountId, familyGroupId, cancellationToken);
        if (authorization is null)
        {
            return InvitationRepositoryOutcome.Unavailable;
        }

        if (await ActiveMemberCountAsync(familyGroupId, cancellationToken) >= authorization.Value.Capacity)
        {
            return InvitationRepositoryOutcome.CapacityReached;
        }

        var previous = await dbContext.MembershipInvitations.SingleOrDefaultAsync(
            invitation => invitation.FamilyGroupId == familyGroupId
                && invitation.NormalizedRecipientEmail == normalizedRecipientEmail
                && invitation.Status == MembershipInvitationStatus.Pending,
            cancellationToken);
        if (previous is not null)
        {
            if (previous.IsAvailable(issuedAt))
            {
                previous.Revoke(issuedAt);
            }
            else
            {
                previous.Expire(issuedAt);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        dbContext.MembershipInvitations.Add(new MembershipInvitation(
            Guid.NewGuid(), familyGroupId, authorization.Value.MembershipId, recipientEmail,
            normalizedRecipientEmail, tokenHash, issuedAt, expiresAt));
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return InvitationRepositoryOutcome.Success;
    }

    public async Task<InvitationRepositoryOutcome> RevokeAsync(
        Guid accountId,
        Guid familyGroupId,
        string normalizedRecipientEmail,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        await LockFamilyGroupAsync(familyGroupId, cancellationToken);
        if (await GetAdministrationAsync(accountId, familyGroupId, cancellationToken) is null)
        {
            return InvitationRepositoryOutcome.Unavailable;
        }

        var invitation = await dbContext.MembershipInvitations.SingleOrDefaultAsync(
            item => item.FamilyGroupId == familyGroupId
                && item.NormalizedRecipientEmail == normalizedRecipientEmail
                && item.Status == MembershipInvitationStatus.Pending,
            cancellationToken);
        if (invitation is not null)
        {
            if (invitation.IsAvailable(now)) invitation.Revoke(now); else invitation.Expire(now);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return InvitationRepositoryOutcome.Success;
    }

    public async Task<InvitationPreviewData> PreviewAsync(
        Guid accountId,
        string tokenHash,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var eligible = await GetEligibleAsync(accountId, tokenHash, now, track: false, cancellationToken);
        return eligible is null
            ? new InvitationPreviewData(InvitationRepositoryOutcome.Unavailable)
            : new InvitationPreviewData(
                InvitationRepositoryOutcome.Success, eligible.FamilyGroupName, eligible.InviterDisplayName);
    }

    public async Task<InvitationAcceptanceData> AcceptAsync(
        Guid accountId,
        string tokenHash,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var familyGroupId = await FindFamilyGroupIdAsync(tokenHash, cancellationToken);
        if (familyGroupId is null)
        {
            return new(InvitationRepositoryOutcome.Unavailable);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        await LockFamilyGroupAsync(familyGroupId.Value, cancellationToken);
        var eligible = await GetEligibleAsync(accountId, tokenHash, now, track: true, cancellationToken);
        if (eligible is null)
        {
            return new(InvitationRepositoryOutcome.Unavailable);
        }

        if (await ActiveMemberCountAsync(eligible.FamilyGroupId, cancellationToken) >= eligible.Capacity)
        {
            return new(InvitationRepositoryOutcome.CapacityReached);
        }

        eligible.Invitation.Accept(accountId, now);
        dbContext.FamilyGroupMemberships.Add(new FamilyGroupMembership(
            Guid.NewGuid(), accountId, eligible.ProfileId, eligible.FamilyGroupId, isActive: true));
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new(InvitationRepositoryOutcome.Success, eligible.FamilyGroupId);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new(InvitationRepositoryOutcome.Unavailable);
        }
    }

    public async Task<InvitationRepositoryOutcome> DeclineAsync(
        Guid accountId,
        string tokenHash,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var familyGroupId = await FindFamilyGroupIdAsync(tokenHash, cancellationToken);
        if (familyGroupId is null)
        {
            return InvitationRepositoryOutcome.Unavailable;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        await LockFamilyGroupAsync(familyGroupId.Value, cancellationToken);
        var eligible = await GetEligibleAsync(accountId, tokenHash, now, track: true, cancellationToken);
        if (eligible is null)
        {
            return InvitationRepositoryOutcome.Unavailable;
        }

        eligible.Invitation.Decline(now);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return InvitationRepositoryOutcome.Success;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return InvitationRepositoryOutcome.Unavailable;
        }
    }

    private async Task<(Guid MembershipId, int Capacity)?> GetAdministrationAsync(
        Guid accountId,
        Guid familyGroupId,
        CancellationToken cancellationToken) =>
        await (from membership in dbContext.FamilyGroupMemberships
               join assignment in dbContext.FamilyAdministratorAssignments
                   on membership.Id equals assignment.MembershipId
               join familyGroup in dbContext.FamilyGroups
                   on membership.FamilyGroupId equals familyGroup.Id
               where membership.AccountId == accountId && membership.FamilyGroupId == familyGroupId
                   && membership.IsActive
               select new ValueTuple<Guid, int>(membership.Id, familyGroup.CapacityLimit))
            .Cast<(Guid MembershipId, int Capacity)?>()
            .SingleOrDefaultAsync(cancellationToken);

    private Task<int> ActiveMemberCountAsync(Guid familyGroupId, CancellationToken cancellationToken) =>
        dbContext.FamilyGroupMemberships.CountAsync(
            membership => membership.FamilyGroupId == familyGroupId && membership.IsActive,
            cancellationToken);

    private Task<Guid?> FindFamilyGroupIdAsync(string tokenHash, CancellationToken cancellationToken) =>
        dbContext.MembershipInvitations.AsNoTracking()
            .Where(invitation => invitation.TokenHash == tokenHash)
            .Select(invitation => (Guid?)invitation.FamilyGroupId)
            .SingleOrDefaultAsync(cancellationToken);

    private async Task LockFamilyGroupAsync(Guid familyGroupId, CancellationToken cancellationToken)
    {
        // A no-op update obtains a transaction-scoped exclusive row lock. Serializing all
        // capacity-changing invitation operations per group prevents double acceptance and overflow.
        await dbContext.FamilyGroups
            .Where(group => group.Id == familyGroupId)
            .ExecuteUpdateAsync(
                updates => updates.SetProperty(group => group.CapacityLimit, group => group.CapacityLimit),
                cancellationToken);
    }

    private async Task<EligibleInvitation?> GetEligibleAsync(
        Guid accountId,
        string tokenHash,
        DateTimeOffset now,
        bool track,
        CancellationToken cancellationToken)
    {
        var invitations = track ? dbContext.MembershipInvitations : dbContext.MembershipInvitations.AsNoTracking();
        var data = await (from invitation in invitations
                          join familyGroup in dbContext.FamilyGroups on invitation.FamilyGroupId equals familyGroup.Id
                          join issuer in dbContext.FamilyGroupMemberships on invitation.IssuedByMembershipId equals issuer.Id
                          join administrator in dbContext.FamilyAdministratorAssignments on issuer.Id equals administrator.MembershipId
                          join inviter in dbContext.AdultProfiles on issuer.AdultProfileId equals inviter.Id
                          join recipient in dbContext.AdultProfiles on accountId equals recipient.AccountId
                          where invitation.TokenHash == tokenHash
                              && invitation.Status == MembershipInvitationStatus.Pending
                              && invitation.ExpiresAt > now
                              && issuer.IsActive
                              && !dbContext.FamilyGroupMemberships.Any(membership =>
                                  membership.AccountId == accountId && membership.FamilyGroupId == invitation.FamilyGroupId)
                          select new EligibleInvitation(
                              invitation, invitation.FamilyGroupId, familyGroup.Name, familyGroup.CapacityLimit,
                              inviter.DisplayName, recipient.Id))
            .SingleOrDefaultAsync(cancellationToken);
        return data;
    }

    private sealed record EligibleInvitation(
        MembershipInvitation Invitation,
        Guid FamilyGroupId,
        string FamilyGroupName,
        int Capacity,
        string InviterDisplayName,
        Guid ProfileId);
}
