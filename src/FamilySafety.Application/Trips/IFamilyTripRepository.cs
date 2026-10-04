namespace FamilySafety.Application.Trips;

public interface IFamilyTripRepository
{
    Task<IReadOnlyList<FamilyTripView>> GetVisibleAsync(Guid accountId, CancellationToken cancellationToken);
    Task<FamilyTripView?> GetAsync(Guid accountId, Guid tripId, CancellationToken cancellationToken);
    Task<FamilyTripView?> CreateAsync(Guid accountId, Guid familyGroupId,
        CreateFamilyTripRequest request, CancellationToken cancellationToken);
    Task<FamilyTripView?> UpdateAsync(Guid accountId, Guid tripId,
        UpdateFamilyTripRequest request, CancellationToken cancellationToken);
    Task<TripAudiencePreview?> PreviewAudienceReplacementAsync(Guid accountId, Guid tripId,
        ReplaceTripAudienceRequest request, CancellationToken cancellationToken);
    Task<FamilyTripView?> ReplaceAudienceAsync(Guid accountId, Guid tripId,
        ReplaceTripAudienceRequest request, string previewToken, CancellationToken cancellationToken);
}
