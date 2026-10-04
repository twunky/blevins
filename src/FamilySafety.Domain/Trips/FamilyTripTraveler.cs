namespace FamilySafety.Domain.Trips;

public sealed class FamilyTripTraveler
{
    private FamilyTripTraveler() { }
    public FamilyTripTraveler(Guid tripId, Guid profileId)
    {
        if (tripId == Guid.Empty || profileId == Guid.Empty) throw new ArgumentException("Traveler identifiers are required.");
        TripId = tripId;
        ProfileId = profileId;
    }
    public Guid TripId { get; private set; }
    public Guid ProfileId { get; private set; }
}
