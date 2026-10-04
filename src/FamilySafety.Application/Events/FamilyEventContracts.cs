namespace FamilySafety.Application.Events;

public sealed record FamilyEventView(
    Guid EventId,
    Guid FamilyGroupId,
    string Name,
    string Description,
    DateOnly Date,
    TimeOnly Time,
    string Location,
    Guid OrganizerProfileId,
    string OrganizerDisplayName,
    bool CanEdit);

public sealed record CreateFamilyEventRequest(
    string Name,
    string Description,
    DateOnly? Date,
    TimeOnly? Time,
    string Location,
    IReadOnlyList<Guid> AudienceProfileIds,
    IReadOnlyList<Guid> EditorProfileIds);

public sealed record UpdateFamilyEventRequest(
    string Name,
    string Description,
    DateOnly? Date,
    TimeOnly? Time,
    string Location);

public sealed class FamilyEventValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("The event request is invalid.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}

public sealed class FamilyEventUnavailableException : Exception
{
}
