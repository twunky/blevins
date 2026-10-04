using System.Data;
using FamilySafety.Application.Events;
using FamilySafety.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Infrastructure.Persistence;

public sealed class EfEventInvitationRepository(FamilySafetyDbContext db, TimeProvider timeProvider)
    : IEventInvitationRepository
{
    public async Task<OrganizerInvitationsView?> GetOrganizerAsync(Guid accountId, Guid eventId, CancellationToken cancellationToken)
    {
        var item = await GetAuthorizedEventAsync(accountId, eventId, cancellationToken);
        if (item is null || !await IsOrganizerAsync(accountId, item, cancellationToken)) return null;
        var choices = await (from member in db.FamilyGroupMemberships.AsNoTracking()
            join profile in db.AdultProfiles.AsNoTracking() on member.AdultProfileId equals profile.Id
            where member.FamilyGroupId == item.FamilyGroupId && member.IsActive
                && member.AdultProfileId != item.CreatorProfileId
                && db.FamilyEventAudienceMembers.Any(audience => audience.EventId == eventId && audience.ProfileId == profile.Id)
            orderby profile.DisplayName, profile.Id
            select new EventInviteeChoice(profile.Id, profile.DisplayName)).ToListAsync(cancellationToken);
        var rows = await (from invitation in db.FamilyEventInvitations.AsNoTracking()
            join profile in db.AdultProfiles.AsNoTracking() on invitation.InviteeProfileId equals profile.Id
            where invitation.EventId == eventId
            orderby invitation.CreatedAtUtc, invitation.Id
            select new { Invitation = invitation, profile.DisplayName }).ToListAsync(cancellationToken);
        return new(choices, rows.Select(row => new NamedEventInvitationView(ToView(row.Invitation), row.DisplayName)).ToArray());
    }

    public async Task<OwnEventInvitationView?> GetOwnAsync(Guid accountId, Guid eventId, CancellationToken cancellationToken)
    {
        var item = await GetAuthorizedEventAsync(accountId, eventId, cancellationToken);
        if (item is null) return null;
        var profileId = await ProfileAsync(accountId, item.FamilyGroupId, cancellationToken);
        var row = await db.FamilyEventInvitations.AsNoTracking().SingleOrDefaultAsync(invitation =>
            invitation.EventId == eventId && invitation.InviteeProfileId == profileId && invitation.RevokedAtUtc == null, cancellationToken);
        return new(row is null ? null : ToView(row));
    }

    public async Task<IReadOnlyList<PendingEventInvitationView>?> GetPendingAsync(Guid accountId, Guid familyGroupId, CancellationToken cancellationToken)
    {
        var profileId = await ProfileAsync(accountId, familyGroupId, cancellationToken);
        if (profileId is null) return null;
        return await (from item in EventVisibilityQuery.ForAccount(db, accountId)
            join invitation in db.FamilyEventInvitations.AsNoTracking() on item.Id equals invitation.EventId
            where item.FamilyGroupId == familyGroupId && invitation.InviteeProfileId == profileId
                && invitation.RevokedAtUtc == null && invitation.RsvpStatus == EventRsvpStatus.Pending
            orderby item.Date, item.Time, item.Name, item.Id
            select new PendingEventInvitationView(invitation.Id, item.Id, item.Name, item.Date, item.Time))
            .ToListAsync(cancellationToken);
    }

    public async Task<EventInvitationView?> IssueAsync(Guid accountId, Guid eventId, IssueEventInvitationRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var item = await LockAndAuthorizeAsync(accountId, eventId, cancellationToken);
        if (item is null || !await IsOrganizerAsync(accountId, item, cancellationToken)) return null;
        if (request.InviteeProfileId == item.CreatorProfileId
            || !await db.FamilyGroupMemberships.AsNoTracking().AnyAsync(member => member.FamilyGroupId == item.FamilyGroupId
                && member.IsActive && member.AdultProfileId == request.InviteeProfileId, cancellationToken)
            || !await db.FamilyEventAudienceMembers.AsNoTracking().AnyAsync(audience => audience.EventId == eventId
                && audience.ProfileId == request.InviteeProfileId, cancellationToken)) return null;
        if (await db.FamilyEventInvitations.AsNoTracking().AnyAsync(invitation => invitation.EventId == eventId
            && invitation.InviteeProfileId == request.InviteeProfileId && invitation.RevokedAtUtc == null, cancellationToken))
            throw new EventInvitationConflictException();
        var row = new FamilyEventInvitation(Guid.NewGuid(), eventId, request.InviteeProfileId, timeProvider.GetUtcNow());
        db.FamilyEventInvitations.Add(row);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ToView(row);
    }

    public async Task<EventInvitationView?> RevokeAsync(Guid accountId, Guid eventId, Guid invitationId, RevokeEventInvitationRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var item = await LockAndAuthorizeAsync(accountId, eventId, cancellationToken);
        if (item is null || !await IsOrganizerAsync(accountId, item, cancellationToken)) return null;
        var row = await FreshInvitationAsync(eventId, invitationId, cancellationToken);
        if (row is null || row.RevokedAtUtc is not null) return null;
        if (row.Version != request.Version) throw new EventInvitationConflictException();
        row.Revoke(timeProvider.GetUtcNow());
        await SaveMutationAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ToView(row);
    }

    public async Task<EventInvitationView?> RespondAsync(Guid accountId, Guid eventId, UpdateEventRsvpRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var item = await LockAndAuthorizeAsync(accountId, eventId, cancellationToken);
        if (item is null) return null;
        var profileId = await ProfileAsync(accountId, item.FamilyGroupId, cancellationToken);
        var row = await FreshInvitationAsync(eventId, request.InvitationId, cancellationToken);
        if (row is null || row.InviteeProfileId != profileId || row.RevokedAtUtc is not null) return null;
        if (row.Version != request.Version) throw new EventInvitationConflictException();
        if (!Enum.IsDefined(request.RsvpStatus)) throw new FamilyEventValidationException(
            new Dictionary<string, string[]> { [nameof(request.RsvpStatus)] = ["Choose a valid RSVP."] });
        row.Respond(request.RsvpStatus, timeProvider.GetUtcNow());
        await SaveMutationAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ToView(row);
    }

    private async Task SaveMutationAsync(CancellationToken cancellationToken)
    {
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { throw new EventInvitationConflictException(); }
    }

    private async Task<FamilyEventInvitation?> FreshInvitationAsync(Guid eventId, Guid invitationId, CancellationToken cancellationToken)
    {
        // Re-read from SQL even if this scoped context previously tracked the row.
        // A stale EF identity-map value cannot supply mutation authority or version.
        var tracked = db.ChangeTracker.Entries<FamilyEventInvitation>().FirstOrDefault(entry => entry.Entity.Id == invitationId);
        if (tracked is not null) tracked.State = EntityState.Detached;
        return await db.FamilyEventInvitations.SingleOrDefaultAsync(row => row.EventId == eventId && row.Id == invitationId, cancellationToken);
    }

    private async Task<FamilyEvent?> LockAndAuthorizeAsync(Guid accountId, Guid eventId, CancellationToken cancellationToken)
    {
        // Resolve the lock target internally; disclose nothing until the protected
        // visibility query below succeeds. Serializable reads hold current access
        // and membership stable through commit, including external eligibility loss.
        var groupId = await db.FamilyEvents.AsNoTracking().Where(item => item.Id == eventId)
            .Select(item => (Guid?)item.FamilyGroupId).SingleOrDefaultAsync(cancellationToken);
        if (groupId is null) return null;
        await db.FamilyGroups.Where(group => group.Id == groupId).ExecuteUpdateAsync(
            updates => updates.SetProperty(group => group.CapacityLimit, group => group.CapacityLimit), cancellationToken);
        return await GetAuthorizedEventAsync(accountId, eventId, cancellationToken);
    }

    private Task<FamilyEvent?> GetAuthorizedEventAsync(Guid accountId, Guid eventId, CancellationToken cancellationToken) =>
        EventVisibilityQuery.ForAccount(db, accountId).SingleOrDefaultAsync(item => item.Id == eventId, cancellationToken);
    private Task<Guid?> ProfileAsync(Guid accountId, Guid groupId, CancellationToken cancellationToken) =>
        db.FamilyGroupMemberships.AsNoTracking().Where(member => member.AccountId == accountId
            && member.FamilyGroupId == groupId && member.IsActive).Select(member => (Guid?)member.AdultProfileId).SingleOrDefaultAsync(cancellationToken);
    private async Task<bool> IsOrganizerAsync(Guid accountId, FamilyEvent item, CancellationToken cancellationToken) =>
        await ProfileAsync(accountId, item.FamilyGroupId, cancellationToken) == item.CreatorProfileId;
    private static EventInvitationView ToView(FamilyEventInvitation row) => new(row.Id, row.InviteeProfileId,
        row.CreatedAtUtc, row.RevokedAtUtc, row.RsvpStatus, row.RsvpUpdatedAtUtc, row.Version);
}
