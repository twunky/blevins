using FamilySafety.Application.Profiles;

namespace FamilySafety.Application.Events;

public sealed class FamilyEventService(
    ICurrentAccountAccessor currentAccountAccessor,
    IFamilyEventRepository repository) : IFamilyEventService
{
    public async Task<IReadOnlyList<FamilyEventView>> GetVisibleAsync(
        Guid familyGroupId,
        CancellationToken cancellationToken)
    {
        if (familyGroupId == Guid.Empty)
        {
            throw new FamilyEventUnavailableException();
        }

        return await repository.GetVisibleAsync(
            RequireAccountId(), familyGroupId, cancellationToken)
            ?? throw new FamilyEventUnavailableException();
    }

    public async Task<FamilyEventView> GetAsync(
        Guid eventId,
        CancellationToken cancellationToken)
    {
        if (eventId == Guid.Empty)
        {
            throw new FamilyEventUnavailableException();
        }

        return await repository.GetAsync(
            RequireAccountId(), eventId, cancellationToken)
            ?? throw new FamilyEventUnavailableException();
    }

    public async Task<FamilyEventView> CreateAsync(
        Guid familyGroupId,
        CreateFamilyEventRequest request,
        CancellationToken cancellationToken)
    {
        if (familyGroupId == Guid.Empty)
        {
            throw new FamilyEventUnavailableException();
        }

        ArgumentNullException.ThrowIfNull(request);
        var accountId = RequireAccountId();
        var errors = new Dictionary<string, string[]>();
        AddRequired(errors, nameof(request.Name), request.Name);
        AddRequired(errors, nameof(request.Description), request.Description);
        AddRequired(errors, nameof(request.Location), request.Location);
        if (request.Date is null || request.Date == DateOnly.MinValue)
        {
            errors[nameof(request.Date)] = ["Enter an event date."];
        }

        if (request.Time is null)
        {
            errors[nameof(request.Time)] = ["Enter an event time."];
        }

        if (request.AudienceProfileIds is null || request.AudienceProfileIds.Count == 0)
        {
            errors[nameof(request.AudienceProfileIds)] = ["Select at least one active adult family member."];
        }
        else if (request.AudienceProfileIds.Any(id => id == Guid.Empty)
            || request.AudienceProfileIds.Distinct().Count() != request.AudienceProfileIds.Count)
        {
            errors[nameof(request.AudienceProfileIds)] = ["Select each active adult family member once."];
        }

        if (request.EditorProfileIds is null)
        {
            errors[nameof(request.EditorProfileIds)] = ["Choose edit access for the selected family members."];
        }
        else if (request.EditorProfileIds.Any(id => id == Guid.Empty)
            || request.EditorProfileIds.Distinct().Count() != request.EditorProfileIds.Count)
        {
            errors[nameof(request.EditorProfileIds)] = ["Select each event editor once."];
        }
        else if (request.AudienceProfileIds is not null
            && request.EditorProfileIds.Except(request.AudienceProfileIds).Any())
        {
            errors[nameof(request.EditorProfileIds)] = ["Event editors must also have view access."];
        }

        if (errors.Count > 0)
        {
            throw new FamilyEventValidationException(errors);
        }

        return await repository.CreateAsync(
            accountId, familyGroupId, request, cancellationToken)
            ?? throw new FamilyEventUnavailableException();
    }

    public async Task<FamilyEventView> UpdateAsync(
        Guid familyGroupId,
        Guid eventId,
        UpdateFamilyEventRequest request,
        CancellationToken cancellationToken)
    {
        if (familyGroupId == Guid.Empty || eventId == Guid.Empty)
        {
            throw new FamilyEventUnavailableException();
        }

        ArgumentNullException.ThrowIfNull(request);
        var accountId = RequireAccountId();
        var errors = new Dictionary<string, string[]>();
        AddRequired(errors, nameof(request.Name), request.Name);
        AddRequired(errors, nameof(request.Description), request.Description);
        AddRequired(errors, nameof(request.Location), request.Location);
        if (request.Date is null || request.Date == DateOnly.MinValue)
        {
            errors[nameof(request.Date)] = ["Enter an event date."];
        }

        if (request.Time is null)
        {
            errors[nameof(request.Time)] = ["Enter an event time."];
        }

        if (errors.Count > 0)
        {
            throw new FamilyEventValidationException(errors);
        }

        return await repository.UpdateAsync(
            accountId, familyGroupId, eventId, request, cancellationToken)
            ?? throw new FamilyEventUnavailableException();
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
