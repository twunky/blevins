namespace FamilySafety.Application.Events;

public interface IFamilyEventRepository
{
    Task<IReadOnlyList<FamilyEventView>?> GetVisibleAsync(
        Guid accountId,
        Guid familyGroupId,
        CancellationToken cancellationToken);

    Task<FamilyEventView?> GetAsync(
        Guid accountId,
        Guid eventId,
        CancellationToken cancellationToken);

    Task<FamilyEventView?> CreateAsync(
        Guid accountId,
        Guid familyGroupId,
        CreateFamilyEventRequest request,
        CancellationToken cancellationToken);

    Task<FamilyEventView?> UpdateAsync(
        Guid accountId,
        Guid familyGroupId,
        Guid eventId,
        UpdateFamilyEventRequest request,
        CancellationToken cancellationToken);
}
