namespace FamilySafety.Domain.Trips;

public sealed class FamilyTripAudienceMember
{
    private FamilyTripAudienceMember() { }
    public FamilyTripAudienceMember(Guid tripId, Guid profileId, int membershipAccessVersion)
    {
        if (tripId == Guid.Empty || profileId == Guid.Empty || membershipAccessVersion < 0)
            throw new ArgumentException("Audience membership is invalid.");
        TripId = tripId;
        ProfileId = profileId;
        MembershipAccessVersion = membershipAccessVersion;
    }
    public Guid TripId { get; private set; }
    public Guid ProfileId { get; private set; }
    public int MembershipAccessVersion { get; private set; }

    public void RefreshMembershipAccessVersion(int membershipAccessVersion)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(membershipAccessVersion);
        MembershipAccessVersion = membershipAccessVersion;
    }
}
