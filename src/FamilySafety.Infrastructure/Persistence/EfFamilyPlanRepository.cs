using System.Data;
using FamilySafety.Application.Plans;
using FamilySafety.Domain.Plans;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Infrastructure.Persistence;

public sealed class EfFamilyPlanRepository(FamilySafetyDbContext dbContext) : IFamilyPlanRepository
{
    public async Task<IReadOnlyList<FamilyPlanView>?> GetVisibleAsync(
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

        var plans = await QueryVisiblePlans(accountId)
            .Where(row => row.Plan.FamilyGroupId == familyGroupId)
            .OrderBy(row => row.Plan.Date)
            .ThenBy(row => row.Plan.Time)
            .ThenBy(row => row.Plan.ActivityOrDestination)
            .ToListAsync(cancellationToken);
        return plans.Select(row => ToView(row.Plan, row.CreatorDisplayName)).ToArray();
    }

    public async Task<FamilyPlanView?> GetAsync(
        Guid accountId,
        Guid planId,
        CancellationToken cancellationToken)
    {
        // One query for every outcome: a missing Plan and a Plan the caller may not
        // read both return null, so the caller cannot tell them apart.
        var row = await QueryVisiblePlans(accountId)
            .Where(item => item.Plan.Id == planId)
            .SingleOrDefaultAsync(cancellationToken);
        return row is null ? null : ToView(row.Plan, row.CreatorDisplayName);
    }

    public async Task<FamilyPlanView?> CreateAsync(
        Guid accountId,
        Guid familyGroupId,
        CreateFamilyPlanRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        await LockFamilyGroupAsync(familyGroupId, cancellationToken);

        var creatorProfileId = await GetActiveProfileIdAsync(
            accountId, familyGroupId, cancellationToken);
        if (creatorProfileId is null
            || request.AudienceProfileIds.Contains(creatorProfileId.Value)
            || (request.AudienceType == PlanAudienceType.FamilyGroup
                && request.AudienceProfileIds.Count != 0)
            || (request.AudienceType == PlanAudienceType.SelectedPeople
                && !await AreActiveAudienceMembersAsync(
                    familyGroupId, request.AudienceProfileIds, cancellationToken)))
        {
            return null;
        }

        var planId = Guid.NewGuid();
        var plan = new FamilyPlan(
            planId,
            familyGroupId,
            creatorProfileId.Value,
            request.ActivityOrDestination,
            request.Date!.Value,
            request.Time!.Value,
            request.ExpectedReturnDate,
            request.ExpectedReturnTime,
            request.ExpectedCheckInDate,
            request.ExpectedCheckInTime,
            request.AudienceType,
            request.Notes);

        dbContext.FamilyPlans.Add(plan);
        if (request.AudienceType == PlanAudienceType.SelectedPeople)
        {
            dbContext.FamilyPlanAudienceMembers.AddRange(request.AudienceProfileIds.Select(
                profileId => new FamilyPlanAudienceMember(planId, profileId)));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await GetPlanViewAsync(planId, cancellationToken);
    }

    public async Task<FamilyPlanView?> UpdateNotesAsync(
        Guid accountId,
        Guid planId,
        string? notes,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);

        // Resolve the Plan and current creator membership together for every write.
        // Missing, non-creator, and inactive-creator requests share the same result.
        var plan = await (from item in dbContext.FamilyPlans
                          join membership in dbContext.FamilyGroupMemberships
                              on item.FamilyGroupId equals membership.FamilyGroupId
                          where item.Id == planId
                              && item.CreatorProfileId == membership.AdultProfileId
                              && membership.AccountId == accountId
                              && membership.IsActive
                          select item).SingleOrDefaultAsync(cancellationToken);
        if (plan is null)
        {
            return null;
        }

        plan.UpdateNotes(notes);
        await dbContext.SaveChangesAsync(cancellationToken);
        var updated = await GetPlanViewAsync(planId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return updated;
    }

    // The only definition of Plan read visibility (REQ-PLAN-005): the caller has an
    // active membership in the Plan's Family and is its creator, or the Plan is
    // shared with the whole Family, or the caller was individually selected. A
    // creator who leaves the Family loses access. Family Administrator status grants
    // nothing. The list and the item read both use it.
    private IQueryable<VisiblePlanRow> QueryVisiblePlans(Guid accountId) =>
        from membership in dbContext.FamilyGroupMemberships.AsNoTracking()
        where membership.AccountId == accountId && membership.IsActive
        join plan in dbContext.FamilyPlans.AsNoTracking()
            on membership.FamilyGroupId equals plan.FamilyGroupId
        join creator in dbContext.AdultProfiles.AsNoTracking()
            on plan.CreatorProfileId equals creator.Id
        where plan.CreatorProfileId == membership.AdultProfileId
            || plan.AudienceType == PlanAudienceType.FamilyGroup
            || dbContext.FamilyPlanAudienceMembers.Any(audienceMember =>
                audienceMember.PlanId == plan.Id
                && audienceMember.ProfileId == membership.AdultProfileId)
        select new VisiblePlanRow { Plan = plan, CreatorDisplayName = creator.DisplayName };

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

    private Task<FamilyPlanView?> GetPlanViewAsync(
        Guid planId,
        CancellationToken cancellationToken) =>
        (from plan in dbContext.FamilyPlans.AsNoTracking()
         join creator in dbContext.AdultProfiles.AsNoTracking()
             on plan.CreatorProfileId equals creator.Id
         where plan.Id == planId
         select ToView(plan, creator.DisplayName))
        .SingleOrDefaultAsync(cancellationToken);

    private static FamilyPlanView ToView(FamilyPlan plan, string creatorDisplayName) => new(
        plan.Id,
        plan.FamilyGroupId,
        plan.ActivityOrDestination,
        plan.Date,
        plan.Time,
        plan.ExpectedReturnDate,
        plan.ExpectedReturnTime,
        plan.ExpectedCheckInDate,
        plan.ExpectedCheckInTime,
        plan.AudienceType,
        plan.CreatorProfileId,
        creatorDisplayName,
        plan.Notes);

    private Task<int> LockFamilyGroupAsync(Guid familyGroupId, CancellationToken cancellationToken) =>
        dbContext.FamilyGroups
            .Where(group => group.Id == familyGroupId)
            .ExecuteUpdateAsync(
                updates => updates.SetProperty(group => group.CapacityLimit, group => group.CapacityLimit),
                cancellationToken);

    // A query-side row with settable members, so EF Core can still filter and order
    // on its properties after the visibility projection.
    private sealed class VisiblePlanRow
    {
        public required FamilyPlan Plan { get; init; }
        public required string CreatorDisplayName { get; init; }
    }
}
