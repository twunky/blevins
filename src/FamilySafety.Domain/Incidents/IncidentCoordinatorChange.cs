namespace FamilySafety.Domain.Incidents;

public sealed class IncidentCoordinatorChange
{
    private IncidentCoordinatorChange() { }

    public IncidentCoordinatorChange(Guid incidentId, Guid fromGrantId, Guid fromProfileId,
        Guid toGrantId, Guid toProfileId, Guid changedByAccountId, DateTimeOffset changedAt)
    {
        if (incidentId == Guid.Empty || fromGrantId == Guid.Empty || fromProfileId == Guid.Empty
            || toGrantId == Guid.Empty || toProfileId == Guid.Empty || changedByAccountId == Guid.Empty)
            throw new ArgumentException("Coordinator change identifiers are required.");
        if (fromGrantId == toGrantId || fromProfileId == toProfileId)
            throw new ArgumentException("Coordination must move to a different participant.");
        Id = Guid.NewGuid();
        IncidentId = incidentId;
        FromGrantId = fromGrantId;
        FromProfileId = fromProfileId;
        ToGrantId = toGrantId;
        ToProfileId = toProfileId;
        ChangedByAccountId = changedByAccountId;
        ChangedAt = changedAt;
    }

    public Guid Id { get; private set; }
    public Guid IncidentId { get; private set; }
    public Guid FromGrantId { get; private set; }
    public Guid FromProfileId { get; private set; }
    public Guid ToGrantId { get; private set; }
    public Guid ToProfileId { get; private set; }
    public Guid ChangedByAccountId { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
}
