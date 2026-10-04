using FamilySafety.Domain.Concerns;

namespace FamilySafety.Application.Concerns;

public sealed record CreateConcernRequest(Guid SubjectProfileId, string? Reason,
    IReadOnlyList<Guid>? ParticipantProfileIds);

public sealed record AddConcernEntryRequest(ConcernEntryKind Kind,
    ConcernInformationCategory Category, string? Text, string? SourceDescription,
    bool SourceUnknown, DateTimeOffset? EventAt, Guid? RelatedEntryId,
    string? Explanation, bool IsOutdated = false,
    ConcernInformationCategory? OriginalCategory = null);

public sealed record ResolveConcernRequest(string? Note);

public sealed record ReplaceConcernParticipantsRequest(IReadOnlyList<Guid>? ParticipantProfileIds);

public sealed record ConcernPersonView(Guid ProfileId, string DisplayName);

public sealed record ConcernEntryView(Guid EntryId, ConcernEntryKind Kind,
    ConcernInformationCategory Category, string Text, string? SourceDescription,
    bool SourceUnknown, Guid ContributorProfileId, string ContributorName,
    DateTimeOffset RecordedAt, DateTimeOffset? EventAt, Guid? RelatedEntryId,
    string? Explanation, bool IsOutdated);

public sealed record ConcernView(Guid ConcernId, Guid FamilyGroupId, string FamilyGroupName,
    Guid CreatorProfileId, string CreatorName, Guid SubjectProfileId, string SubjectName,
    string CreationReason, ConcernState State, DateTimeOffset CreatedAt,
    DateTimeOffset LastAuthorizedMutationAt, DateTimeOffset? FrozenAt,
    ConcernFreezeReason? FreezeReason, DateTimeOffset? ResolvedAt,
    string? ResolutionNote, DateTimeOffset? ExpiresAt,
    IReadOnlyList<ConcernPersonView> Participants, IReadOnlyList<ConcernEntryView> Entries);

public sealed class ConcernValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("Concern request validation failed.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
