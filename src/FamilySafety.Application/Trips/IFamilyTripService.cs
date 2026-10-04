namespace FamilySafety.Application.Trips;

public interface IFamilyTripService
{
    Task<IReadOnlyList<FamilyTripView>> GetVisibleAsync(CancellationToken cancellationToken);
    Task<FamilyTripView> GetAsync(Guid tripId, CancellationToken cancellationToken);
    Task<FamilyTripView> CreateAsync(Guid familyGroupId, CreateFamilyTripRequest request,
        CancellationToken cancellationToken);
    Task<FamilyTripView> UpdateAsync(Guid tripId, UpdateFamilyTripRequest request,
        CancellationToken cancellationToken);
    Task<TripAudiencePreview> PreviewAudienceReplacementAsync(Guid tripId,
        ReplaceTripAudienceRequest request, CancellationToken cancellationToken);
    Task<FamilyTripView> ReplaceAudienceAsync(Guid tripId,
        ReplaceTripAudienceRequest request, string previewToken, CancellationToken cancellationToken);
}
