namespace FamilySafety.Domain.CheckIns;

/// <summary>An immutable personal report, never a verified safety conclusion.</summary>
public sealed class CheckIn
{
    public const int NoteLimit = 1000;
    private CheckIn() { }
    public CheckIn(Guid id, Guid familyGroupId, Guid authorProfileId, Guid authorMembershipId,
        int authorMembershipVersion, CheckInStatus status, string? note, DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty || familyGroupId == Guid.Empty || authorProfileId == Guid.Empty || authorMembershipId == Guid.Empty)
            throw new ArgumentException("Check-In identifiers are required.");
        if (!Enum.IsDefined(status)) throw new ArgumentException("Choose a Check-In status.");
        if (note?.Trim().Length > NoteLimit) throw new ArgumentException("Check-In note is too long.");
        Id = id; FamilyGroupId = familyGroupId; AuthorProfileId = authorProfileId;
        AuthorMembershipId = authorMembershipId; AuthorMembershipVersion = authorMembershipVersion;
        Status = status; Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(); CreatedAtUtc = createdAtUtc;
    }
    public Guid Id { get; private set; }
    public Guid FamilyGroupId { get; private set; }
    public Guid AuthorProfileId { get; private set; }
    public Guid AuthorMembershipId { get; private set; }
    public int AuthorMembershipVersion { get; private set; }
    public CheckInStatus Status { get; private set; }
    public string? Note { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}

/// <summary>Exact profile/membership grant. Captured AccessVersion prevents rejoin reactivation.</summary>
public sealed class CheckInRecipient
{
    private CheckInRecipient() { }
    public CheckInRecipient(Guid checkInId, Guid profileId, Guid membershipId, int membershipVersion)
    {
        if (checkInId == Guid.Empty || profileId == Guid.Empty || membershipId == Guid.Empty)
            throw new ArgumentException("Recipient identifiers are required.");
        CheckInId = checkInId; ProfileId = profileId; MembershipId = membershipId; MembershipVersion = membershipVersion;
    }
    public Guid CheckInId { get; private set; }
    public Guid ProfileId { get; private set; }
    public Guid MembershipId { get; private set; }
    public int MembershipVersion { get; private set; }
    // The membership-loss trigger also records invalidation for operational membership changes.
    public bool IsInvalidated { get; private set; }
}
