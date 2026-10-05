using System.Data;
using FamilySafety.Application.FamilyDates;
using FamilySafety.Domain.FamilyDates;
using FamilySafety.Domain.Profiles;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Infrastructure.Persistence;

public sealed class EfSharedFamilyDateRepository(FamilySafetyDbContext db, TimeProvider clock) : ISharedFamilyDateRepository
{
    public async Task<SharedFamilyDatesView?> ListAsync(Guid accountId, Guid familyGroupId, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await LockFamilyAsync(familyGroupId, ct);
        var member = await MemberAsync(accountId, familyGroupId, ct);
        if (member is null) return null;
        await ReconcileFamilyAsync(familyGroupId, ct);
        // Authorization filters run in SQL before content or associated-person projections are loaded.
        var dates = await Projection(member, familyGroupId).ToListAsync(ct);
        await tx.CommitAsync(ct);
        return new(true, dates);
    }

    public async Task<SharedFamilyDateView?> CreateAsync(Guid accountId, Guid familyGroupId, CreateSharedFamilyDateRequest request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await LockFamilyAsync(familyGroupId, ct);
        var member = await MemberAsync(accountId, familyGroupId, ct);
        if (member is null) return null;
        Validate(request.Title, request.Description, request.Date);
        var associations = await AssociationsAsync(familyGroupId, member.AdultProfileId, request.AssociatedProfileIds, ct);
        if (associations is null) return null;
        var row = new SharedFamilyDate(Guid.NewGuid(), familyGroupId, member.AdultProfileId, member.Id,
            member.AccessVersion, request.Title, request.Description, request.Date, clock.GetUtcNow());
        db.SharedFamilyDates.Add(row);
        AddAssociations(row.Id, associations);
        await SaveAsync(ct);
        var view = await Projection(member, familyGroupId, row.Id).SingleAsync(ct);
        await tx.CommitAsync(ct);
        return view;
    }

    public async Task<SharedFamilyDateView?> EditAsync(Guid accountId, Guid familyGroupId, Guid dateId, EditSharedFamilyDateRequest request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await LockFamilyAsync(familyGroupId, ct);
        var member = await MemberAsync(accountId, familyGroupId, ct);
        if (member is null) return null;
        await ReconcileFamilyAsync(familyGroupId, ct);
        var row = await FreshVisibleAsync(member, familyGroupId, dateId, ct);
        if (row is null || (request.AssociatedProfileIds is not null && row.CreatorProfileId != member.AdultProfileId)) return null;
        if (row.Version != request.Version) throw new SharedFamilyDateConflictException();
        Validate(request.Title, request.Description, request.Date);
        if (request.AssociatedProfileIds is not null)
        {
            var associations = await AssociationsAsync(familyGroupId, row.CreatorProfileId, request.AssociatedProfileIds, ct);
            if (associations is null) return null;
            // Explicit creator reassociation is allowed; membership-loss unavailability is irreversible.
            await db.SharedFamilyDateAssociations.Where(a => a.DateId == row.Id).ExecuteDeleteAsync(ct);
            foreach (var tracked in db.ChangeTracker.Entries<SharedFamilyDateAssociation>().Where(e => e.Entity.DateId == row.Id).ToList())
                tracked.State = EntityState.Detached;
            AddAssociations(row.Id, associations);
        }
        row.Edit(request.Title, request.Description, request.Date, clock.GetUtcNow());
        await SaveAsync(ct);
        var view = await Projection(member, familyGroupId, row.Id).SingleAsync(ct);
        await tx.CommitAsync(ct);
        return view;
    }

    public async Task<bool> DeleteAsync(Guid accountId, Guid familyGroupId, Guid dateId, Guid version, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await LockFamilyAsync(familyGroupId, ct);
        var member = await MemberAsync(accountId, familyGroupId, ct);
        if (member is null) return false;
        await ReconcileFamilyAsync(familyGroupId, ct);
        var row = await FreshVisibleAsync(member, familyGroupId, dateId, ct);
        if (row is null || row.CreatorProfileId != member.AdultProfileId) return false;
        if (row.Version != version) throw new SharedFamilyDateConflictException();
        row.MakeUnavailable(clock.GetUtcNow());
        await SaveAsync(ct);
        await tx.CommitAsync(ct);
        return true;
    }

    public async Task ReconcileAsync(CancellationToken ct)
    {
        var families = await db.SharedFamilyDates.AsNoTracking().Select(d => d.FamilyGroupId).Distinct().ToListAsync(ct);
        foreach (var family in families)
        {
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            await LockFamilyAsync(family, ct);
            await ReconcileFamilyAsync(family, ct);
            await tx.CommitAsync(ct);
        }
    }

    private IQueryable<SharedFamilyDate> Available(Guid family) => db.SharedFamilyDates.Where(d =>
        d.FamilyGroupId == family && d.UnavailableAtUtc == null && d.RetainUntilUtc == null
        && db.FamilyGroupMemberships.Any(m => m.Id == d.CreatorMembershipId && m.IsActive
            && m.FamilyGroupId == family && m.AdultProfileId == d.CreatorProfileId && m.AccessVersion == d.CreatorMembershipVersion)
        && !db.SharedFamilyDateAssociations.Any(a => a.DateId == d.Id && !db.FamilyGroupMemberships.Any(m =>
            m.Id == a.MembershipId && m.IsActive && m.FamilyGroupId == family && m.AdultProfileId == a.ProfileId
            && m.AccessVersion == a.MembershipVersion)));

    private IQueryable<SharedFamilyDate> Visible(FamilyGroupMembership member, Guid family) => Available(family).Where(d =>
        d.CreatorProfileId == member.AdultProfileId || db.SharedFamilyDateAssociations.Any(a => a.DateId == d.Id
            && a.ProfileId == member.AdultProfileId && a.MembershipId == member.Id && a.MembershipVersion == member.AccessVersion));

    private IQueryable<SharedFamilyDateView> Projection(FamilyGroupMembership member, Guid family, Guid? dateId = null) =>
        Visible(member, family).AsNoTracking().Where(d => dateId == null || d.Id == dateId)
        .OrderBy(d => d.Date).ThenBy(d => d.Title).ThenBy(d => d.Id)
        .Select(d => new SharedFamilyDateView(d.Id, d.Title, d.Description, d.Date, d.CreatorProfileId,
            (from a in db.SharedFamilyDateAssociations
             join p in db.AdultProfiles on a.ProfileId equals p.Id
             where a.DateId == d.Id orderby p.DisplayName, p.Id
             select new FamilyDatePerson(p.Id, p.DisplayName)).ToList(), true,
            d.CreatorProfileId == member.AdultProfileId, d.CreatorProfileId == member.AdultProfileId, d.Version));

    private Task<FamilyGroupMembership?> MemberAsync(Guid account, Guid family, CancellationToken ct) =>
        db.FamilyGroupMemberships.AsNoTracking().SingleOrDefaultAsync(m => m.AccountId == account && m.FamilyGroupId == family && m.IsActive, ct);

    private async Task<SharedFamilyDate?> FreshVisibleAsync(FamilyGroupMembership member, Guid family, Guid dateId, CancellationToken ct)
    {
        var tracked = db.ChangeTracker.Entries<SharedFamilyDate>().FirstOrDefault(e => e.Entity.Id == dateId);
        if (tracked is not null) tracked.State = EntityState.Detached;
        return await Visible(member, family).SingleOrDefaultAsync(d => d.Id == dateId, ct);
    }
    private async Task<List<FamilyGroupMembership>?> AssociationsAsync(Guid family, Guid creator, Guid[]? ids, CancellationToken ct)
    {
        if (ids is null) throw new SharedFamilyDateValidationException(new Dictionary<string, string[]> { ["AssociatedProfileIds"] = ["Choose the associated members explicitly."] });
        var people = ids.Distinct().ToArray();
        if (people.Contains(creator)) return null; // Creator already has access; no duplicate association.
        var memberships = await db.FamilyGroupMemberships.AsNoTracking().Where(m => m.FamilyGroupId == family
            && m.IsActive && people.Contains(m.AdultProfileId)).ToListAsync(ct);
        return memberships.Count == people.Length ? memberships : null;
    }
    private void AddAssociations(Guid dateId, IEnumerable<FamilyGroupMembership> members) =>
        db.SharedFamilyDateAssociations.AddRange(members.Select(m => new SharedFamilyDateAssociation(dateId, m.AdultProfileId, m.Id, m.AccessVersion)));

    private Task<int> LockFamilyAsync(Guid family, CancellationToken ct) => db.FamilyGroups.Where(g => g.Id == family)
        .ExecuteUpdateAsync(u => u.SetProperty(g => g.CapacityLimit, g => g.CapacityLimit), ct);

    // Trigger handles live loss at the mutation instant. This fallback handles stale operational
    // restoration/missing memberships before any normal projection, without authorizing retained rows.
    private async Task ReconcileFamilyAsync(Guid family, CancellationToken ct)
    {
        var invalid = await db.SharedFamilyDates.AsNoTracking().Where(d => d.FamilyGroupId == family && d.UnavailableAtUtc == null
            && !Available(family).Any(v => v.Id == d.Id)).ToListAsync(ct);
        foreach (var row in invalid)
        {
            var creator = await db.FamilyGroupMemberships.AsNoTracking().SingleOrDefaultAsync(m => m.Id == row.CreatorMembershipId, ct);
            var lost = await (from a in db.SharedFamilyDateAssociations
                              join m in db.FamilyGroupMemberships on a.MembershipId equals m.Id into members
                              from m in members.DefaultIfEmpty()
                              where a.DateId == row.Id && (m == null || !m.IsActive || m.AccessVersion != a.MembershipVersion
                                  || m.FamilyGroupId != family || m.AdultProfileId != a.ProfileId)
                              select new { a.ProfileId, EndedAt = m == null ? null : m.EndedAt }).ToListAsync(ct);
            var first = lost.Where(a => a.EndedAt is not null).Select(a => a.EndedAt!.Value).Append(clock.GetUtcNow());
            if (creator?.EndedAt is not null) first = first.Append(creator.EndedAt.Value);
            var when = first.Min();
            await db.SharedFamilyDates.Where(d => d.Id == row.Id && d.UnavailableAtUtc == null)
                .ExecuteUpdateAsync(u => u.SetProperty(d => d.UnavailableAtUtc, when)
                    .SetProperty(d => d.RetainUntilUtc, when.AddDays(30)).SetProperty(d => d.Version, Guid.NewGuid()), ct);
            var lostIds = lost.Select(a => a.ProfileId).ToArray();
            await db.SharedFamilyDateAssociations.Where(a => a.DateId == row.Id && lostIds.Contains(a.ProfileId)).ExecuteDeleteAsync(ct);
        }
        var now = clock.GetUtcNow();
        var expired = db.SharedFamilyDates.Where(d => d.FamilyGroupId == family && d.UnavailableAtUtc != null && d.RetainUntilUtc <= now);
        await db.SharedFamilyDateAssociations.Where(a => expired.Any(d => d.Id == a.DateId)).ExecuteDeleteAsync(ct);
        await expired.ExecuteDeleteAsync(ct);
    }
    private async Task SaveAsync(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { throw new SharedFamilyDateConflictException(); }
    }
    private static void Validate(string? title, string? description, DateOnly date)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > SharedFamilyDate.TitleLimit) errors["Title"] = ["Enter a title of up to 100 characters."];
        if (string.IsNullOrWhiteSpace(description) || description.Trim().Length > SharedFamilyDate.DescriptionLimit) errors["Description"] = ["Enter a description of up to 1,000 characters."];
        if (date == default) errors["Date"] = ["Enter a specific calendar date including its year."];
        if (errors.Count > 0) throw new SharedFamilyDateValidationException(errors);
    }
}
