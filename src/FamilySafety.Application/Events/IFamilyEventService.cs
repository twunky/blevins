namespace FamilySafety.Application.Events;

public interface IFamilyEventService
{
    Task<IReadOnlyList<FamilyEventView>> GetVisibleAsync(
        Guid familyGroupId,
        CancellationToken cancellationToken);

    Task<FamilyEventView> GetAsync(
        Guid eventId,
        CancellationToken cancellationToken);

    Task<FamilyEventView> CreateAsync(
        Guid familyGroupId,
        CreateFamilyEventRequest request,
        CancellationToken cancellationToken);

    Task<FamilyEventView> UpdateAsync(
        Guid familyGroupId,
        Guid eventId,
        UpdateFamilyEventRequest request,
        CancellationToken cancellationToken);
}
