using FamilySafety.Application.Profiles;
using FamilySafety.Domain.Plans;

namespace FamilySafety.Application.Plans;

public sealed class FamilyPlanService(
    ICurrentAccountAccessor currentAccountAccessor,
    IFamilyPlanRepository repository) : IFamilyPlanService
{
    public async Task<IReadOnlyList<FamilyPlanView>> GetVisibleAsync(
        Guid familyGroupId,
        CancellationToken cancellationToken)
    {
        if (familyGroupId == Guid.Empty)
        {
            throw new FamilyPlanUnavailableException();
        }

        return await repository.GetVisibleAsync(
            RequireAccountId(), familyGroupId, cancellationToken)
            ?? throw new FamilyPlanUnavailableException();
    }

    public async Task<FamilyPlanView> GetAsync(
        Guid planId,
        CancellationToken cancellationToken)
    {
        if (planId == Guid.Empty)
        {
            throw new FamilyPlanUnavailableException();
        }

        return await repository.GetAsync(
            RequireAccountId(), planId, cancellationToken)
            ?? throw new FamilyPlanUnavailableException();
    }

    public async Task<FamilyPlanView> CreateAsync(
        Guid familyGroupId,
        CreateFamilyPlanRequest request,
        CancellationToken cancellationToken)
    {
        if (familyGroupId == Guid.Empty)
        {
            throw new FamilyPlanUnavailableException();
        }

        ArgumentNullException.ThrowIfNull(request);
        var errors = new Dictionary<string, string[]>();
        AddActivityTimingErrors(errors, request.ActivityOrDestination, request.Date, request.Time,
            request.ExpectedReturnDate, request.ExpectedReturnTime,
            request.ExpectedCheckInDate, request.ExpectedCheckInTime);

        if (request.Notes?.Length > 1000)
        {
            errors[nameof(request.Notes)] = ["Notes cannot exceed 1,000 characters."];
        }

        if (!Enum.IsDefined(request.AudienceType))
        {
            errors[nameof(request.AudienceType)] = ["Choose who can see this plan."];
        }

        if (request.AudienceProfileIds is null)
        {
            errors[nameof(request.AudienceProfileIds)] = ["Select a plan audience."];
        }
        else if (request.AudienceProfileIds.Any(id => id == Guid.Empty)
            || request.AudienceProfileIds.Distinct().Count() != request.AudienceProfileIds.Count)
        {
            errors[nameof(request.AudienceProfileIds)] = ["Select each family member once."];
        }
        else if (request.AudienceType == PlanAudienceType.FamilyGroup
            && request.AudienceProfileIds.Count != 0)
        {
            errors[nameof(request.AudienceProfileIds)] = ["A whole-group plan does not accept a separate people list."];
        }
        else if (request.AudienceType == PlanAudienceType.SelectedPeople
            && request.AudienceProfileIds.Count == 0)
        {
            errors[nameof(request.AudienceProfileIds)] = ["Select at least one active adult family member."];
        }

        if (errors.Count > 0)
        {
            throw new FamilyPlanValidationException(errors);
        }

        return await repository.CreateAsync(
            RequireAccountId(), familyGroupId, request, cancellationToken)
            ?? throw new FamilyPlanUnavailableException();
    }

    public async Task<FamilyPlanView> UpdateNotesAsync(
        Guid planId,
        UpdateFamilyPlanNotesRequest request,
        CancellationToken cancellationToken)
    {
        if (planId == Guid.Empty)
        {
            throw new FamilyPlanUnavailableException();
        }

        ArgumentNullException.ThrowIfNull(request);
        if (request.Notes?.Length > 1000)
        {
            throw new FamilyPlanValidationException(new Dictionary<string, string[]>
            {
                [nameof(request.Notes)] = ["Notes cannot exceed 1,000 characters."],
            });
        }

        return await repository.UpdateNotesAsync(
            RequireAccountId(), planId, request.Notes, cancellationToken)
            ?? throw new FamilyPlanUnavailableException();
    }

    public async Task<FamilyPlanView> CorrectActivityTimingAsync(
        Guid planId, CorrectFamilyPlanActivityTimingRequest request, CancellationToken cancellationToken)
    {
        if (planId == Guid.Empty) throw new FamilyPlanUnavailableException();
        ArgumentNullException.ThrowIfNull(request);
        var errors = new Dictionary<string, string[]>();
        AddActivityTimingErrors(errors, request.ActivityOrDestination, request.Date, request.Time,
            request.ExpectedReturnDate, request.ExpectedReturnTime,
            request.ExpectedCheckInDate, request.ExpectedCheckInTime);
        if (request.Version == Guid.Empty) errors[nameof(request.Version)] = ["Refresh the Plan before correcting it."];
        if (errors.Count > 0) throw new FamilyPlanValidationException(errors);
        return await repository.CorrectActivityTimingAsync(RequireAccountId(), planId, request, cancellationToken)
            ?? throw new FamilyPlanUnavailableException();
    }

    private static void AddActivityTimingErrors(
        Dictionary<string, string[]> errors, string activityOrDestination, DateOnly? date, TimeOnly? time,
        DateOnly? expectedReturnDate, TimeOnly? expectedReturnTime,
        DateOnly? expectedCheckInDate, TimeOnly? expectedCheckInTime)
    {
        AddRequired(errors, nameof(CreateFamilyPlanRequest.ActivityOrDestination), activityOrDestination);
        if (activityOrDestination?.Length > 200)
            errors[nameof(CreateFamilyPlanRequest.ActivityOrDestination)] = ["Activity or destination cannot exceed 200 characters."];
        if (date is null || date == DateOnly.MinValue)
            errors[nameof(CreateFamilyPlanRequest.Date)] = ["Enter a plan date."];
        if (time is null) errors[nameof(CreateFamilyPlanRequest.Time)] = ["Enter a plan time."];
        if (expectedReturnDate.HasValue != expectedReturnTime.HasValue)
            errors[nameof(CreateFamilyPlanRequest.ExpectedReturnDate)] = ["Enter both expected return date and time, or leave both empty."];
        if (expectedCheckInDate.HasValue != expectedCheckInTime.HasValue)
            errors[nameof(CreateFamilyPlanRequest.ExpectedCheckInDate)] = ["Enter both expected check-in date and time, or leave both empty."];
    }

    private Guid RequireAccountId() => currentAccountAccessor.Current?.AccountId
        ?? throw new CurrentAccountUnavailableException();

    private static void AddRequired(Dictionary<string, string[]> errors, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[key] = ["This field is required."];
        }
    }
}
