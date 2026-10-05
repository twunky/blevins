using System.Data;
using System.Text;
using FamilySafety.Application.SafetyCircles;
using FamilySafety.Domain.Profiles;
using FamilySafety.Domain.SafetyCircles;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Infrastructure.Persistence;

public sealed class EfSafetyCircleRepository(FamilySafetyDbContext db, TimeProvider clock) : ISafetyCircleRepository
{
    private IQueryable<FamilyGroupMembership> Eligible(Guid family) => db.FamilyGroupMemberships.Where(m =>
        m.FamilyGroupId == family && m.IsActive && db.AdultProfiles.Any(p => p.Id == m.AdultProfileId && p.AccountId == m.AccountId));
    private IQueryable<SafetyCircle> Owned(Guid account, Guid family) => db.SafetyCircles.Where(c => c.FamilyGroupId == family
        && Eligible(family).Any(m => m.AccountId == account && m.AdultProfileId == c.OwnerProfileId
            && m.Id == c.OwnerMembershipId && m.AccessVersion == c.OwnerMembershipVersion));
    private IQueryable<SafetyCircleView> Project(IQueryable<SafetyCircle> rows) => rows.AsNoTracking().OrderBy(c => c.Name).ThenBy(c => c.Id)
        .Select(c => new SafetyCircleView(c.Id, c.FamilyGroupId, c.Name, c.Version,
            db.SafetyCircleMembers.Where(r => r.SafetyCircleId == c.Id && db.FamilyGroupMemberships.Any(m => m.FamilyGroupId == c.FamilyGroupId && m.IsActive
                && m.Id == r.MembershipId && m.AdultProfileId == r.MemberProfileId && m.AccessVersion == r.MembershipVersion
                && db.AdultProfiles.Any(p => p.Id == m.AdultProfileId && p.AccountId == m.AccountId)))
            .OrderBy(r => r.MemberProfileId).Select(r => new SafetyCircleMemberView(r.MemberProfileId,
                db.AdultProfiles.Where(p => p.Id == r.MemberProfileId).Select(p => p.DisplayName).Single())).ToList()));
    public async Task<IReadOnlyList<SafetyCircleView>?> ListAsync(Guid accountId, Guid familyId, CancellationToken ct)
    {
        if (!await Eligible(familyId).AnyAsync(m => m.AccountId == accountId, ct)) return null;
        return await Project(Owned(accountId, familyId)).ToListAsync(ct);
    }
    public Task<SafetyCircleView?> ReadAsync(Guid accountId, Guid familyId, Guid circleId, CancellationToken ct) =>
        Project(Owned(accountId, familyId).Where(c => c.Id == circleId)).SingleOrDefaultAsync(ct);
    // All writes acquire Family before memberships/circle, matching existing feature lock order.
    private Task<int> LockFamily(Guid family, CancellationToken ct) => db.FamilyGroups.Where(g => g.Id == family)
        .ExecuteUpdateAsync(u => u.SetProperty(g => g.CapacityLimit, g => g.CapacityLimit), ct);
    private async Task<SafetyCircle?> WriteRow(Guid account, Guid family, Guid circle, Guid version, CancellationToken ct)
    {
        await LockFamily(family, ct);
        // Never reuse tracked state after membership-trigger deletion or version changes.
        foreach (var entry in db.ChangeTracker.Entries<SafetyCircle>().Where(e => e.Entity.Id == circle).ToList()) entry.State = EntityState.Detached;
        foreach (var entry in db.ChangeTracker.Entries<SafetyCircleMember>().Where(e => e.Entity.SafetyCircleId == circle).ToList()) entry.State = EntityState.Detached;
        var row = await Owned(account, family).SingleOrDefaultAsync(c => c.Id == circle, ct);
        if (row is not null && row.Version != version) throw new SafetyCircleConflictException();
        return row;
    }
    public async Task<SafetyCircleView?> CreateAsync(Guid accountId, Guid familyId, string name, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await LockFamily(familyId, ct);
        var owner = await Eligible(familyId).AsNoTracking().SingleOrDefaultAsync(m => m.AccountId == accountId, ct);
        if (owner is null) return null;
        var fields = Name(name);
        var row = new SafetyCircle(Guid.NewGuid(), owner.AdultProfileId, familyId, owner.Id, owner.AccessVersion, fields.Display, fields.Normalized, clock.GetUtcNow());
        db.SafetyCircles.Add(row); await Save(ct);
        var view = await ReadAsync(accountId, familyId, row.Id, ct); await tx.CommitAsync(ct); return view;
    }
    public async Task<SafetyCircleView?> RenameAsync(Guid accountId, Guid familyId, Guid circleId, RenameSafetyCircleRequest request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var row = await WriteRow(accountId, familyId, circleId, request.Version, ct); if (row is null) return null;
        var fields = Name(request.Name); row.Rename(fields.Display, fields.Normalized, clock.GetUtcNow()); await Save(ct);
        var view = await ReadAsync(accountId, familyId, circleId, ct); await tx.CommitAsync(ct); return view;
    }
    public async Task<SafetyCircleView?> AddAsync(Guid accountId, Guid familyId, Guid circleId, AddSafetyCircleMemberRequest request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var row = await WriteRow(accountId, familyId, circleId, request.Version, ct); if (row is null) return null;
        var member = await Eligible(familyId).AsNoTracking().SingleOrDefaultAsync(m => m.AdultProfileId == request.MemberProfileId, ct);
        if (member is null) return null;
        if (await db.SafetyCircleMembers.AnyAsync(m => m.SafetyCircleId == circleId && m.MemberProfileId == member.AdultProfileId, ct))
            throw new SafetyCircleConflictException();
        db.SafetyCircleMembers.Add(new(circleId, member.AdultProfileId, member.Id, member.AccessVersion));
        row.Touch(clock.GetUtcNow()); await Save(ct);
        var view = await ReadAsync(accountId, familyId, circleId, ct); await tx.CommitAsync(ct); return view;
    }
    public async Task<SafetyCircleView?> RemoveAsync(Guid accountId, Guid familyId, Guid circleId, Guid profileId, Guid version, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var row = await WriteRow(accountId, familyId, circleId, version, ct); if (row is null) return null;
        var member = await db.SafetyCircleMembers.SingleOrDefaultAsync(m => m.SafetyCircleId == circleId && m.MemberProfileId == profileId, ct);
        if (member is null) return null;
        db.SafetyCircleMembers.Remove(member); row.Touch(clock.GetUtcNow()); await Save(ct);
        var view = await ReadAsync(accountId, familyId, circleId, ct); await tx.CommitAsync(ct); return view;
    }
    public async Task<bool> DeleteAsync(Guid accountId, Guid familyId, Guid circleId, Guid version, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var row = await WriteRow(accountId, familyId, circleId, version, ct); if (row is null) return false;
        db.SafetyCircles.Remove(row); await Save(ct); await tx.CommitAsync(ct); return true;
    }
    private static (string Display, string Normalized) Name(string? name)
    {
        var display = name?.Trim() ?? "";
        if (display.Length is < 1 or > 100 || display.Any(char.IsControl)) throw new SafetyCircleValidationException();
        var normalized = display.Normalize(NormalizationForm.FormKC).ToUpperInvariant();
        if (normalized.Length > 200) throw new SafetyCircleValidationException();
        return (display, normalized);
    }
    private async Task Save(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { throw new SafetyCircleConflictException(); }
        catch (DbUpdateException e) when (e.InnerException is SqlException { Number: 2601 or 2627 }) { throw new SafetyCircleDuplicateException(); }
    }
}
