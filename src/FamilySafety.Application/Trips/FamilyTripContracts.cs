using FamilySafety.Domain.Trips;

namespace FamilySafety.Application.Trips;

public sealed record TripPersonView(Guid ProfileId, string DisplayName);
public sealed record TripContactView(Guid ContactId, string Name, string? RelationshipRole, string? Phone, string? Email);

public sealed record FamilyTripView(
    Guid TripId, Guid FamilyGroupId, string FamilyGroupName,
    Guid CreatorProfileId, string CreatorDisplayName, bool CanEdit,
    string Title, string PrimaryDestination, DateOnly StartDate, DateOnly EndDate,
    string? Description, string? TransportationNotes, string? AccommodationNotes,
    DateOnly? ExpectedArrivalDate, TimeOnly? ExpectedArrivalTime,
    string? ExpectedArrivalTimeZone, TripAudienceType AudienceType,
    IReadOnlyList<TripPersonView> Travelers,
    IReadOnlyList<TripContactView> Contacts,
    IReadOnlyList<TripPersonView> AudienceRecipients);

public sealed record TripContactInput(string? Name, string? RelationshipRole, string? Phone, string? Email);

public sealed record CreateFamilyTripRequest(
    string? Title, string? PrimaryDestination,
    DateOnly? StartDate, DateOnly? EndDate,
    string? Description, string? TransportationNotes, string? AccommodationNotes,
    DateOnly? ExpectedArrivalDate, TimeOnly? ExpectedArrivalTime,
    string? ExpectedArrivalTimeZone,
    IReadOnlyList<Guid>? TravelerProfileIds,
    TripAudienceType? AudienceType,
    IReadOnlyList<Guid>? AudienceProfileIds,
    IReadOnlyList<TripContactInput>? Contacts);

// Deliberately contains no audience field. Audience changes require a separate work item.
public sealed record UpdateFamilyTripRequest(
    string? Title, string? PrimaryDestination,
    DateOnly? StartDate, DateOnly? EndDate,
    string? Description, string? TransportationNotes, string? AccommodationNotes,
    DateOnly? ExpectedArrivalDate, TimeOnly? ExpectedArrivalTime,
    string? ExpectedArrivalTimeZone,
    IReadOnlyList<Guid>? TravelerProfileIds,
    IReadOnlyList<TripContactInput>? Contacts);

public sealed record ReplaceTripAudienceRequest(
    TripAudienceType? AudienceType,
    IReadOnlyList<Guid>? AudienceProfileIds);

public sealed record TripAudiencePreview(
    string PreviewToken,
    TripAudienceType AudienceType,
    IReadOnlyList<TripPersonView> AudienceRecipients,
    IReadOnlyList<TripPersonView> GainingAccess,
    IReadOnlyList<TripPersonView> LosingAccess);

public sealed record ConfirmTripAudienceRequest(
    TripAudienceType? AudienceType,
    IReadOnlyList<Guid>? AudienceProfileIds,
    string? PreviewToken);

public sealed class FamilyTripValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("The trip request is invalid.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}

public sealed class FamilyTripUnavailableException : Exception;

public sealed class FamilyTripAudienceConflictException : Exception;
