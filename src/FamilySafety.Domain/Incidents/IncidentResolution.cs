namespace FamilySafety.Domain.Incidents;

/// <summary>
/// The coordinator's attributed resolution content. There is deliberately no verdict field:
/// resolution means coordination ended, not that anyone is confirmed safe or in danger.
/// </summary>
public sealed record IncidentResolution
{
    public IncidentResolution(string summary, string outcomeText, IncidentInformationCategory outcomeCategory,
        string? outcomeSourceDescription, bool outcomeSourceUnknown, DateTimeOffset? contactRestoredAt,
        bool contactRestoredTimeUnknown, string? assistanceProvided, string? unresolvedFollowUp, string? lessons)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);
        ArgumentException.ThrowIfNullOrWhiteSpace(outcomeText);
        if (!Enum.IsDefined(outcomeCategory) || outcomeCategory == IncidentInformationCategory.Outdated)
            throw new ArgumentOutOfRangeException(nameof(outcomeCategory));
        IncidentProvenance.EnsureSource(outcomeCategory, outcomeSourceDescription, outcomeSourceUnknown);
        if (contactRestoredTimeUnknown == contactRestoredAt.HasValue)
            throw new ArgumentException("Provide the contact-restored time or mark it explicitly unknown.");
        Summary = summary.Trim();
        OutcomeText = outcomeText.Trim();
        OutcomeCategory = outcomeCategory;
        OutcomeSourceDescription = outcomeSourceUnknown ? null : outcomeSourceDescription!.Trim();
        OutcomeSourceUnknown = outcomeSourceUnknown;
        ContactRestoredAt = contactRestoredAt;
        ContactRestoredTimeUnknown = contactRestoredTimeUnknown;
        AssistanceProvided = Optional(assistanceProvided);
        UnresolvedFollowUp = Optional(unresolvedFollowUp);
        Lessons = Optional(lessons);
    }

    public string Summary { get; }
    public string OutcomeText { get; }
    public IncidentInformationCategory OutcomeCategory { get; }
    public string? OutcomeSourceDescription { get; }
    public bool OutcomeSourceUnknown { get; }
    public DateTimeOffset? ContactRestoredAt { get; }
    public bool ContactRestoredTimeUnknown { get; }
    public string? AssistanceProvided { get; }
    public string? UnresolvedFollowUp { get; }
    public string? Lessons { get; }

    private static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
