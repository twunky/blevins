using FamilySafety.Domain.Plans;
using System.Text.Json.Serialization;

namespace FamilySafety.Application.Plans;

public sealed record FamilyPlanView(
    Guid PlanId,
    Guid FamilyGroupId,
    string ActivityOrDestination,
    DateOnly Date,
    TimeOnly Time,
    DateOnly? ExpectedReturnDate,
    TimeOnly? ExpectedReturnTime,
    DateOnly? ExpectedCheckInDate,
    TimeOnly? ExpectedCheckInTime,
    PlanAudienceType AudienceType,
    Guid CreatorProfileId,
    string CreatorDisplayName,
    string? Notes = null,
    Guid Version = default,
    bool CanCorrectActivityTiming = false);

public sealed record CreateFamilyPlanRequest(
    string ActivityOrDestination,
    DateOnly? Date,
    TimeOnly? Time,
    DateOnly? ExpectedReturnDate,
    TimeOnly? ExpectedReturnTime,
    DateOnly? ExpectedCheckInDate,
    TimeOnly? ExpectedCheckInTime,
    PlanAudienceType AudienceType,
    IReadOnlyList<Guid> AudienceProfileIds,
    string? Notes = null);

public sealed record UpdateFamilyPlanNotesRequest(string? Notes);

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CorrectFamilyPlanActivityTimingRequest(
    [property: JsonRequired] string ActivityOrDestination,
    [property: JsonRequired] DateOnly? Date,
    [property: JsonRequired] TimeOnly? Time,
    [property: JsonRequired] DateOnly? ExpectedReturnDate,
    [property: JsonRequired] TimeOnly? ExpectedReturnTime,
    [property: JsonRequired] DateOnly? ExpectedCheckInDate,
    [property: JsonRequired] TimeOnly? ExpectedCheckInTime,
    [property: JsonRequired] Guid Version);

public sealed class FamilyPlanConflictException : Exception
{
}

public sealed class FamilyPlanValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("The plan request is invalid.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}

public sealed class FamilyPlanUnavailableException : Exception
{
}
