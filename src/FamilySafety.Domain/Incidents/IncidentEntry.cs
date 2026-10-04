namespace FamilySafety.Domain.Incidents;

/// <summary>Record-only contact-attempt details. Never populated from profiles or other domains.</summary>
public sealed record IncidentContactAttemptDetails(string? AttemptedByDescription, string TargetLabel,
    IncidentContactMethod Method, IncidentContactOutcome Outcome);

/// <summary>
/// One attributed Incident history record. Entries are immutable after insert: corrections and
/// challenges are separate linked entries, and no update or delete path exists.
/// </summary>
public sealed class IncidentEntry
{
    private IncidentEntry() { }

    public IncidentEntry(Guid id, Guid incidentId, Guid contributorAccountId, Guid contributorProfileId,
        IncidentEntryKind kind, IncidentInformationCategory category, bool isOutdated, string? text,
        string? sourceDescription, bool sourceUnknown, DateTimeOffset recordedAt, DateTimeOffset? eventAt,
        bool eventTimeUnknown, Guid? relatedEntryId, Guid? relatedTaskId, string? explanation,
        IncidentContactAttemptDetails? contactAttempt = null)
    {
        if (id == Guid.Empty || incidentId == Guid.Empty || contributorAccountId == Guid.Empty
            || contributorProfileId == Guid.Empty || relatedEntryId == Guid.Empty || relatedTaskId == Guid.Empty)
            throw new ArgumentException("Incident entry identifiers are required.");
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        if (!Enum.IsDefined(category)) throw new ArgumentOutOfRangeException(nameof(category));
        if (category == IncidentInformationCategory.Outdated)
            throw new ArgumentException("Persist the prior information category and mark it outdated separately.");
        if (kind == IncidentEntryKind.Unknown && category != IncidentInformationCategory.Unknown)
            throw new ArgumentException("An explicit unknown entry must use the Unknown category.");
        if (kind == IncidentEntryKind.Observation && category == IncidentInformationCategory.Unknown)
            throw new ArgumentException("Record an unknown as an explicit Unknown entry.");
        if (isOutdated && (category == IncidentInformationCategory.Unknown || kind == IncidentEntryKind.TaskResult))
            throw new ArgumentException("Only a known earlier classification can be marked outdated.");
        if (kind != IncidentEntryKind.ContactAttempt) ArgumentException.ThrowIfNullOrWhiteSpace(text);
        IncidentProvenance.EnsureSource(category, sourceDescription, sourceUnknown);
        if (eventTimeUnknown && eventAt is not null)
            throw new ArgumentException("An event time cannot be both known and unknown.");
        if (kind == IncidentEntryKind.ContactAttempt && !eventTimeUnknown && eventAt is null)
            throw new ArgumentException("A contact attempt needs its attempt time or an explicit Unknown.");
        var linksEntry = kind is IncidentEntryKind.Correction or IncidentEntryKind.Challenge;
        if (linksEntry != relatedEntryId.HasValue)
            throw new ArgumentException("Only corrections and challenges reference another entry, and they must.");
        if (linksEntry) ArgumentException.ThrowIfNullOrWhiteSpace(explanation);
        if ((kind == IncidentEntryKind.TaskResult) != relatedTaskId.HasValue)
            throw new ArgumentException("Only task results reference a task, and they must.");
        if ((kind == IncidentEntryKind.ContactAttempt) != (contactAttempt is not null))
            throw new ArgumentException("Contact-attempt details belong only to contact attempts.");
        if (contactAttempt is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(contactAttempt.TargetLabel);
            if (!Enum.IsDefined(contactAttempt.Method) || !Enum.IsDefined(contactAttempt.Outcome))
                throw new ArgumentOutOfRangeException(nameof(contactAttempt));
        }

        Id = id;
        IncidentId = incidentId;
        ContributorAccountId = contributorAccountId;
        ContributorProfileId = contributorProfileId;
        Kind = kind;
        Category = category;
        IsOutdated = isOutdated;
        Text = string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        SourceDescription = sourceUnknown ? null : sourceDescription!.Trim();
        SourceUnknown = sourceUnknown;
        RecordedAt = recordedAt;
        EventAt = eventAt;
        EventTimeUnknown = eventTimeUnknown;
        RelatedEntryId = relatedEntryId;
        RelatedTaskId = relatedTaskId;
        Explanation = string.IsNullOrWhiteSpace(explanation) ? null : explanation.Trim();
        AttemptedByDescription = string.IsNullOrWhiteSpace(contactAttempt?.AttemptedByDescription)
            ? null : contactAttempt.AttemptedByDescription.Trim();
        TargetLabel = contactAttempt?.TargetLabel.Trim();
        ContactMethod = contactAttempt?.Method;
        ContactOutcome = contactAttempt?.Outcome;
    }

    public Guid Id { get; private set; }
    public Guid IncidentId { get; private set; }
    public Guid ContributorAccountId { get; private set; }
    public Guid ContributorProfileId { get; private set; }
    public IncidentEntryKind Kind { get; private set; }

    // The original classification; Outdated is a separate marker so history is preserved.
    public IncidentInformationCategory Category { get; private set; }
    public bool IsOutdated { get; private set; }

    // Required for every kind except a contact attempt, where it holds optional notes.
    public string? Text { get; private set; }
    public string? SourceDescription { get; private set; }
    public bool SourceUnknown { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }
    public DateTimeOffset? EventAt { get; private set; }
    public bool EventTimeUnknown { get; private set; }
    public Guid? RelatedEntryId { get; private set; }
    public Guid? RelatedTaskId { get; private set; }
    public string? Explanation { get; private set; }

    // Null means the contributor made the attempt themself.
    public string? AttemptedByDescription { get; private set; }
    public string? TargetLabel { get; private set; }
    public IncidentContactMethod? ContactMethod { get; private set; }
    public IncidentContactOutcome? ContactOutcome { get; private set; }
}
