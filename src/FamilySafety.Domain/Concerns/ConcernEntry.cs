namespace FamilySafety.Domain.Concerns;

public sealed class ConcernEntry
{
    private ConcernEntry() { }

    public ConcernEntry(Guid id, Guid concernId, Guid contributorAccountId, Guid contributorProfileId,
        ConcernEntryKind kind, ConcernInformationCategory category, string text,
        string? sourceDescription, bool sourceUnknown, DateTimeOffset recordedAt,
        DateTimeOffset? eventAt, Guid? relatedEntryId, string? explanation, bool isOutdated = false)
    {
        if (id == Guid.Empty || concernId == Guid.Empty || contributorAccountId == Guid.Empty
            || contributorProfileId == Guid.Empty)
            throw new ArgumentException("Concern entry identifiers are required.");
        if (!Enum.IsDefined(kind) || !Enum.IsDefined(category))
            throw new ArgumentOutOfRangeException(nameof(category));
        if (category == ConcernInformationCategory.Outdated)
            throw new ArgumentException("Persist the prior information category and mark it outdated separately.");
        if (kind == ConcernEntryKind.Unknown && category != ConcernInformationCategory.Unknown)
            throw new ArgumentException("An explicit unknown entry must use the Unknown category.");
        if (kind == ConcernEntryKind.Observation && category == ConcernInformationCategory.Unknown)
            throw new ArgumentException("Record an unknown as an explicit Unknown entry.");
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        if (sourceUnknown == !string.IsNullOrWhiteSpace(sourceDescription))
            throw new ArgumentException("Provide a source description or explicitly mark the source unknown.");
        if (kind is ConcernEntryKind.Correction or ConcernEntryKind.Challenge && relatedEntryId is null)
            throw new ArgumentException("Corrections and challenges must reference an entry.");
        if (kind is ConcernEntryKind.Correction or ConcernEntryKind.Challenge)
            ArgumentException.ThrowIfNullOrWhiteSpace(explanation);
        if (kind is ConcernEntryKind.Observation or ConcernEntryKind.Unknown && relatedEntryId is not null)
            throw new ArgumentException("A new observation cannot reference another entry.");
        Id = id;
        ConcernId = concernId;
        ContributorAccountId = contributorAccountId;
        ContributorProfileId = contributorProfileId;
        Kind = kind;
        Category = category;
        Text = text.Trim();
        SourceDescription = sourceUnknown ? null : sourceDescription!.Trim();
        SourceUnknown = sourceUnknown;
        RecordedAt = recordedAt;
        EventAt = eventAt;
        RelatedEntryId = relatedEntryId;
        Explanation = explanation?.Trim();
        IsOutdated = isOutdated;
    }

    public Guid Id { get; private set; }
    public Guid ConcernId { get; private set; }
    public Guid ContributorAccountId { get; private set; }
    public Guid ContributorProfileId { get; private set; }
    public ConcernEntryKind Kind { get; private set; }
    public ConcernInformationCategory Category { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public string? SourceDescription { get; private set; }
    public bool SourceUnknown { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }
    public DateTimeOffset? EventAt { get; private set; }
    public Guid? RelatedEntryId { get; private set; }
    public string? Explanation { get; private set; }
    public bool IsOutdated { get; private set; }
}
