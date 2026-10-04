namespace FamilySafety.Domain.Incidents;

/// <summary>
/// The only source of Incident access, including the coordinator's. A grant is valid only while
/// its captured membership row is active at the captured access version.
/// </summary>
public sealed class IncidentParticipantGrant
{
    private IncidentParticipantGrant() { }

    public IncidentParticipantGrant(Guid incidentId, Guid accountId, Guid profileId,
        Guid membershipId, int membershipAccessVersion, DateTimeOffset selectedAt, Guid? addedByProfileId)
    {
        if (incidentId == Guid.Empty || accountId == Guid.Empty || profileId == Guid.Empty
            || membershipId == Guid.Empty || membershipAccessVersion < 0 || addedByProfileId == Guid.Empty)
            throw new ArgumentException("Incident participant grant identifiers are required.");
        Id = Guid.NewGuid();
        IncidentId = incidentId;
        AccountId = accountId;
        ProfileId = profileId;
        MembershipId = membershipId;
        MembershipAccessVersion = membershipAccessVersion;
        SelectedAt = selectedAt;
        AddedByProfileId = addedByProfileId;
    }

    public Guid Id { get; private set; }
    public Guid IncidentId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid ProfileId { get; private set; }
    public Guid MembershipId { get; private set; }
    public int MembershipAccessVersion { get; private set; }
    public DateTimeOffset SelectedAt { get; private set; }

    // Null for the creator's initial coordinator grant.
    public Guid? AddedByProfileId { get; private set; }
    public DateTimeOffset? RemovedAt { get; private set; }
    public Guid? RemovedByProfileId { get; private set; }
    public bool EligibilityRevoked { get; private set; }
    public bool IsCurrent => RemovedAt is null;

    public bool MatchesMembership(Guid membershipId, Guid accountId, bool isActive, int accessVersion) =>
        IsCurrent && isActive && membershipId == MembershipId && accountId == AccountId
        && accessVersion == MembershipAccessVersion;

    public void Remove(Guid removedByProfileId, DateTimeOffset at)
    {
        if (removedByProfileId == Guid.Empty) throw new ArgumentException("The remover is required.");
        if (RemovedAt is not null) return;
        RemovedAt = at;
        RemovedByProfileId = removedByProfileId;
    }

    /// <summary>Records system revocation once. Returns false when the grant had already ended.</summary>
    public bool RevokeForEligibilityLoss(DateTimeOffset at)
    {
        if (RemovedAt is not null) return false;
        RemovedAt = at;
        EligibilityRevoked = true;
        return true;
    }
}
