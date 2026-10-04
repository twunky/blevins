namespace FamilySafety.Domain.Events;

public sealed class FamilyEventAudienceMember
{
    private FamilyEventAudienceMember()
    {
    }

    public FamilyEventAudienceMember(Guid eventId, Guid profileId, bool canEdit)
    {
        if (eventId == Guid.Empty || profileId == Guid.Empty)
        {
            throw new ArgumentException("Event audience identifiers are required.");
        }

        EventId = eventId;
        ProfileId = profileId;
        CanEdit = canEdit;
    }

    public Guid EventId { get; private set; }

    public Guid ProfileId { get; private set; }

    public bool CanEdit { get; private set; }
}
