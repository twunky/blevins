using FamilySafety.Domain.Incidents;

namespace FamilySafety.Application.Incidents;

/// <summary>
/// Builds the Incident timeline at read time from persisted records only. There is no timeline
/// table and no event sourcing. Ordering: event time when known, otherwise recorded time; then
/// recorded time; then the fixed <see cref="IncidentTimelineItemType"/> precedence; then the record ID.
/// </summary>
public static class IncidentTimeline
{
    public const string SystemActorName = "System";

    public static IReadOnlyList<IncidentTimelineItemView> Build(SafetyIncident incident,
        IEnumerable<IncidentParticipantGrant> grants, IEnumerable<IncidentCoordinatorChange> coordinatorChanges,
        IEnumerable<IncidentEntry> entries, IEnumerable<IncidentTask> tasks, Func<Guid, string> displayName)
    {
        ArgumentNullException.ThrowIfNull(incident);
        ArgumentNullException.ThrowIfNull(grants);
        ArgumentNullException.ThrowIfNull(coordinatorChanges);
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(tasks);
        ArgumentNullException.ThrowIfNull(displayName);

        var items = new List<IncidentTimelineItemView>
        {
            Item(incident.Id, IncidentTimelineItemType.IncidentCreated, incident.CreatedAt,
                incident.CreatedByProfileId, displayName),
        };

        foreach (var grant in grants)
        {
            // The creator's initial coordinator grant is represented by IncidentCreated.
            if (grant.AddedByProfileId is { } addedBy)
                items.Add(Item(grant.Id, IncidentTimelineItemType.ParticipantAdded, grant.SelectedAt, addedBy,
                    displayName) with { AffectedProfileId = grant.ProfileId, AffectedName = displayName(grant.ProfileId) });
            if (grant.RemovedAt is not { } removedAt) continue;
            items.Add(grant.EligibilityRevoked
                // System access end: deliberately no reason, so membership status is not disclosed.
                ? Item(grant.Id, IncidentTimelineItemType.ParticipantAccessEnded, removedAt, null, displayName)
                    with { AffectedProfileId = grant.ProfileId, AffectedName = displayName(grant.ProfileId) }
                : Item(grant.Id, IncidentTimelineItemType.ParticipantRemoved, removedAt, grant.RemovedByProfileId,
                    displayName) with { AffectedProfileId = grant.ProfileId, AffectedName = displayName(grant.ProfileId) });
        }

        foreach (var change in coordinatorChanges)
            items.Add(Item(change.Id, IncidentTimelineItemType.CoordinatorHandedOff, change.ChangedAt,
                change.FromProfileId, displayName) with
            { AffectedProfileId = change.ToProfileId, AffectedName = displayName(change.ToProfileId) });

        foreach (var entry in entries)
            items.Add(Item(entry.Id, IncidentTimelineItemType.Entry, entry.RecordedAt, entry.ContributorProfileId,
                displayName) with
            {
                EventAt = entry.EventAt,
                EntryKind = entry.Kind,
                Category = entry.Category,
                IsOutdated = entry.IsOutdated,
                EntryId = entry.Id,
                RelatedEntryId = entry.RelatedEntryId,
                TaskId = entry.RelatedTaskId,
            });

        foreach (var task in tasks)
        {
            IncidentTimelineItemView TaskItem(IncidentTimelineItemType type, DateTimeOffset at, Guid? actor) =>
                Item(task.Id, type, at, actor, displayName) with
                {
                    TaskId = task.Id,
                    AffectedProfileId = task.AssigneeProfileId,
                    AffectedName = displayName(task.AssigneeProfileId),
                };
            items.Add(TaskItem(IncidentTimelineItemType.TaskCreated, task.CreatedAt, task.CreatedByProfileId));
            if (task.CompletedAt is { } completedAt)
                items.Add(TaskItem(IncidentTimelineItemType.TaskCompleted, completedAt, task.AssigneeProfileId));
            if (task.CancelledAt is { } cancelledAt)
                items.Add(TaskItem(IncidentTimelineItemType.TaskCancelled, cancelledAt, task.CancelledByProfileId));
        }

        if (incident.FrozenAt is { } frozenAt)
            items.Add(Item(incident.Id, IncidentTimelineItemType.Frozen, frozenAt, null, displayName)
                with { FreezeReason = incident.FreezeReason });
        if (incident.ResolvedAt is { } resolvedAt)
            items.Add(Item(incident.Id, IncidentTimelineItemType.Resolved, resolvedAt, incident.ResolvedByProfileId,
                displayName));

        return Order(items);
    }

    public static IReadOnlyList<IncidentTimelineItemView> Order(IEnumerable<IncidentTimelineItemView> items) =>
        [.. items.OrderBy(item => item.EventAt ?? item.RecordedAt)
            .ThenBy(item => item.RecordedAt)
            .ThenBy(item => item.Type)
            .ThenBy(item => item.ItemId)];

    private static IncidentTimelineItemView Item(Guid itemId, IncidentTimelineItemType type,
        DateTimeOffset recordedAt, Guid? actorProfileId, Func<Guid, string> displayName) =>
        new(itemId, type, null, recordedAt, actorProfileId,
            actorProfileId is { } actor ? displayName(actor) : SystemActorName,
            null, null, false, null, null, null, null, null, null);
}
