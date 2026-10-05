namespace FamilySafety.Domain.Sharing;

/// <summary>Self-initiated, fixed-audience sharing. Expiration is authoritative without a state job.</summary>
public sealed class SharingSession
{
    private SharingSession() { }
    public SharingSession(Guid id, Guid familyId, Guid authorProfileId, Guid membershipId,
        int membershipVersion, DateTimeOffset start, DateTimeOffset end)
    {
        if (id == Guid.Empty || familyId == Guid.Empty || authorProfileId == Guid.Empty || membershipId == Guid.Empty)
            throw new ArgumentException("Session identifiers are required.");
        if (end <= start || end > start.AddHours(24)) throw new ArgumentException("Choose an end time within 24 hours.");
        Id = id; FamilyGroupId = familyId; AuthorProfileId = authorProfileId;
        AuthorMembershipId = membershipId; AuthorMembershipVersion = membershipVersion;
        StartedAtUtc = start; EndsAtUtc = end;
    }
    public Guid Id { get; private set; }
    public Guid FamilyGroupId { get; private set; }
    public Guid AuthorProfileId { get; private set; }
    public Guid AuthorMembershipId { get; private set; }
    public int AuthorMembershipVersion { get; private set; }
    public DateTimeOffset StartedAtUtc { get; private set; }
    public DateTimeOffset EndsAtUtc { get; private set; }
    public DateTimeOffset? StoppedAtUtc { get; private set; }
    public int Version { get; private set; } = 1;
    public void Stop(DateTimeOffset now) { StoppedAtUtc = now; Version++; }
    public void LocationReplaced() => Version++;
}

public sealed class SharingSessionRecipient
{
    private SharingSessionRecipient() { }
    public SharingSessionRecipient(Guid sessionId, Guid profileId, Guid membershipId, int membershipVersion)
    { SharingSessionId = sessionId; ProfileId = profileId; MembershipId = membershipId; MembershipVersion = membershipVersion; }
    public Guid SharingSessionId { get; private set; }
    public Guid ProfileId { get; private set; }
    public Guid MembershipId { get; private set; }
    public int MembershipVersion { get; private set; }
    public bool IsInvalidated { get; private set; }
}

/// <summary>One row per session, replaced in place; no coordinate history.</summary>
public sealed class SharingCurrentLocation
{
    private SharingCurrentLocation() { }
    public SharingCurrentLocation(Guid sessionId) => SharingSessionId = sessionId;
    public Guid SharingSessionId { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public double Accuracy { get; private set; }
    public DateTimeOffset CapturedAtUtc { get; private set; }
    public DateTimeOffset ReceivedAtUtc { get; private set; }
    public void Replace(double latitude, double longitude, double accuracy, DateTimeOffset captured, DateTimeOffset received)
    { Latitude = latitude; Longitude = longitude; Accuracy = accuracy; CapturedAtUtc = captured; ReceivedAtUtc = received; }
}
