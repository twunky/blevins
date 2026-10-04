using FamilySafety.Domain.Plans;

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
    string? Notes = null);

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

public sealed class FamilyPlanValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("The plan request is invalid.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}

public sealed class FamilyPlanUnavailableException : Exception
{
}
