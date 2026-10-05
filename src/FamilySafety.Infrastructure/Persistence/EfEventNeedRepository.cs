using System.Data;
using FamilySafety.Application.Events;
using FamilySafety.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Infrastructure.Persistence;

public sealed class EfEventNeedRepository(FamilySafetyDbContext db, TimeProvider timeProvider) : IEventNeedRepository
{
    public async Task<EventNeedsView?> GetAsync(Guid accountId, Guid eventId, CancellationToken ct)
    {
        var groupId = await GroupAsync(eventId, ct);
        if (groupId is null) return null;
        // Read a coherent participation/Need snapshot, serialized with mutations and
        // eligibility loss. All protected operations acquire Family before Event.
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var item = await AuthorizeAsync(accountId, eventId, groupId.Value, ct);
        if (item is null) return null;
        var profileId = (await ProfileAsync(accountId, item.FamilyGroupId, ct))!.Value;
        var organizer = profileId == item.CreatorProfileId;
        var eligible = await EligibleAsync(item, ct);
        var rows = await db.FamilyEventNeeds.AsNoTracking().Where(n => n.EventId == eventId).OrderBy(n => n.CreatedAtUtc).ThenBy(n => n.Id).ToListAsync(ct);
        var needs = new List<EventNeedView>();
        foreach (var row in rows) needs.Add(await ProjectAsync(row, profileId, organizer, ct));
        await transaction.CommitAsync(ct);
        return new(organizer, eligible.Any(p => p.ProfileId == profileId), organizer ? eligible : [], needs);
    }

    public async Task<EventNeedView?> CreateAsync(Guid accountId, Guid eventId, CreateEventNeedRequest request, CancellationToken ct)
    {
        var groupId = await GroupAsync(eventId, ct);
        if (groupId is null) return null;
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var item = await AuthorizeAsync(accountId, eventId, groupId.Value, ct);
        if (item is null || await ProfileAsync(accountId, item.FamilyGroupId, ct) != item.CreatorProfileId) return null;
        Validate(request.Title, request.Detail, request.Category);
        var row = new FamilyEventNeed(Guid.NewGuid(), eventId, request.Title, request.Detail, request.Category, timeProvider.GetUtcNow());
        db.FamilyEventNeeds.Add(row);
        await SaveAsync(ct);
        var view = await ProjectAsync(row, item.CreatorProfileId, true, ct);
        await transaction.CommitAsync(ct);
        return view;
    }

    public async Task<EventNeedView?> EditAsync(Guid accountId, Guid eventId, Guid needId, EditEventNeedRequest request, CancellationToken ct)
    {
        var groupId = await GroupAsync(eventId, ct);
        if (groupId is null) return null;
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var item = await AuthorizeAsync(accountId, eventId, groupId.Value, ct);
        if (item is null || await ProfileAsync(accountId, item.FamilyGroupId, ct) != item.CreatorProfileId) return null;
        var row = await FreshAsync(eventId, needId, ct);
        if (row is null) return null;
        CheckVersion(row, request.Version);
        Validate(request.Title, request.Detail, request.Category);
        row.Edit(request.Title, request.Detail, request.Category, timeProvider.GetUtcNow());
        await SaveAsync(ct);
        var view = await ProjectAsync(row, item.CreatorProfileId, true, ct);
        await transaction.CommitAsync(ct);
        return view;
    }

    public async Task<bool> RemoveAsync(Guid accountId, Guid eventId, Guid needId, Guid version, CancellationToken ct)
    {
        var groupId = await GroupAsync(eventId, ct);
        if (groupId is null) return false;
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var item = await AuthorizeAsync(accountId, eventId, groupId.Value, ct);
        if (item is null || await ProfileAsync(accountId, item.FamilyGroupId, ct) != item.CreatorProfileId) return false;
        var row = await FreshAsync(eventId, needId, ct);
        if (row is null) return false;
        CheckVersion(row, version);
        db.FamilyEventNeeds.Remove(row);
        await SaveAsync(ct);
        await transaction.CommitAsync(ct);
        return true;
    }

    public async Task<EventNeedView?> ActAsync(Guid accountId, Guid eventId, Guid needId, Guid version, string action, Guid? assigneeId, CancellationToken ct)
    {
        var groupId = await GroupAsync(eventId, ct);
        if (groupId is null) return null;
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var item = await AuthorizeAsync(accountId, eventId, groupId.Value, ct);
        if (item is null) return null;
        var profileId = (await ProfileAsync(accountId, item.FamilyGroupId, ct))!.Value;
        var organizer = profileId == item.CreatorProfileId;
        var row = await FreshAsync(eventId, needId, ct);
        if (row is null) return null;
        var eligible = await EligibleAsync(item, ct);
        var selfEligible = eligible.Any(p => p.ProfileId == profileId);
        var own = selfEligible && row.ResponsibleProfileId == profileId;
        // Deny unauthorized actions before revealing version/state conflicts.
        if (!(action switch
        {
            "claim" => selfEligible,
            "assign" => organizer && eligible.Any(p => p.ProfileId == assigneeId),
            "release" or "accept" or "decline" => own,
            "complete" => organizer || own,
            _ => false,
        })) return null;
        CheckVersion(row, version);
        var prior = row.Status;
        var responsible = row.ResponsibleProfileId;
        var now = timeProvider.GetUtcNow();
        switch (action)
        {
            case "claim" when prior == EventNeedStatus.Open: row.Take(profileId, false, now); break;
            case "assign" when prior == EventNeedStatus.Open: row.Take(assigneeId!.Value, true, now); break;
            case "accept" when prior == EventNeedStatus.Assigned: row.Accept(now); break;
            case "decline" when prior == EventNeedStatus.Assigned: row.Release(now); break;
            case "release" when prior == EventNeedStatus.Claimed: row.Release(now); break;
            case "complete" when prior is EventNeedStatus.Assigned or EventNeedStatus.Claimed: row.Complete(now); break;
            default: throw new EventNeedConflictException();
        }
        db.EventNeedResponsibilityChanges.Add(new(row.Id, responsible ?? row.ResponsibleProfileId!.Value, prior, row.Status, action, now));
        await SaveAsync(ct);
        var view = await ProjectAsync(row, profileId, organizer, ct);
        await transaction.CommitAsync(ct);
        return view;
    }

    private Task<Guid?> GroupAsync(Guid eventId, CancellationToken ct) => db.FamilyEvents.AsNoTracking()
        .Where(e => e.Id == eventId).Select(e => (Guid?)e.FamilyGroupId).SingleOrDefaultAsync(ct);
    private Task<Guid?> ProfileAsync(Guid accountId, Guid groupId, CancellationToken ct) => db.FamilyGroupMemberships.AsNoTracking()
        .Where(m => m.AccountId == accountId && m.FamilyGroupId == groupId && m.IsActive).Select(m => (Guid?)m.AdultProfileId).SingleOrDefaultAsync(ct);
    private async Task<FamilyEvent?> AuthorizeAsync(Guid accountId, Guid eventId, Guid groupId, CancellationToken ct)
    {
        await db.FamilyGroups.Where(g => g.Id == groupId).ExecuteUpdateAsync(u => u.SetProperty(g => g.CapacityLimit, g => g.CapacityLimit), ct);
        var item = await EventVisibilityQuery.ForAccount(db, accountId).SingleOrDefaultAsync(e => e.Id == eventId, ct);
        return item?.FamilyGroupId == groupId ? item : null;
    }
    // Reuses invitation state and the Event visibility predicates. Keep the
    // correlated audience predicate inside the expression so EF translates it.
    private Task<List<EventInviteeChoice>> EligibleAsync(FamilyEvent item, CancellationToken ct) =>
        (from member in db.FamilyGroupMemberships.AsNoTracking()
         join profile in db.AdultProfiles.AsNoTracking() on member.AdultProfileId equals profile.Id
         where member.FamilyGroupId == item.FamilyGroupId && member.IsActive
             && (profile.Id == item.CreatorProfileId || db.FamilyEventAudienceMembers.Any(a => a.EventId == item.Id && a.ProfileId == profile.Id))
             && db.FamilyEventInvitations.Any(i => i.EventId == item.Id && i.InviteeProfileId == profile.Id
                 && i.RevokedAtUtc == null && i.RsvpStatus != EventRsvpStatus.NotGoing)
         orderby profile.DisplayName, profile.Id
         select new EventInviteeChoice(profile.Id, profile.DisplayName)).ToListAsync(ct);

    private async Task<EventNeedView> ProjectAsync(FamilyEventNeed row, Guid profileId, bool organizer, CancellationToken ct)
    {
        var disclose = organizer || row.ResponsibleProfileId == profileId;
        var name = disclose && row.ResponsibleProfileId is not null
            ? await db.AdultProfiles.Where(p => p.Id == row.ResponsibleProfileId).Select(p => p.DisplayName).SingleAsync(ct) : null;
        var history = await (from change in db.EventNeedResponsibilityChanges.AsNoTracking()
            join profile in db.AdultProfiles.AsNoTracking() on change.ProfileId equals profile.Id
            where change.NeedId == row.Id && (organizer || change.ProfileId == profileId)
            orderby change.OccurredAtUtc, change.Id
            select new EventNeedHistoryView(change.ProfileId, profile.DisplayName, change.PriorStatus, change.Status, change.Reason, change.OccurredAtUtc)).ToListAsync(ct);
        return new(row.Id, row.Title, row.Detail, row.Category, row.Status, disclose ? row.ResponsibleProfileId : null,
            name, row.ResponsibleProfileId == profileId, row.CreatedAtUtc, row.UpdatedAtUtc, row.Version, history);
    }
    private async Task<FamilyEventNeed?> FreshAsync(Guid eventId, Guid needId, CancellationToken ct)
    {
        var tracked = db.ChangeTracker.Entries<FamilyEventNeed>().FirstOrDefault(e => e.Entity.Id == needId);
        if (tracked is not null) tracked.State = EntityState.Detached;
        return await db.FamilyEventNeeds.SingleOrDefaultAsync(n => n.Id == needId && n.EventId == eventId, ct);
    }
    private static void CheckVersion(FamilyEventNeed row, Guid version)
    { if (row.Version != version) throw new EventNeedConflictException(); }
    private async Task SaveAsync(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { throw new EventNeedConflictException(); }
    }
    private static void Validate(string? title, string? detail, EventNeedCategory? category)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 200) errors["Title"] = ["Enter a title of up to 200 characters."];
        if (detail?.Trim().Length > 1000) errors["Detail"] = ["Keep details to 1,000 characters or fewer."];
        if (category is not null && !Enum.IsDefined(category.Value)) errors["Category"] = ["Choose a valid category."];
        if (errors.Count > 0) throw new FamilyEventValidationException(errors);
    }
}
