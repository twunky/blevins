using FamilySafety.Domain.Incidents;

namespace FamilySafety.Application.Incidents;

// Requests. Category and kind are nullable so a missing value is a validation error, never a default.

public sealed record CreateIncidentRequest(Guid SubjectProfileId, string? Reason,
    IncidentInformationCategory? ReasonCategory, string? ReasonSourceDescription, bool ReasonSourceUnknown,
    IReadOnlyList<Guid>? ParticipantProfileIds, Guid? OriginatingConcernId = null);

public sealed record ReplaceIncidentParticipantsRequest(IReadOnlyList<Guid>? ParticipantProfileIds);

public sealed record TransferIncidentCoordinatorRequest(Guid ParticipantProfileId);

public sealed record AddIncidentEntryRequest(IncidentEntryKind? Kind, IncidentInformationCategory? Category,
    string? Text, string? SourceDescription, bool SourceUnknown, DateTimeOffset? EventAt = null,
    bool EventTimeUnknown = false, Guid? RelatedEntryId = null, string? Explanation = null,
    IncidentInformationCategory? OriginalCategory = null, string? AttemptedByDescription = null,
    string? TargetLabel = null, IncidentContactMethod? ContactMethod = null,
    IncidentContactOutcome? ContactOutcome = null);

public sealed record CreateIncidentTaskRequest(string? Description, Guid AssigneeProfileId);

public sealed record CompleteIncidentTaskRequest(string? ResultText, IncidentInformationCategory? Category,
    string? SourceDescription, bool SourceUnknown, DateTimeOffset? EventAt = null);

public sealed record CancelIncidentTaskRequest(string? Reason);

public sealed record ResolveIncidentRequest(string? Summary, string? OutcomeText,
    IncidentInformationCategory? OutcomeCategory, string? OutcomeSourceDescription, bool OutcomeSourceUnknown,
    DateTimeOffset? ContactRestoredAt, bool ContactRestoredTimeUnknown, string? AssistanceProvided = null,
    string? UnresolvedFollowUp = null, string? Lessons = null);

// Responses. Only ordinary display names and Incident fields; no profile contact details.

public sealed record IncidentSummaryView(Guid IncidentId, Guid FamilyGroupId, string FamilyGroupName,
    Guid SubjectProfileId, string SubjectName, IncidentState State, Guid CoordinatorProfileId,
    string CoordinatorName, DateTimeOffset CreatedAt, DateTimeOffset LastAuthorizedMutationAt,
    DateTimeOffset? ExpiresAt);

public sealed record IncidentParticipantView(Guid ProfileId, string DisplayName, bool IsCoordinator,
    DateTimeOffset SelectedAt);

public sealed record IncidentEntryView(Guid EntryId, IncidentEntryKind Kind, IncidentInformationCategory Category,
    bool IsOutdated, string? Text, string? SourceDescription, bool SourceUnknown, Guid ContributorProfileId,
    string ContributorName, DateTimeOffset RecordedAt, DateTimeOffset? EventAt, bool EventTimeUnknown,
    Guid? RelatedEntryId, Guid? RelatedTaskId, string? Explanation, string? AttemptedByDescription,
    string? TargetLabel, IncidentContactMethod? ContactMethod, IncidentContactOutcome? ContactOutcome);

public sealed record IncidentTaskView(Guid TaskId, string Description, Guid AssigneeProfileId,
    string AssigneeName, IncidentTaskStatus Status, bool NotCompleted, Guid CreatedByProfileId,
    string CreatedByName, DateTimeOffset CreatedAt, DateTimeOffset? CompletedAt, Guid? ResultEntryId,
    DateTimeOffset? CancelledAt, Guid? CancelledByProfileId, string? CancelledByName, string? CancelReason);

public sealed record IncidentResolutionView(string Summary, string OutcomeText,
    IncidentInformationCategory OutcomeCategory, string? OutcomeSourceDescription, bool OutcomeSourceUnknown,
    DateTimeOffset? ContactRestoredAt, bool ContactRestoredTimeUnknown, string? AssistanceProvided,
    string? UnresolvedFollowUp, string? Lessons, Guid ResolvedByProfileId, string ResolvedByName,
    DateTimeOffset ResolvedAt);

/// <summary>Timeline item types. Declaration order is the fixed tie-break precedence.</summary>
public enum IncidentTimelineItemType
{
    IncidentCreated,
    ParticipantAdded,
    CoordinatorHandedOff,
    Entry,
    TaskCreated,
    TaskCompleted,
    TaskCancelled,
    ParticipantRemoved,
    ParticipantAccessEnded,
    Frozen,
    Resolved,
}

/// <summary>
/// One read-time timeline item. ActorProfileId is null and ActorName is "System" for system
/// actions. ParticipantAccessEnded never carries a reason.
/// </summary>
public sealed record IncidentTimelineItemView(Guid ItemId, IncidentTimelineItemType Type,
    DateTimeOffset? EventAt, DateTimeOffset RecordedAt, Guid? ActorProfileId, string ActorName,
    IncidentEntryKind? EntryKind, IncidentInformationCategory? Category, bool IsOutdated, Guid? EntryId,
    Guid? RelatedEntryId, Guid? TaskId, Guid? AffectedProfileId, string? AffectedName,
    IncidentFreezeReason? FreezeReason);

public sealed record IncidentView(Guid IncidentId, Guid FamilyGroupId, string FamilyGroupName,
    Guid SubjectProfileId, string SubjectName, Guid CreatedByProfileId, string CreatedByName,
    Guid CoordinatorProfileId, string CoordinatorName, IncidentState State, DateTimeOffset CreatedAt,
    DateTimeOffset LastAuthorizedMutationAt, DateTimeOffset? InactivityFreezeAt, DateTimeOffset? FrozenAt,
    IncidentFreezeReason? FreezeReason, DateTimeOffset? ExpiresAt, IncidentResolutionView? Resolution,
    Guid? OriginatingConcernId, IReadOnlyList<IncidentParticipantView> Participants,
    IReadOnlyList<IncidentEntryView> Entries, IReadOnlyList<IncidentTaskView> Tasks,
    IReadOnlyList<IncidentTimelineItemView> Timeline);

public sealed class IncidentValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("Incident request validation failed.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}

/// <summary>The single nondisclosing result for every unavailable or unauthorized Incident request.</summary>
public sealed class IncidentUnavailableException : Exception { }
