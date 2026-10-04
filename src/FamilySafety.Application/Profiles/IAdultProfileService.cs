namespace FamilySafety.Application.Profiles;

public interface IAdultProfileService
{
    Task<AdultProfileOwnerView> GetCurrentAsync(CancellationToken cancellationToken);

    Task<AdultProfileOwnerView> UpdateCurrentAsync(
        UpdateAdultProfileRequest request,
        CancellationToken cancellationToken);

    Task<AdultProfileSharedView?> GetSharedAsync(Guid profileId, CancellationToken cancellationToken);
}
