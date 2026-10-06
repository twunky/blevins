namespace FamilySafety.Application.Plans;

public interface IFamilyPlanService
{
    Task<FamilyPlanView> CorrectActivityTimingAsync(
        Guid planId,
        CorrectFamilyPlanActivityTimingRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<FamilyPlanView>> GetVisibleAsync(
        Guid familyGroupId,
        CancellationToken cancellationToken);

    Task<FamilyPlanView> GetAsync(
        Guid planId,
        CancellationToken cancellationToken);

    Task<FamilyPlanView> CreateAsync(
        Guid familyGroupId,
        CreateFamilyPlanRequest request,
        CancellationToken cancellationToken);

    Task<FamilyPlanView> UpdateNotesAsync(
        Guid planId,
        UpdateFamilyPlanNotesRequest request,
        CancellationToken cancellationToken);
}
