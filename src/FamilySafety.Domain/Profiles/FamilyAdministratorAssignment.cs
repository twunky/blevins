namespace FamilySafety.Domain.Profiles;

public sealed class FamilyAdministratorAssignment
{
    private FamilyAdministratorAssignment()
    {
    }

    public FamilyAdministratorAssignment(Guid id, Guid membershipId)
    {
        if (id == Guid.Empty || membershipId == Guid.Empty)
        {
            throw new ArgumentException("Administrator assignment identifiers are required.");
        }

        Id = id;
        MembershipId = membershipId;
    }

    public Guid Id { get; private set; }

    public Guid MembershipId { get; private set; }
}
