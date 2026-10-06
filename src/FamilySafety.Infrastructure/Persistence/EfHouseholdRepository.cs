using System.Data;
using System.Text;
using FamilySafety.Application.Households;
using FamilySafety.Domain.Households;
using FamilySafety.Domain.Profiles;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Infrastructure.Persistence;

public sealed class EfHouseholdRepository(FamilySafetyDbContext db, TimeProvider clock) : IHouseholdRepository
{
    private IQueryable<FamilyGroupMembership> Eligible(Guid family) => db.FamilyGroupMemberships.Where(m =>
        m.FamilyGroupId == family && m.IsActive && db.AdultProfiles.Any(p => p.Id == m.AdultProfileId && p.AccountId == m.AccountId));

    private Task<bool> IsAdmin(Guid account, Guid family, CancellationToken ct) => Eligible(family).AnyAsync(m =>
        m.AccountId == account && db.FamilyAdministratorAssignments.Any(a => a.MembershipId == m.Id), ct);

    private IQueryable<HouseholdAssignment> Assignments(Guid family) => db.HouseholdAssignments.Where(a =>
        a.FamilyGroupId == family && Eligible(family).Any(m => m.Id == a.MembershipId
            && m.AdultProfileId == a.ProfileId && m.AccessVersion == a.MembershipVersion));

    private async Task<HouseholdView> View(FamilyHousehold household, CancellationToken ct)
    {
        var members = await (from a in Assignments(household.FamilyGroupId).AsNoTracking()
                             join p in db.AdultProfiles.AsNoTracking() on a.ProfileId equals p.Id
                             where a.HouseholdId == household.Id
                             orderby p.DisplayName, p.Id
                             select new HouseholdMemberView(p.Id, p.DisplayName)).ToListAsync(ct);
        return new(household.Id, household.Name, household.Version, members);
    }

    public async Task<HouseholdStructureView?> ListAsync(Guid accountId, Guid familyId, CancellationToken ct)
    {
        // Keep membership authorization and this multi-query organization snapshot consistent.
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        if (!await Eligible(familyId).AnyAsync(m => m.AccountId == accountId, ct)) return null;
        var canManage = await IsAdmin(accountId, familyId, ct);
        var rows = await db.FamilyHouseholds.AsNoTracking().Where(h => h.FamilyGroupId == familyId)
            .OrderBy(h => h.Name).ThenBy(h => h.Id).ToListAsync(ct);
        var people = await (from m in Eligible(familyId).AsNoTracking()
                            join p in db.AdultProfiles.AsNoTracking() on m.AdultProfileId equals p.Id
                            orderby p.DisplayName, p.Id
                            select new HouseholdMemberView(p.Id, p.DisplayName)).ToListAsync(ct);
        var assignments = await Assignments(familyId).AsNoTracking().ToListAsync(ct);
        var byProfile = assignments.ToDictionary(a => a.ProfileId, a => a.HouseholdId);
        var views = rows.Select(row => new HouseholdView(row.Id, row.Name, row.Version,
            people.Where(p => byProfile.GetValueOrDefault(p.ProfileId) == row.Id).ToArray())).ToArray();
        var unassigned = people.Where(p => !byProfile.ContainsKey(p.ProfileId)).ToArray();
        await tx.CommitAsync(ct);
        return new(canManage, views, unassigned);
    }

    // Existing Family-before-membership/feature lock order; never trust tracked authorization.
    private async Task<bool> LockForAdmin(Guid account, Guid family, CancellationToken ct)
    {
        await db.FamilyGroups.Where(g => g.Id == family)
            .ExecuteUpdateAsync(u => u.SetProperty(g => g.CapacityLimit, g => g.CapacityLimit), ct);
        return await IsAdmin(account, family, ct);
    }

    private async Task<FamilyHousehold?> Row(Guid family, Guid household, Guid version, CancellationToken ct)
    {
        foreach (var e in db.ChangeTracker.Entries<FamilyHousehold>().Where(e => e.Entity.Id == household).ToList()) e.State = EntityState.Detached;
        var row = await db.FamilyHouseholds.SingleOrDefaultAsync(h => h.Id == household && h.FamilyGroupId == family, ct);
        if (row is not null && row.Version != version) throw new HouseholdConflictException();
        return row;
    }

    public async Task<HouseholdView?> CreateAsync(Guid accountId, Guid familyId, string name, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        if (!await LockForAdmin(accountId, familyId, ct)) return null;
        var fields = Name(name);
        var row = new FamilyHousehold(Guid.NewGuid(), familyId, fields.Display, fields.Normalized, clock.GetUtcNow());
        db.FamilyHouseholds.Add(row);
        await Save(ct);
        await tx.CommitAsync(ct);
        return new(row.Id, row.Name, row.Version, []);
    }

    public async Task<HouseholdView?> RenameAsync(Guid accountId, Guid familyId, Guid householdId, RenameHouseholdRequest request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        if (!await LockForAdmin(accountId, familyId, ct)) return null;
        var row = await Row(familyId, householdId, request.Version, ct);
        if (row is null) return null;
        var fields = Name(request.Name);
        row.Rename(fields.Display, fields.Normalized, clock.GetUtcNow());
        await Save(ct);
        var view = await View(row, ct);
        await tx.CommitAsync(ct);
        return view;
    }

    public async Task<bool> AssignAsync(Guid accountId, Guid familyId, Guid householdId, AssignHouseholdMemberRequest request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        if (!await LockForAdmin(accountId, familyId, ct)) return false;
        var target = await Row(familyId, householdId, request.Version, ct);
        if (target is null) return false;
        var member = await Eligible(familyId).AsNoTracking().SingleOrDefaultAsync(m => m.AdultProfileId == request.ProfileId, ct);
        if (member is null) return false;
        DetachAssignment(familyId, request.ProfileId);
        var assignment = await db.HouseholdAssignments.AsNoTracking().SingleOrDefaultAsync(a => a.FamilyGroupId == familyId && a.ProfileId == request.ProfileId, ct);
        if (assignment?.HouseholdId != request.CurrentHouseholdId) throw new HouseholdConflictException();
        if (assignment is not null)
        {
            if (assignment.HouseholdId == householdId || request.SourceVersion is null) throw new HouseholdConflictException();
            var source = await Row(familyId, assignment.HouseholdId, request.SourceVersion.Value, ct);
            if (source is null) throw new HouseholdConflictException();
            source.Touch(clock.GetUtcNow());
            // Load/refresh both principals before tracking the dependent; detaching a stale principal
            // can detach its tracked assignments through EF relationship fixup.
            db.HouseholdAssignments.Attach(assignment);
            assignment.MoveTo(householdId);
        }
        else
        {
            db.HouseholdAssignments.Add(new(familyId, householdId, member.AdultProfileId, member.Id, member.AccessVersion));
        }
        target.Touch(clock.GetUtcNow());
        await Save(ct);
        await tx.CommitAsync(ct);
        return true;
    }

    public async Task<bool> UnassignAsync(Guid accountId, Guid familyId, Guid householdId, Guid profileId, Guid version, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        if (!await LockForAdmin(accountId, familyId, ct)) return false;
        var row = await Row(familyId, householdId, version, ct);
        if (row is null) return false;
        DetachAssignment(familyId, profileId);
        var assignment = await db.HouseholdAssignments.SingleOrDefaultAsync(a => a.FamilyGroupId == familyId && a.HouseholdId == householdId && a.ProfileId == profileId, ct);
        if (assignment is null) return false;
        db.HouseholdAssignments.Remove(assignment);
        row.Touch(clock.GetUtcNow());
        await Save(ct);
        await tx.CommitAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid accountId, Guid familyId, Guid householdId, Guid version, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        if (!await LockForAdmin(accountId, familyId, ct)) return false;
        var row = await Row(familyId, householdId, version, ct);
        if (row is null) return false;
        foreach (var e in db.ChangeTracker.Entries<HouseholdAssignment>().Where(e => e.Entity.HouseholdId == householdId).ToList()) e.State = EntityState.Detached;
        db.FamilyHouseholds.Remove(row);
        await Save(ct);
        await tx.CommitAsync(ct);
        return true;
    }

    private void DetachAssignment(Guid family, Guid profile)
    {
        foreach (var e in db.ChangeTracker.Entries<HouseholdAssignment>().Where(e => e.Entity.FamilyGroupId == family && e.Entity.ProfileId == profile).ToList()) e.State = EntityState.Detached;
    }

    private static (string Display, string Normalized) Name(string? name)
    {
        var display = name?.Trim() ?? "";
        if (display.Length is < 1 or > 100 || display.Any(char.IsControl)) throw new HouseholdValidationException();
        string normalized;
        try { normalized = display.Normalize(NormalizationForm.FormKC).ToUpperInvariant(); }
        catch (ArgumentException) { throw new HouseholdValidationException(); }
        if (normalized.Length > 200) throw new HouseholdValidationException();
        return (display, normalized);
    }

    private async Task Save(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { throw new HouseholdConflictException(); }
        catch (DbUpdateException e) when (e.InnerException is SqlException { Number: 2601 or 2627 }) { throw new HouseholdDuplicateException(); }
    }
}
