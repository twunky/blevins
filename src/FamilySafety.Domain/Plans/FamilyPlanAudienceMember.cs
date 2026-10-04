namespace FamilySafety.Domain.Plans;

public sealed class FamilyPlanAudienceMember
{
    private FamilyPlanAudienceMember()
    {
    }

    public FamilyPlanAudienceMember(Guid planId, Guid profileId)
    {
        if (planId == Guid.Empty || profileId == Guid.Empty)
        {
            throw new ArgumentException("Plan audience identifiers are required.");
        }

        PlanId = planId;
        ProfileId = profileId;
    }

    public Guid PlanId { get; private set; }
    public Guid ProfileId { get; private set; }
}
