namespace FamilySafety.Domain.Incidents;

/// <summary>
/// A simple coordination task. The description and assignee never change; status only moves
/// forward. The completion result is the TaskResult entry that references this task.
/// </summary>
public sealed class IncidentTask
{
    private IncidentTask() { }

    public IncidentTask(Guid id, Guid incidentId, string description, Guid assigneeAccountId,
        Guid assigneeProfileId, Guid assigneeGrantId, Guid createdByAccountId, Guid createdByProfileId,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty || incidentId == Guid.Empty || assigneeAccountId == Guid.Empty
            || assigneeProfileId == Guid.Empty || assigneeGrantId == Guid.Empty
            || createdByAccountId == Guid.Empty || createdByProfileId == Guid.Empty)
            throw new ArgumentException("Incident task identifiers are required.");
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        Id = id;
        IncidentId = incidentId;
        Description = description.Trim();
        AssigneeAccountId = assigneeAccountId;
        AssigneeProfileId = assigneeProfileId;
        AssigneeGrantId = assigneeGrantId;
        CreatedByAccountId = createdByAccountId;
        CreatedByProfileId = createdByProfileId;
        CreatedAt = createdAt;
        Status = IncidentTaskStatus.Open;
    }

    public Guid Id { get; private set; }
    public Guid IncidentId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Guid AssigneeAccountId { get; private set; }
    public Guid AssigneeProfileId { get; private set; }

    // The grant valid at assignment. Only the same grant may complete the task.
    public Guid AssigneeGrantId { get; private set; }
    public Guid CreatedByAccountId { get; private set; }
    public Guid CreatedByProfileId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public IncidentTaskStatus Status { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public Guid? CancelledByAccountId { get; private set; }
    public Guid? CancelledByProfileId { get; private set; }
    public string? CancelReason { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public bool IsSelfAssignedBy(Guid accountId) =>
        CreatedByAccountId == accountId && AssigneeAccountId == accountId;

    public void Complete(Guid actorGrantId, DateTimeOffset at)
    {
        EnsureOpen();
        if (actorGrantId != AssigneeGrantId)
            throw new InvalidOperationException("Only the assignee's original grant may complete a task.");
        Status = IncidentTaskStatus.Done;
        CompletedAt = at;
    }

    public void Cancel(Guid actorAccountId, Guid actorProfileId, string reason, DateTimeOffset at)
    {
        EnsureOpen();
        if (actorAccountId == Guid.Empty || actorProfileId == Guid.Empty)
            throw new ArgumentException("The canceller is required.");
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        Status = IncidentTaskStatus.Cancelled;
        CancelledAt = at;
        CancelledByAccountId = actorAccountId;
        CancelledByProfileId = actorProfileId;
        CancelReason = reason.Trim();
    }

    private void EnsureOpen()
    {
        if (Status != IncidentTaskStatus.Open)
            throw new InvalidOperationException("Done and Cancelled tasks are terminal.");
    }
}
