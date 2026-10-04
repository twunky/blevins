namespace FamilySafety.Application.Plans;

public interface IFamilyPlanRepository
{
    Task<IReadOnlyList<FamilyPlanView>?> GetVisibleAsync(
        Guid accountId,
        Guid familyGroupId,
        CancellationToken cancellationToken);

    Task<FamilyPlanView?> GetAsync(
        Guid accountId,
        Guid planId,
        CancellationToken cancellationToken);

    Task<FamilyPlanView?> CreateAsync(
        Guid accountId,
        Guid familyGroupId,
        CreateFamilyPlanRequest request,
        CancellationToken cancellationToken);
}
