using System.Data;
using FamilySafety.Application.Events;
using FamilySafety.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Infrastructure.Persistence;

public sealed class EfEventUpdateRepository(FamilySafetyDbContext db, TimeProvider timeProvider) : IEventUpdateRepository
{
    public async Task<EventUpdatesView?> GetAsync(Guid accountId, Guid eventId, CancellationToken ct)
    {
        var groupId = await GroupAsync(eventId, ct);
        if (groupId is null) return null;
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var item = await AuthorizeAsync(accountId, eventId, groupId.Value, ct);
        if (item is null) return null;
        var organizer = await ProfileAsync(accountId, groupId.Value, ct) == item.CreatorProfileId;
        // Filter and project in SQL: hidden text and revision text are never loaded.
        var updates = await VisibleProjection(eventId).ToListAsync(ct);
        await transaction.CommitAsync(ct);
        return new(organizer, updates);
    }

    public async Task<EventUpdateView?> CreateAsync(Guid accountId, Guid eventId, CreateEventUpdateRequest request, CancellationToken ct)
    {
        var groupId = await GroupAsync(eventId, ct);
        if (groupId is null) return null;
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var item = await AuthorizeAsync(accountId, eventId, groupId.Value, ct);
        if (item is null || await ProfileAsync(accountId, groupId.Value, ct) != item.CreatorProfileId) return null;
        Validate(request.Text);
        var row = new FamilyEventUpdate(Guid.NewGuid(), eventId, item.CreatorProfileId, request.Text, timeProvider.GetUtcNow());
        db.FamilyEventUpdates.Add(row);
        await SaveAsync(ct);
        var view = await VisibleProjection(eventId, row.Id).SingleAsync(ct);
        await transaction.CommitAsync(ct);
        return view;
    }

    public async Task<EventUpdateView?> EditAsync(Guid accountId, Guid eventId, Guid updateId, EditEventUpdateRequest request, CancellationToken ct)
    {
        var groupId = await GroupAsync(eventId, ct);
        if (groupId is null) return null;
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var item = await AuthorizeAsync(accountId, eventId, groupId.Value, ct);
        if (item is null || await ProfileAsync(accountId, groupId.Value, ct) != item.CreatorProfileId) return null;
        var row = await FreshAsync(eventId, updateId, ct);
        if (row is null || row.AuthorProfileId != item.CreatorProfileId) return null;
        CheckState(row, request.Version);
        Validate(request.Text);
        db.EventUpdateRevisions.Add(row.Edit(request.Text, timeProvider.GetUtcNow()));
        await SaveAsync(ct);
        var view = await VisibleProjection(eventId, row.Id).SingleAsync(ct);
        await transaction.CommitAsync(ct);
        return view;
    }

    public async Task<bool> HideAsync(Guid accountId, Guid eventId, Guid updateId, Guid version, CancellationToken ct)
    {
        var groupId = await GroupAsync(eventId, ct);
        if (groupId is null) return false;
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var item = await AuthorizeAsync(accountId, eventId, groupId.Value, ct);
        if (item is null || await ProfileAsync(accountId, groupId.Value, ct) != item.CreatorProfileId) return false;
        var row = await FreshAsync(eventId, updateId, ct);
        if (row is null || row.AuthorProfileId != item.CreatorProfileId) return false;
        CheckState(row, version);
        row.Hide(timeProvider.GetUtcNow());
        await SaveAsync(ct);
        await transaction.CommitAsync(ct);
        return true;
    }

    private IQueryable<EventUpdateView> VisibleProjection(Guid eventId, Guid? updateId = null) =>
        from update in db.FamilyEventUpdates.AsNoTracking()
        join author in db.AdultProfiles.AsNoTracking() on update.AuthorProfileId equals author.Id
        where update.EventId == eventId && update.HiddenAtUtc == null && (updateId == null || update.Id == updateId)
        orderby update.CreatedAtUtc, update.Id
        select new EventUpdateView(update.Id, author.Id, author.DisplayName, update.Text,
            update.CreatedAtUtc, update.UpdatedAtUtc, update.Version);
    private Task<Guid?> GroupAsync(Guid eventId, CancellationToken ct) => db.FamilyEvents.AsNoTracking()
        .Where(e => e.Id == eventId).Select(e => (Guid?)e.FamilyGroupId).SingleOrDefaultAsync(ct);
    private Task<Guid?> ProfileAsync(Guid accountId, Guid groupId, CancellationToken ct) => db.FamilyGroupMemberships.AsNoTracking()
        .Where(m => m.AccountId == accountId && m.FamilyGroupId == groupId && m.IsActive).Select(m => (Guid?)m.AdultProfileId).SingleOrDefaultAsync(ct);
    private async Task<FamilyEvent?> AuthorizeAsync(Guid accountId, Guid eventId, Guid groupId, CancellationToken ct)
    {
        // Match protected Event operations: Family lock before Event/access reads.
        await db.FamilyGroups.Where(g => g.Id == groupId).ExecuteUpdateAsync(u => u.SetProperty(g => g.CapacityLimit, g => g.CapacityLimit), ct);
        var item = await EventVisibilityQuery.ForAccount(db, accountId).SingleOrDefaultAsync(e => e.Id == eventId, ct);
        return item?.FamilyGroupId == groupId ? item : null;
    }
    private async Task<FamilyEventUpdate?> FreshAsync(Guid eventId, Guid updateId, CancellationToken ct)
    {
        var tracked = db.ChangeTracker.Entries<FamilyEventUpdate>().FirstOrDefault(e => e.Entity.Id == updateId);
        if (tracked is not null) tracked.State = EntityState.Detached;
        return await db.FamilyEventUpdates.SingleOrDefaultAsync(u => u.EventId == eventId && u.Id == updateId, ct);
    }
    private static void CheckState(FamilyEventUpdate row, Guid version)
    {
        if (row.Version != version || row.HiddenAtUtc is not null) throw new EventUpdateConflictException();
    }
    private async Task SaveAsync(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { throw new EventUpdateConflictException(); }
    }
    private static void Validate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Trim().Length > FamilyEventUpdate.TextLimit)
            throw new FamilyEventValidationException(new Dictionary<string, string[]> { ["Text"] = ["Enter Update text of up to 1,000 characters."] });
    }
}
