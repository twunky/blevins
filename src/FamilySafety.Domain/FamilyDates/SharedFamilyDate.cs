namespace FamilySafety.Domain.FamilyDates;

public sealed class SharedFamilyDate
{
    public const int TitleLimit = 100;
    public const int DescriptionLimit = 1000;
    private SharedFamilyDate() { }
    public SharedFamilyDate(Guid id, Guid familyGroupId, Guid creatorProfileId, Guid creatorMembershipId,
        int creatorMembershipVersion, string title, string description, DateOnly date, DateTimeOffset now)
    {
        if (id == Guid.Empty || familyGroupId == Guid.Empty || creatorProfileId == Guid.Empty || creatorMembershipId == Guid.Empty)
            throw new ArgumentException("Date identifiers are required.");
        Id = id; FamilyGroupId = familyGroupId; CreatorProfileId = creatorProfileId;
        CreatorMembershipId = creatorMembershipId; CreatorMembershipVersion = creatorMembershipVersion;
        SetContent(title, description, date); CreatedAtUtc = now; Version = Guid.NewGuid();
    }
    public Guid Id { get; private set; }
    public Guid FamilyGroupId { get; private set; }
    public Guid CreatorProfileId { get; private set; }
    public Guid CreatorMembershipId { get; private set; }
    public int CreatorMembershipVersion { get; private set; }
    public string Title { get; private set; } = "";
    public string Description { get; private set; } = "";
    public DateOnly Date { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    public DateTimeOffset? UnavailableAtUtc { get; private set; }
    public DateTimeOffset? RetainUntilUtc { get; private set; }
    public Guid Version { get; private set; }
    public void Edit(string title, string description, DateOnly date, DateTimeOffset now)
    {
        if (UnavailableAtUtc is not null) throw new InvalidOperationException("Date unavailable.");
        SetContent(title, description, date); UpdatedAtUtc = now; Version = Guid.NewGuid();
    }
    public void MakeUnavailable(DateTimeOffset now)
    {
        if (UnavailableAtUtc is not null) return;
        UnavailableAtUtc = now; RetainUntilUtc = now.AddDays(30); Version = Guid.NewGuid();
    }
    private void SetContent(string title, string description, DateOnly date)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > TitleLimit
            || string.IsNullOrWhiteSpace(description) || description.Trim().Length > DescriptionLimit || date == default)
            throw new ArgumentException("A title, description and calendar date are required.");
        Title = title.Trim(); Description = description.Trim(); Date = date;
    }
}

// Membership identity/version prevents a later membership from reviving an old association.
public sealed class SharedFamilyDateAssociation
{
    private SharedFamilyDateAssociation() { }
    public SharedFamilyDateAssociation(Guid dateId, Guid profileId, Guid membershipId, int membershipVersion)
    { DateId = dateId; ProfileId = profileId; MembershipId = membershipId; MembershipVersion = membershipVersion; }
    public Guid DateId { get; private set; }
    public Guid ProfileId { get; private set; }
    public Guid MembershipId { get; private set; }
    public int MembershipVersion { get; private set; }
}
