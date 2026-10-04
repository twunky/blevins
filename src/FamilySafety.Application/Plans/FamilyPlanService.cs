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
        AddRequired(errors, nameof(request.ActivityOrDestination), request.ActivityOrDestination);
        if (request.Date is null || request.Date == DateOnly.MinValue)
        {
            errors[nameof(request.Date)] = ["Enter a plan date."];
        }

        if (request.Time is null)
        {
            errors[nameof(request.Time)] = ["Enter a plan time."];
        }

        if (request.ExpectedReturnDate.HasValue != request.ExpectedReturnTime.HasValue)
        {
            errors[nameof(request.ExpectedReturnDate)] = ["Enter both expected return date and time, or leave both empty."];
        }

        if (request.ExpectedCheckInDate.HasValue != request.ExpectedCheckInTime.HasValue)
        {
            errors[nameof(request.ExpectedCheckInDate)] = ["Enter both expected check-in date and time, or leave both empty."];
        }

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
