namespace FamilySafety.Domain.Concerns;

public sealed class ConcernAuditEvent
{
    private ConcernAuditEvent() { }

    public ConcernAuditEvent(Guid id, Guid? concernId, Guid? actorAccountId,
        string action, DateTimeOffset occurredAt, bool authorized, Guid? affectedParticipantId = null)
    {
        if (id == Guid.Empty) throw new ArgumentException("Audit identifier is required.");
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        Id = id;
        ConcernId = concernId;
        ActorAccountId = actorAccountId;
        Action = action;
        OccurredAt = occurredAt;
        Authorized = authorized;
        AffectedParticipantId = affectedParticipantId;
    }

    public static ConcernAuditEvent DeniedSummary(Guid actorAccountId, string operationCategory,
        DateTimeOffset bucketStart, DateTimeOffset firstAttemptAt, DateTimeOffset lastAttemptAt,
        int attemptCount = 1)
    {
        if (actorAccountId == Guid.Empty) throw new ArgumentException("Audit actor is required.", nameof(actorAccountId));
        if (operationCategory is not "ConcernReadDenied" and not "ConcernWriteDenied")
            throw new ArgumentOutOfRangeException(nameof(operationCategory));
        ArgumentOutOfRangeException.ThrowIfLessThan(attemptCount, 1);
        return new ConcernAuditEvent(Guid.NewGuid(), null, actorAccountId, operationCategory,
            firstAttemptAt, false)
        {
            UtcHourBucket = bucketStart,
            FirstAttemptAt = firstAttemptAt,
            LastAttemptAt = lastAttemptAt,
            DeniedAttemptCount = attemptCount,
        };
    }

    public void RecordDeniedAttempt(DateTimeOffset occurredAt)
    {
        if (DeniedAttemptCount < 1 || FirstAttemptAt is null || LastAttemptAt is null)
            throw new InvalidOperationException("Only a denied summary can be incremented.");
        if (occurredAt < FirstAttemptAt || occurredAt < LastAttemptAt)
            throw new ArgumentOutOfRangeException(nameof(occurredAt));
        LastAttemptAt = occurredAt;
        DeniedAttemptCount = checked(DeniedAttemptCount + 1);
    }

    public Guid Id { get; private set; }
    public Guid? ConcernId { get; private set; }
    public Guid? ActorAccountId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; private set; }
    public bool Authorized { get; private set; }
    public Guid? AffectedParticipantId { get; private set; }
    public DateTimeOffset? UtcHourBucket { get; private set; }
    public DateTimeOffset? FirstAttemptAt { get; private set; }
    public DateTimeOffset? LastAttemptAt { get; private set; }
    public int DeniedAttemptCount { get; private set; }
    public DateTimeOffset ExpiresAt => OccurredAt.AddDays(180);
}
