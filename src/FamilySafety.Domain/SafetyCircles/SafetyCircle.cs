namespace FamilySafety.Domain.SafetyCircles;

public sealed class SafetyCircle
{
    private SafetyCircle() { }
    public SafetyCircle(Guid id, Guid ownerProfileId, Guid familyGroupId, Guid ownerMembershipId,
        int ownerMembershipVersion, string name, string normalizedName, DateTimeOffset now)
    {
        Id = id; OwnerProfileId = ownerProfileId; FamilyGroupId = familyGroupId;
        OwnerMembershipId = ownerMembershipId; OwnerMembershipVersion = ownerMembershipVersion;
        Name = name; NormalizedName = normalizedName; CreatedAtUtc = UpdatedAtUtc = now;
    }
    public Guid Id { get; private set; }
    public Guid OwnerProfileId { get; private set; }
    public Guid FamilyGroupId { get; private set; }
    public Guid OwnerMembershipId { get; private set; }
    public int OwnerMembershipVersion { get; private set; }
    public string Name { get; private set; } = "";
    public string NormalizedName { get; private set; } = "";
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public Guid Version { get; private set; } = Guid.NewGuid();
    public void Rename(string name, string normalizedName, DateTimeOffset now)
    { Name = name; NormalizedName = normalizedName; Touch(now); }
    public void Touch(DateTimeOffset now) { UpdatedAtUtc = now; Version = Guid.NewGuid(); }
}

public sealed class SafetyCircleMember
{
    private SafetyCircleMember() { }
    public SafetyCircleMember(Guid circleId, Guid profileId, Guid membershipId, int membershipVersion)
    { SafetyCircleId = circleId; MemberProfileId = profileId; MembershipId = membershipId; MembershipVersion = membershipVersion; }
    public Guid SafetyCircleId { get; private set; }
    public Guid MemberProfileId { get; private set; }
    public Guid MembershipId { get; private set; }
    public int MembershipVersion { get; private set; }
}
