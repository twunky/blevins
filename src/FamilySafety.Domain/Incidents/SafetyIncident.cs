namespace FamilySafety.Domain.Incidents;

public sealed class SafetyIncident
{
    public const int InactivityFreezeDays = 30;
    public const int RetentionDays = 365;

    private SafetyIncident() { }

    public SafetyIncident(Guid id, Guid familyGroupId, Guid createdByAccountId, Guid createdByProfileId,
        Guid subjectProfileId, Guid coordinatorGrantId, Guid? originatingConcernId, DateTimeOffset createdAt)
    {
        if (id == Guid.Empty || familyGroupId == Guid.Empty || createdByAccountId == Guid.Empty
            || createdByProfileId == Guid.Empty || subjectProfileId == Guid.Empty
            || coordinatorGrantId == Guid.Empty || originatingConcernId == Guid.Empty)
            throw new ArgumentException("Incident identifiers and the coordinator grant are required.");
        Id = id;
        FamilyGroupId = familyGroupId;
        CreatedByAccountId = createdByAccountId;
        CreatedByProfileId = createdByProfileId;
        SubjectProfileId = subjectProfileId;
        CoordinatorGrantId = coordinatorGrantId;
        OriginatingConcernId = originatingConcernId;
        CreatedAt = createdAt;
        LastAuthorizedMutationAt = createdAt;
        State = IncidentState.Open;
    }

    public Guid Id { get; private set; }
    public Guid FamilyGroupId { get; private set; }

    // Attribution only: the creator holds no authority beyond their current grant.
    public Guid CreatedByAccountId { get; private set; }
    public Guid CreatedByProfileId { get; private set; }
    public Guid SubjectProfileId { get; private set; }

    // Deliberately not a database foreign key, so whole-aggregate deletion has no cycle.
    public Guid CoordinatorGrantId { get; private set; }

    // Non-authorizing lineage reference; deliberately not a foreign key to Concerns.
    public Guid? OriginatingConcernId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastAuthorizedMutationAt { get; private set; }
    public IncidentState State { get; private set; }
    public DateTimeOffset? FrozenAt { get; private set; }
    public IncidentFreezeReason? FreezeReason { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }
    public Guid? ResolvedByAccountId { get; private set; }
    public Guid? ResolvedByProfileId { get; private set; }
    public string? ResolutionSummary { get; private set; }
    public string? OutcomeText { get; private set; }
    public IncidentInformationCategory? OutcomeCategory { get; private set; }
    public string? OutcomeSourceDescription { get; private set; }
    public bool OutcomeSourceUnknown { get; private set; }
    public DateTimeOffset? ContactRestoredAt { get; private set; }
    public bool ContactRestoredTimeUnknown { get; private set; }
    public string? AssistanceProvided { get; private set; }
    public string? UnresolvedFollowUp { get; private set; }
    public string? Lessons { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public DateTimeOffset? ExpiresAt => State switch
    {
        IncidentState.Frozen => FrozenAt?.AddDays(RetentionDays),
        IncidentState.Resolved => ResolvedAt?.AddDays(RetentionDays),
        _ => null,
    };

    public DateTimeOffset InactivityFreezeAt => LastAuthorizedMutationAt.AddDays(InactivityFreezeDays);

    public bool IsEffectivelyFrozen(DateTimeOffset now, DateTimeOffset? coordinatorLostAt) =>
        State == IncidentState.Frozen
        || (State == IncidentState.Open
            && (now >= InactivityFreezeAt || coordinatorLostAt is not null));

    public DateTimeOffset? EffectiveFreezeAt(DateTimeOffset? coordinatorLostAt) =>
        State == IncidentState.Frozen ? FrozenAt
        : State != IncidentState.Open ? null
        : coordinatorLostAt is null ? InactivityFreezeAt
        : coordinatorLostAt < InactivityFreezeAt ? coordinatorLostAt : InactivityFreezeAt;

    public IncidentFreezeReason EffectiveFreezeReason(DateTimeOffset? coordinatorLostAt) =>
        coordinatorLostAt is not null && coordinatorLostAt < InactivityFreezeAt
            ? IncidentFreezeReason.CoordinatorEligibilityLost
            : IncidentFreezeReason.Inactivity;

    public bool IsExpired(DateTimeOffset now) => ExpiresAt is { } expiresAt && now >= expiresAt;

    public void RecordAuthorizedMutation(DateTimeOffset at)
    {
        EnsureOpen();
        LastAuthorizedMutationAt = at;
    }

    /// <summary>Freezes an Open Incident once. Returns false when it was not Open.</summary>
    public bool Freeze(DateTimeOffset at, IncidentFreezeReason reason)
    {
        if (State != IncidentState.Open) return false;
        if (!Enum.IsDefined(reason)) throw new ArgumentOutOfRangeException(nameof(reason));
        State = IncidentState.Frozen;
        FrozenAt = at;
        FreezeReason = reason;
        return true;
    }

    public void TransferCoordinator(Guid newCoordinatorGrantId, DateTimeOffset at)
    {
        EnsureOpen();
        if (newCoordinatorGrantId == Guid.Empty || newCoordinatorGrantId == CoordinatorGrantId)
            throw new ArgumentException("Coordination must move to a different participant grant.");
        CoordinatorGrantId = newCoordinatorGrantId;
        LastAuthorizedMutationAt = at;
    }

    public void Resolve(Guid actorAccountId, Guid actorProfileId, IncidentResolution resolution,
        DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(resolution);
        EnsureOpen();
        if (actorAccountId == Guid.Empty || actorProfileId == Guid.Empty)
            throw new ArgumentException("The resolver is required.");
        State = IncidentState.Resolved;
        ResolvedAt = at;
        ResolvedByAccountId = actorAccountId;
        ResolvedByProfileId = actorProfileId;
        ResolutionSummary = resolution.Summary;
        OutcomeText = resolution.OutcomeText;
        OutcomeCategory = resolution.OutcomeCategory;
        OutcomeSourceDescription = resolution.OutcomeSourceDescription;
        OutcomeSourceUnknown = resolution.OutcomeSourceUnknown;
        ContactRestoredAt = resolution.ContactRestoredAt;
        ContactRestoredTimeUnknown = resolution.ContactRestoredTimeUnknown;
        AssistanceProvided = resolution.AssistanceProvided;
        UnresolvedFollowUp = resolution.UnresolvedFollowUp;
        Lessons = resolution.Lessons;
        LastAuthorizedMutationAt = at;
    }

    public void EnsureOpen()
    {
        if (State != IncidentState.Open)
            throw new InvalidOperationException("The Incident is read-only.");
    }
}
