using System.Data;
using FamilySafety.Application.Relationships;
using FamilySafety.Domain.Relationships;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Infrastructure.Persistence;

public sealed class EfFamilyRelationshipRepository(FamilySafetyDbContext dbContext)
    : IFamilyRelationshipRepository
{
    public async Task<IReadOnlyList<FamilyRelationshipView>?> GetForGroupAsync(
        Guid accountId,
        Guid familyGroupId,
        CancellationToken cancellationToken)
    {
        var currentProfileId = await (from membership in dbContext.FamilyGroupMemberships.AsNoTracking()
                                      join profile in dbContext.AdultProfiles.AsNoTracking()
                                          on membership.AdultProfileId equals profile.Id
                                      where membership.AccountId == accountId
                                          && membership.FamilyGroupId == familyGroupId
                                          && membership.IsActive
                                      select (Guid?)profile.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (currentProfileId is null)
        {
            return null;
        }

        return await (from relationship in dbContext.FamilyRelationships.AsNoTracking()
                      join proposer in dbContext.AdultProfiles.AsNoTracking()
                          on relationship.ProposerProfileId equals proposer.Id
                      join recipient in dbContext.AdultProfiles.AsNoTracking()
                          on relationship.RecipientProfileId equals recipient.Id
                      where relationship.FamilyGroupId == familyGroupId
                          && dbContext.FamilyGroupMemberships.Any(membership =>
                              membership.FamilyGroupId == familyGroupId
                              && membership.AdultProfileId == relationship.ProposerProfileId
                              && membership.IsActive)
                          && dbContext.FamilyGroupMemberships.Any(membership =>
                              membership.FamilyGroupId == familyGroupId
                              && membership.AdultProfileId == relationship.RecipientProfileId
                              && membership.IsActive)
                          && (relationship.Status == FamilyRelationshipStatus.Accepted
                              || relationship.ProposerProfileId == currentProfileId.Value
                              || relationship.RecipientProfileId == currentProfileId.Value)
                      orderby proposer.DisplayName, recipient.DisplayName
                      select new FamilyRelationshipView(
                          relationship.Id,
                          proposer.Id,
                          proposer.DisplayName,
                          relationship.ProposerLabel,
                          recipient.Id,
                          recipient.DisplayName,
                          FamilyRelationship.ReciprocalOf(relationship.ProposerLabel),
                          relationship.Status,
                          relationship.ProposerProfileId == currentProfileId.Value,
                          relationship.RecipientProfileId == currentProfileId.Value))
            .ToListAsync(cancellationToken);
    }

    public async Task<FamilyRelationshipOperationResult> ProposeAsync(
        Guid accountId,
        Guid familyGroupId,
        Guid recipientProfileId,
        FamilyRelationshipLabel proposerLabel,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        await LockFamilyGroupAsync(familyGroupId, cancellationToken);

        var proposerProfileId = await (from membership in dbContext.FamilyGroupMemberships
                                       join profile in dbContext.AdultProfiles
                                           on membership.AdultProfileId equals profile.Id
                                       where membership.AccountId == accountId
                                           && membership.FamilyGroupId == familyGroupId
                                           && membership.IsActive
                                       select (Guid?)profile.Id)
            .SingleOrDefaultAsync(cancellationToken);
        if (proposerProfileId is null || proposerProfileId == recipientProfileId
            || !await IsActiveMemberAsync(familyGroupId, recipientProfileId, cancellationToken))
        {
            return FamilyRelationshipOperationResult.Unavailable;
        }

        var firstProfileId = proposerProfileId.Value.CompareTo(recipientProfileId) < 0
            ? proposerProfileId.Value
            : recipientProfileId;
        var secondProfileId = proposerProfileId.Value.CompareTo(recipientProfileId) < 0
            ? recipientProfileId
            : proposerProfileId.Value;
        if (await dbContext.FamilyRelationships.AnyAsync(relationship =>
                relationship.FamilyGroupId == familyGroupId
                && relationship.FirstProfileId == firstProfileId
                && relationship.SecondProfileId == secondProfileId,
                cancellationToken))
        {
            return FamilyRelationshipOperationResult.Conflict;
        }

        dbContext.FamilyRelationships.Add(new FamilyRelationship(
            Guid.NewGuid(), familyGroupId, proposerProfileId.Value, recipientProfileId, proposerLabel));
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return FamilyRelationshipOperationResult.Success;
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return FamilyRelationshipOperationResult.Conflict;
        }
    }

    public async Task<FamilyRelationshipOperationResult> RespondAsync(
        Guid accountId,
        Guid relationshipId,
        FamilyRelationshipAction action,
        CancellationToken cancellationToken)
    {
        var familyGroupId = await dbContext.FamilyRelationships.AsNoTracking()
            .Where(relationship => relationship.Id == relationshipId)
            .Select(relationship => (Guid?)relationship.FamilyGroupId)
            .SingleOrDefaultAsync(cancellationToken);
        if (familyGroupId is null)
        {
            return FamilyRelationshipOperationResult.Unavailable;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        await LockFamilyGroupAsync(familyGroupId.Value, cancellationToken);

        var relationship = await dbContext.FamilyRelationships.SingleOrDefaultAsync(
            item => item.Id == relationshipId && item.FamilyGroupId == familyGroupId.Value,
            cancellationToken);
        var currentProfileId = await (from membership in dbContext.FamilyGroupMemberships
                                      join profile in dbContext.AdultProfiles
                                          on membership.AdultProfileId equals profile.Id
                                      where membership.AccountId == accountId
                                          && membership.FamilyGroupId == familyGroupId.Value
                                          && membership.IsActive
                                      select (Guid?)profile.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (relationship is null || currentProfileId is null
            || !await IsActiveMemberAsync(familyGroupId.Value, relationship.ProposerProfileId, cancellationToken)
            || !await IsActiveMemberAsync(familyGroupId.Value, relationship.RecipientProfileId, cancellationToken))
        {
            return FamilyRelationshipOperationResult.Unavailable;
        }

        try
        {
            switch (action)
            {
                case FamilyRelationshipAction.Accept:
                    relationship.Accept(currentProfileId.Value);
                    break;
                case FamilyRelationshipAction.Decline:
                    relationship.EnsureCanDecline(currentProfileId.Value);
                    dbContext.FamilyRelationships.Remove(relationship);
                    break;
                case FamilyRelationshipAction.Withdraw:
                    relationship.EnsureCanWithdraw(currentProfileId.Value);
                    dbContext.FamilyRelationships.Remove(relationship);
                    break;
                case FamilyRelationshipAction.Remove:
                    relationship.EnsureCanRemove(currentProfileId.Value);
                    dbContext.FamilyRelationships.Remove(relationship);
                    break;
                default:
                    return FamilyRelationshipOperationResult.Unavailable;
            }
        }
        catch (InvalidOperationException)
        {
            return FamilyRelationshipOperationResult.Unavailable;
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return FamilyRelationshipOperationResult.Success;
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return FamilyRelationshipOperationResult.Unavailable;
        }
    }

    private Task<bool> IsActiveMemberAsync(
        Guid familyGroupId,
        Guid profileId,
        CancellationToken cancellationToken) =>
        dbContext.FamilyGroupMemberships.AnyAsync(membership =>
            membership.FamilyGroupId == familyGroupId
            && membership.AdultProfileId == profileId
            && membership.IsActive,
            cancellationToken);

    private Task<int> LockFamilyGroupAsync(Guid familyGroupId, CancellationToken cancellationToken) =>
        dbContext.FamilyGroups
            .Where(group => group.Id == familyGroupId)
            .ExecuteUpdateAsync(
                updates => updates.SetProperty(group => group.CapacityLimit, group => group.CapacityLimit),
                cancellationToken);
}
