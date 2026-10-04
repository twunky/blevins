namespace FamilySafety.Domain.Concerns;

public sealed class Concern
{
    private Concern() { }

    public Concern(Guid id, Guid familyGroupId, Guid creatorAccountId, Guid creatorProfileId,
        Guid creatorMembershipId, int creatorMembershipAccessVersion, Guid subjectProfileId,
        string creationReason, DateTimeOffset createdAt)
    {
        if (id == Guid.Empty || familyGroupId == Guid.Empty || creatorAccountId == Guid.Empty
            || creatorProfileId == Guid.Empty || creatorMembershipId == Guid.Empty
            || subjectProfileId == Guid.Empty || creatorMembershipAccessVersion < 0)
            throw new ArgumentException("Concern identifiers and creator membership are required.");
        ArgumentException.ThrowIfNullOrWhiteSpace(creationReason);
        Id = id;
        FamilyGroupId = familyGroupId;
        CreatorAccountId = creatorAccountId;
        CreatorProfileId = creatorProfileId;
        CreatorMembershipId = creatorMembershipId;
        CreatorMembershipAccessVersion = creatorMembershipAccessVersion;
        SubjectProfileId = subjectProfileId;
        CreationReason = creationReason.Trim();
        CreatedAt = createdAt;
        LastAuthorizedMutationAt = createdAt;
        State = ConcernState.Open;
    }

    public Guid Id { get; private set; }
    public Guid FamilyGroupId { get; private set; }
    public Guid CreatorAccountId { get; private set; }
    public Guid CreatorProfileId { get; private set; }
    public Guid CreatorMembershipId { get; private set; }
    public int CreatorMembershipAccessVersion { get; private set; }
    public Guid SubjectProfileId { get; private set; }
    public string CreationReason { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastAuthorizedMutationAt { get; private set; }
    public ConcernState State { get; private set; }
    public DateTimeOffset? FrozenAt { get; private set; }
    public ConcernFreezeReason? FreezeReason { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }
    public DateTimeOffset? CreatorAccessRevokedAt { get; private set; }
    public Guid? ResolvedByAccountId { get; private set; }
    public string? ResolutionNote { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public DateTimeOffset? ExpiresAt => State switch
    {
        ConcernState.Frozen => FrozenAt?.AddDays(180),
        ConcernState.Resolved => ResolvedAt?.AddDays(180),
        _ => null,
    };

    public DateTimeOffset InactivityFreezeAt => LastAuthorizedMutationAt.AddDays(30);

    public bool IsEffectivelyFrozen(DateTimeOffset now, DateTimeOffset? creatorLostAt) =>
        State == ConcernState.Frozen
        || (State == ConcernState.Open
            && (now >= InactivityFreezeAt || creatorLostAt is not null));

    public DateTimeOffset? EffectiveFreezeAt(DateTimeOffset? creatorLostAt) =>
        State == ConcernState.Frozen ? FrozenAt
        : State != ConcernState.Open ? null
        : creatorLostAt is null ? InactivityFreezeAt
        : creatorLostAt < InactivityFreezeAt ? creatorLostAt : InactivityFreezeAt;

    public ConcernFreezeReason EffectiveFreezeReason(DateTimeOffset? creatorLostAt) =>
        creatorLostAt is not null && creatorLostAt < InactivityFreezeAt
            ? ConcernFreezeReason.CreatorEligibilityLost
            : ConcernFreezeReason.Inactivity;

    public bool IsExpired(DateTimeOffset now) => ExpiresAt is { } expiresAt && now >= expiresAt;

    public void RecordAuthorizedMutation(DateTimeOffset at)
    {
        EnsureOpen();
        LastAuthorizedMutationAt = at;
    }

    public void Freeze(DateTimeOffset at, ConcernFreezeReason reason)
    {
        if (State != ConcernState.Open) return;
        State = ConcernState.Frozen;
        FrozenAt = at;
        FreezeReason = reason;
    }

    public void Resolve(Guid actorAccountId, string note, DateTimeOffset at)
    {
        EnsureOpen();
        if (actorAccountId != CreatorAccountId)
            throw new InvalidOperationException("Only the creator may resolve a Concern.");
        ArgumentException.ThrowIfNullOrWhiteSpace(note);
        State = ConcernState.Resolved;
        ResolvedAt = at;
        ResolvedByAccountId = actorAccountId;
        ResolutionNote = note.Trim();
        LastAuthorizedMutationAt = at;
    }

    public bool RecordCreatorAccessRevoked(DateTimeOffset at)
    {
        if (CreatorAccessRevokedAt is not null) return false;
        CreatorAccessRevokedAt = at;
        return true;
    }

    public void EnsureOpen()
    {
        if (State != ConcernState.Open)
            throw new InvalidOperationException("The Concern is read-only.");
    }
}
