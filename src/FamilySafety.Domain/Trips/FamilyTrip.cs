namespace FamilySafety.Domain.Trips;

public sealed class FamilyTrip
{
    private FamilyTrip() { }

    public FamilyTrip(Guid id, Guid familyGroupId, Guid creatorProfileId,
        TripAudienceType audienceType, string title, string primaryDestination,
        DateOnly startDate, DateOnly endDate, string? description,
        string? transportationNotes, string? accommodationNotes,
        DateOnly? expectedArrivalDate, TimeOnly? expectedArrivalTime,
        string? expectedArrivalTimeZone)
    {
        if (id == Guid.Empty || familyGroupId == Guid.Empty || creatorProfileId == Guid.Empty)
            throw new ArgumentException("Trip identifiers are required.");
        if (!Enum.IsDefined(audienceType)) throw new ArgumentOutOfRangeException(nameof(audienceType));

        Id = id;
        FamilyGroupId = familyGroupId;
        CreatorProfileId = creatorProfileId;
        AudienceType = audienceType;
        Update(title, primaryDestination, startDate, endDate, description,
            transportationNotes, accommodationNotes, expectedArrivalDate,
            expectedArrivalTime, expectedArrivalTimeZone);
    }

    public Guid Id { get; private set; }
    public Guid FamilyGroupId { get; private set; }
    public Guid CreatorProfileId { get; private set; }
    public TripAudienceType AudienceType { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string PrimaryDestination { get; private set; } = string.Empty;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public string? Description { get; private set; }
    public string? TransportationNotes { get; private set; }
    public string? AccommodationNotes { get; private set; }
    public DateOnly? ExpectedArrivalDate { get; private set; }
    public TimeOnly? ExpectedArrivalTime { get; private set; }
    public string? ExpectedArrivalTimeZone { get; private set; }
    public bool IsCompleted { get; private set; }
    public Guid Version { get; private set; } = Guid.NewGuid();

    public void Complete()
    {
        if (IsCompleted) throw new InvalidOperationException("The Trip is already completed.");
        IsCompleted = true;
        Version = Guid.NewGuid();
    }

    public void ReplaceAudience(TripAudienceType audienceType)
    {
        if (!Enum.IsDefined(audienceType)) throw new ArgumentOutOfRangeException(nameof(audienceType));
        AudienceType = audienceType;
        Version = Guid.NewGuid();
    }

    public void Update(string title, string primaryDestination, DateOnly startDate,
        DateOnly endDate, string? description, string? transportationNotes,
        string? accommodationNotes, DateOnly? expectedArrivalDate,
        TimeOnly? expectedArrivalTime, string? expectedArrivalTimeZone)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(primaryDestination);
        if (endDate < startDate) throw new ArgumentException("End date cannot precede start date.");
        if (expectedArrivalDate.HasValue != expectedArrivalTime.HasValue
            || expectedArrivalDate.HasValue != !string.IsNullOrWhiteSpace(expectedArrivalTimeZone))
            throw new ArgumentException("Expected arrival requires date, time, and time zone together.");

        Title = title.Trim();
        PrimaryDestination = primaryDestination.Trim();
        StartDate = startDate;
        EndDate = endDate;
        Description = NormalizeOptional(description);
        TransportationNotes = NormalizeOptional(transportationNotes);
        AccommodationNotes = NormalizeOptional(accommodationNotes);
        ExpectedArrivalDate = expectedArrivalDate;
        ExpectedArrivalTime = expectedArrivalTime;
        ExpectedArrivalTimeZone = NormalizeOptional(expectedArrivalTimeZone);
        Version = Guid.NewGuid();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
