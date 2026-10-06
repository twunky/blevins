namespace FamilySafety.Domain.Households;

public sealed class FamilyHousehold
{
    private FamilyHousehold() { }

    public FamilyHousehold(Guid id, Guid familyGroupId, string name, string normalizedName, DateTimeOffset now)
    {
        Id = id;
        FamilyGroupId = familyGroupId;
        Name = name;
        NormalizedName = normalizedName;
        CreatedAtUtc = UpdatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public Guid FamilyGroupId { get; private set; }
    public string Name { get; private set; } = "";
    public string NormalizedName { get; private set; } = "";
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public Guid Version { get; private set; } = Guid.NewGuid();

    public void Rename(string name, string normalizedName, DateTimeOffset now)
    {
        Name = name;
        NormalizedName = normalizedName;
        Touch(now);
    }

    public void Touch(DateTimeOffset now)
    {
        UpdatedAtUtc = now;
        Version = Guid.NewGuid();
    }
}

public sealed class HouseholdAssignment
{
    private HouseholdAssignment() { }

    public HouseholdAssignment(Guid familyGroupId, Guid householdId, Guid profileId, Guid membershipId, int membershipVersion)
    {
        FamilyGroupId = familyGroupId;
        HouseholdId = householdId;
        ProfileId = profileId;
        MembershipId = membershipId;
        MembershipVersion = membershipVersion;
    }

    public Guid FamilyGroupId { get; private set; }
    public Guid HouseholdId { get; private set; }
    public Guid ProfileId { get; private set; }
    public Guid MembershipId { get; private set; }
    public int MembershipVersion { get; private set; }

    public void MoveTo(Guid householdId) => HouseholdId = householdId;
}
