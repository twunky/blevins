namespace FamilySafety.Domain.Profiles;

public sealed class FamilyGroupMembership
{
    private FamilyGroupMembership()
    {
    }

    public FamilyGroupMembership(Guid id, Guid accountId, Guid adultProfileId, Guid familyGroupId, bool isActive)
    {
        if (id == Guid.Empty || accountId == Guid.Empty || adultProfileId == Guid.Empty || familyGroupId == Guid.Empty)
        {
            throw new ArgumentException("Membership identifiers are required.");
        }

        Id = id;
        AccountId = accountId;
        AdultProfileId = adultProfileId;
        FamilyGroupId = familyGroupId;
        IsActive = isActive;
    }

    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }

    public Guid AdultProfileId { get; private set; }

    public Guid FamilyGroupId { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset? EndedAt { get; private set; }

    // Audience grants tied to this membership must not become valid again after rejoining.
    public int AccessVersion { get; private set; }

    public void End()
        => End(DateTimeOffset.UtcNow);

    public void End(DateTimeOffset endedAt)
    {
        if (!IsActive) return;
        IsActive = false;
        EndedAt = endedAt;
        AccessVersion++;
    }
}
