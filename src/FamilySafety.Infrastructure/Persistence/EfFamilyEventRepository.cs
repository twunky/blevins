using System.Data;
using FamilySafety.Application.Events;
using FamilySafety.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Infrastructure.Persistence;

public sealed class EfFamilyEventRepository(FamilySafetyDbContext dbContext) : IFamilyEventRepository
{
    public async Task<IReadOnlyList<FamilyEventView>?> GetVisibleAsync(
        Guid accountId,
        Guid familyGroupId,
        CancellationToken cancellationToken)
    {
        // The membership pre-check keeps the list contract: a caller with no active
        // membership in this Family gets null (404), while a member who can see
        // nothing gets an empty list.
        var currentProfileId = await GetActiveProfileIdAsync(
            accountId, familyGroupId, cancellationToken);
        if (currentProfileId is null)
        {
            return null;
        }

        var rows = await QueryVisibleEvents(accountId)
            .Where(row => row.FamilyGroupId == familyGroupId)
            .OrderBy(row => row.Date)
            .ThenBy(row => row.Time)
            .ThenBy(row => row.Name)
            .ToListAsync(cancellationToken);
        return rows.Select(ToView).ToArray();
    }

    public async Task<FamilyEventView?> GetAsync(
        Guid accountId,
        Guid eventId,
        CancellationToken cancellationToken)
    {
        // One query for every outcome: a missing Event and an Event the caller may
        // not read both return null, so the caller cannot tell them apart.
        var row = await QueryVisibleEvents(accountId)
            .Where(item => item.EventId == eventId)
            .SingleOrDefaultAsync(cancellationToken);
        return row is null ? null : ToView(row);
    }

    public async Task<FamilyEventView?> CreateAsync(
        Guid accountId,
        Guid familyGroupId,
        CreateFamilyEventRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        await LockFamilyGroupAsync(familyGroupId, cancellationToken);

        var creatorProfileId = await GetActiveProfileIdAsync(
            accountId, familyGroupId, cancellationToken);
        if (creatorProfileId is null
            || request.AudienceProfileIds.Contains(creatorProfileId.Value)
            || request.EditorProfileIds.Except(request.AudienceProfileIds).Any()
            || !await AreActiveAudienceMembersAsync(
                familyGroupId, request.AudienceProfileIds, cancellationToken))
        {
            return null;
        }

        var eventId = Guid.NewGuid();
        var familyEvent = new FamilyEvent(
            eventId,
            familyGroupId,
            creatorProfileId.Value,
            request.Name,
            request.Description,
            request.Date!.Value,
            request.Time!.Value,
            request.Location);

        dbContext.FamilyEvents.Add(familyEvent);
        dbContext.FamilyEventAudienceMembers.AddRange(request.AudienceProfileIds.Select(
            profileId => new FamilyEventAudienceMember(
                eventId, profileId, request.EditorProfileIds.Contains(profileId))));
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await GetCreatedEventAsync(eventId, cancellationToken);
    }

    public async Task<FamilyEventView?> UpdateAsync(
        Guid accountId,
        Guid familyGroupId,
        Guid eventId,
        UpdateFamilyEventRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        await LockFamilyGroupAsync(familyGroupId, cancellationToken);

        var currentProfileId = await GetActiveProfileIdAsync(
            accountId, familyGroupId, cancellationToken);
        if (currentProfileId is null)
        {
            return null;
        }

        var familyEvent = await dbContext.FamilyEvents
            .Where(item => item.Id == eventId
                && item.FamilyGroupId == familyGroupId
                && (item.CreatorProfileId == currentProfileId.Value
                    || dbContext.FamilyEventAudienceMembers.Any(audienceMember =>
                        audienceMember.EventId == item.Id
                        && audienceMember.ProfileId == currentProfileId.Value
                        && audienceMember.CanEdit)))
            .SingleOrDefaultAsync(cancellationToken);
        if (familyEvent is null)
        {
            return null;
        }

        familyEvent.UpdateDetails(
            request.Name,
            request.Description,
            request.Date!.Value,
            request.Time!.Value,
            request.Location);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await GetCreatedEventAsync(eventId, cancellationToken);
    }

    // The only definition of Event read visibility (REQ-PRIV-021): the caller has an
    // active membership in the Event's Family and is its creator or a selected
    // audience member. CanEdit is the creator or a selected editor. Family
    // Administrator status grants nothing. The list and the item read both use it.
    private IQueryable<VisibleEventRow> QueryVisibleEvents(Guid accountId) =>
        from membership in dbContext.FamilyGroupMemberships.AsNoTracking()
        where membership.AccountId == accountId && membership.IsActive
        join familyEvent in EventVisibilityQuery.ForAccount(dbContext, accountId)
            on membership.FamilyGroupId equals familyEvent.FamilyGroupId
        join creator in dbContext.AdultProfiles.AsNoTracking()
            on familyEvent.CreatorProfileId equals creator.Id
        select new VisibleEventRow
        {
            EventId = familyEvent.Id,
            FamilyGroupId = familyEvent.FamilyGroupId,
            Name = familyEvent.Name,
            Description = familyEvent.Description,
            Date = familyEvent.Date,
            Time = familyEvent.Time,
            Location = familyEvent.Location,
            OrganizerProfileId = creator.Id,
            OrganizerDisplayName = creator.DisplayName,
            CanEdit = familyEvent.CreatorProfileId == membership.AdultProfileId
                || dbContext.FamilyEventAudienceMembers.Any(audienceMember =>
                    audienceMember.EventId == familyEvent.Id
                    && audienceMember.ProfileId == membership.AdultProfileId
                    && audienceMember.CanEdit),
        };

    private static FamilyEventView ToView(VisibleEventRow row) => new(
        row.EventId,
        row.FamilyGroupId,
        row.Name,
        row.Description,
        row.Date,
        row.Time,
        row.Location,
        row.OrganizerProfileId,
        row.OrganizerDisplayName,
        row.CanEdit);

    private Task<Guid?> GetActiveProfileIdAsync(
        Guid accountId,
        Guid familyGroupId,
        CancellationToken cancellationToken) =>
        (from membership in dbContext.FamilyGroupMemberships.AsNoTracking()
         where membership.AccountId == accountId
             && membership.FamilyGroupId == familyGroupId
             && membership.IsActive
         select (Guid?)membership.AdultProfileId)
        .SingleOrDefaultAsync(cancellationToken);

    private async Task<bool> AreActiveAudienceMembersAsync(
        Guid familyGroupId,
        IReadOnlyList<Guid> profileIds,
        CancellationToken cancellationToken)
    {
        var activeProfileIds = await dbContext.FamilyGroupMemberships.AsNoTracking()
            .Where(membership => membership.FamilyGroupId == familyGroupId
                && membership.IsActive
                && profileIds.Contains(membership.AdultProfileId))
            .Select(membership => membership.AdultProfileId)
            .ToListAsync(cancellationToken);
        return activeProfileIds.Count == profileIds.Count;
    }

    private Task<FamilyEventView?> GetCreatedEventAsync(
        Guid eventId,
        CancellationToken cancellationToken) =>
        (from familyEvent in dbContext.FamilyEvents.AsNoTracking()
         join creator in dbContext.AdultProfiles.AsNoTracking()
             on familyEvent.CreatorProfileId equals creator.Id
         where familyEvent.Id == eventId
         select new FamilyEventView(
             familyEvent.Id,
             familyEvent.FamilyGroupId,
             familyEvent.Name,
             familyEvent.Description,
             familyEvent.Date,
             familyEvent.Time,
             familyEvent.Location,
             creator.Id,
             creator.DisplayName,
             CanEdit: true))
        .SingleOrDefaultAsync(cancellationToken);

    private Task<int> LockFamilyGroupAsync(Guid familyGroupId, CancellationToken cancellationToken) =>
        dbContext.FamilyGroups
            .Where(group => group.Id == familyGroupId)
            .ExecuteUpdateAsync(
                updates => updates.SetProperty(group => group.CapacityLimit, group => group.CapacityLimit),
                cancellationToken);

    // A query-side row with settable members, so EF Core can still filter and order
    // on its properties after the visibility projection.
    private sealed class VisibleEventRow
    {
        public Guid EventId { get; init; }
        public Guid FamilyGroupId { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public DateOnly Date { get; init; }
        public TimeOnly Time { get; init; }
        public required string Location { get; init; }
        public Guid OrganizerProfileId { get; init; }
        public required string OrganizerDisplayName { get; init; }
        public bool CanEdit { get; init; }
    }
}
