namespace FamilySafety.Domain.Incidents;

/// <summary>
/// Metadata-only Incident audit evidence, retained 365 days per event and independently of
/// product-history deletion. It never stores Incident text, labels, notes, or resolution content.
/// </summary>
public sealed class IncidentAuditEvent
{
    public const string ReadDenied = "IncidentReadDenied";
    public const string WriteDenied = "IncidentWriteDenied";
    public const int RetentionDays = 365;

    private IncidentAuditEvent() { }

    public IncidentAuditEvent(Guid id, Guid? incidentId, Guid? actorAccountId, string action,
        DateTimeOffset occurredAt, bool authorized, Guid? affectedProfileId = null,
        Guid? fromProfileId = null, Guid? entryId = null, Guid? taskId = null)
    {
        if (id == Guid.Empty) throw new ArgumentException("Audit identifier is required.");
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        Id = id;
        IncidentId = incidentId;
        ActorAccountId = actorAccountId;
        Action = action;
        OccurredAt = occurredAt;
        Authorized = authorized;
        AffectedProfileId = affectedProfileId;
        FromProfileId = fromProfileId;
        EntryId = entryId;
        TaskId = taskId;
    }

    // DEC-069 shape: one row per actor, broad category, and UTC hour, with no Incident ID.
    public static IncidentAuditEvent DeniedSummary(Guid actorAccountId, string operationCategory,
        DateTimeOffset bucketStart, DateTimeOffset firstAttemptAt, DateTimeOffset lastAttemptAt,
        int attemptCount = 1)
    {
        if (actorAccountId == Guid.Empty) throw new ArgumentException("Audit actor is required.", nameof(actorAccountId));
        if (operationCategory is not ReadDenied and not WriteDenied)
            throw new ArgumentOutOfRangeException(nameof(operationCategory));
        ArgumentOutOfRangeException.ThrowIfLessThan(attemptCount, 1);
        return new IncidentAuditEvent(Guid.NewGuid(), null, actorAccountId, operationCategory,
            firstAttemptAt, false)
        {
            UtcHourBucket = bucketStart,
            FirstAttemptAt = firstAttemptAt,
            LastAttemptAt = lastAttemptAt,
            DeniedAttemptCount = attemptCount,
        };
    }

    public static DateTimeOffset HourBucket(DateTimeOffset at)
    {
        var utc = at.UtcDateTime;
        return new DateTimeOffset(utc.Year, utc.Month, utc.Day, utc.Hour, 0, 0, TimeSpan.Zero);
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
    public Guid? IncidentId { get; private set; }
    public Guid? ActorAccountId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; private set; }
    public bool Authorized { get; private set; }
    public Guid? AffectedProfileId { get; private set; }
    public Guid? FromProfileId { get; private set; }
    public Guid? EntryId { get; private set; }
    public Guid? TaskId { get; private set; }
    public DateTimeOffset? UtcHourBucket { get; private set; }
    public DateTimeOffset? FirstAttemptAt { get; private set; }
    public DateTimeOffset? LastAttemptAt { get; private set; }
    public int DeniedAttemptCount { get; private set; }
    public DateTimeOffset ExpiresAt => OccurredAt.AddDays(RetentionDays);
}
