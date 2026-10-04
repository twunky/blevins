namespace FamilySafety.Domain.Concerns;

public sealed class ConcernParticipantGrant
{
    private ConcernParticipantGrant() { }

    public ConcernParticipantGrant(Guid concernId, Guid accountId, Guid profileId,
        Guid membershipId, int membershipAccessVersion, DateTimeOffset selectedAt)
    {
        if (concernId == Guid.Empty || accountId == Guid.Empty || profileId == Guid.Empty
            || membershipId == Guid.Empty || membershipAccessVersion < 0)
            throw new ArgumentException("Concern participant grant identifiers are required.");
        Id = Guid.NewGuid();
        ConcernId = concernId;
        AccountId = accountId;
        ProfileId = profileId;
        MembershipId = membershipId;
        MembershipAccessVersion = membershipAccessVersion;
        SelectedAt = selectedAt;
    }

    public Guid Id { get; private set; }
    public Guid ConcernId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid ProfileId { get; private set; }
    public Guid MembershipId { get; private set; }
    public int MembershipAccessVersion { get; private set; }
    public DateTimeOffset SelectedAt { get; private set; }
    public DateTimeOffset? RemovedAt { get; private set; }
    public bool EligibilityRevoked { get; private set; }
    public bool IsSelected => RemovedAt is null;

    public void Remove(DateTimeOffset at)
    {
        RemovedAt ??= at;
    }

    public void RevokeForEligibilityLoss(DateTimeOffset at)
    {
        if (RemovedAt is not null) return;
        RemovedAt = at;
        EligibilityRevoked = true;
    }
}
