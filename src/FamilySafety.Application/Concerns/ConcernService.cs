using FamilySafety.Application.Profiles;
using FamilySafety.Domain.Concerns;

namespace FamilySafety.Application.Concerns;

public sealed class ConcernService(ICurrentAccountAccessor currentAccountAccessor,
    IConcernRepository repository) : IConcernService
{
    public Task<IReadOnlyList<ConcernView>> GetVisibleAsync(CancellationToken cancellationToken) =>
        repository.GetVisibleAsync(RequireAccountId(), cancellationToken);

    public async Task<ConcernView> GetAsync(Guid concernId, CancellationToken cancellationToken) =>
        concernId == Guid.Empty ? throw new ConcernUnavailableException()
        : await repository.GetAsync(RequireAccountId(), concernId, cancellationToken)
          ?? throw new ConcernUnavailableException();

    public async Task<ConcernView> CreateAsync(Guid familyGroupId, CreateConcernRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (familyGroupId == Guid.Empty) throw new ConcernUnavailableException();
        Required(errors, nameof(request.Reason), request.Reason, 1000);
        ValidateIds(errors, nameof(request.ParticipantProfileIds), request.ParticipantProfileIds);
        ThrowIfInvalid(errors);
        return await repository.CreateAsync(RequireAccountId(), familyGroupId, request, cancellationToken)
            ?? throw new ConcernUnavailableException();
    }

    public async Task<ConcernView> ReplaceParticipantsAsync(Guid concernId,
        ReplaceConcernParticipantsRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (concernId == Guid.Empty) throw new ConcernUnavailableException();
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        ValidateIds(errors, nameof(request.ParticipantProfileIds), request.ParticipantProfileIds);
        ThrowIfInvalid(errors);
        return await repository.ReplaceParticipantsAsync(RequireAccountId(), concernId, request, cancellationToken)
            ?? throw new ConcernUnavailableException();
    }

    public async Task<ConcernView> AddEntryAsync(Guid concernId,
        AddConcernEntryRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (concernId == Guid.Empty) throw new ConcernUnavailableException();
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (!Enum.IsDefined(request.Kind)) errors[nameof(request.Kind)] = ["Choose a valid entry type."];
        if (!Enum.IsDefined(request.Category)) errors[nameof(request.Category)] = ["Choose a valid information category."];
        if (request.Kind == ConcernEntryKind.Unknown && request.Category != ConcernInformationCategory.Unknown)
            errors[nameof(request.Category)] = ["An explicit unknown entry must use the Unknown category."];
        if (request.Kind == ConcernEntryKind.Observation && request.Category == ConcernInformationCategory.Unknown)
            errors[nameof(request.Category)] = ["Record an unknown as an explicit Unknown entry."];
        if (request.Category == ConcernInformationCategory.Outdated
            && (request.OriginalCategory is null || !Enum.IsDefined(request.OriginalCategory.Value)
                || request.OriginalCategory == ConcernInformationCategory.Outdated
                || request.OriginalCategory == ConcernInformationCategory.Unknown))
            errors[nameof(request.OriginalCategory)] = ["Outdated information must preserve its earlier classification."];
        if (request.Category != ConcernInformationCategory.Outdated && request.OriginalCategory is not null)
            errors[nameof(request.OriginalCategory)] = ["An earlier classification is provided only when marking information outdated."];
        if (request.Category == ConcernInformationCategory.Confirmed && request.SourceUnknown)
            errors[nameof(request.SourceDescription)] = ["Confirmed information must include its verification source."];
        Required(errors, nameof(request.Text), request.Text, 2000);
        if (request.SourceUnknown == !string.IsNullOrWhiteSpace(request.SourceDescription))
            errors[nameof(request.SourceDescription)] = ["Provide a source or mark it explicitly unknown."];
        if (request.SourceDescription?.Length > 500)
            errors[nameof(request.SourceDescription)] = ["Source description must be 500 characters or fewer."];
        if (request.Explanation?.Length > 1000)
            errors[nameof(request.Explanation)] = ["Explanation must be 1000 characters or fewer."];
        if (request.Kind is ConcernEntryKind.Correction or ConcernEntryKind.Challenge
            && (request.RelatedEntryId is null || string.IsNullOrWhiteSpace(request.Explanation)))
            errors[nameof(request.RelatedEntryId)] = ["A correction or challenge needs a target and explanation."];
        if (request.Kind is ConcernEntryKind.Observation or ConcernEntryKind.Unknown
            && request.RelatedEntryId is not null)
            errors[nameof(request.RelatedEntryId)] = ["An observation cannot reference another entry."];
        ThrowIfInvalid(errors);
        return await repository.AddEntryAsync(RequireAccountId(), concernId, request, cancellationToken)
            ?? throw new ConcernUnavailableException();
    }

    public async Task<ConcernView> ResolveAsync(Guid concernId, ResolveConcernRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (concernId == Guid.Empty) throw new ConcernUnavailableException();
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        Required(errors, nameof(request.Note), request.Note, 2000);
        ThrowIfInvalid(errors);
        return await repository.ResolveAsync(RequireAccountId(), concernId, request, cancellationToken)
            ?? throw new ConcernUnavailableException();
    }

    private Guid RequireAccountId() => currentAccountAccessor.Current?.AccountId
        ?? throw new CurrentAccountUnavailableException();

    private static void ValidateIds(Dictionary<string, string[]> errors, string key, IReadOnlyList<Guid>? ids)
    {
        if (ids is null) { errors[key] = ["Provide the complete participant list."]; return; }
        if (ids.Any(id => id == Guid.Empty) || ids.Distinct().Count() != ids.Count)
            errors[key] = ["Select each participant once."];
    }

    private static void Required(Dictionary<string, string[]> errors, string key, string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > max)
            errors[key] = [$"Enter between 1 and {max} characters."];
    }

    private static void ThrowIfInvalid(Dictionary<string, string[]> errors)
    {
        if (errors.Count > 0) throw new ConcernValidationException(errors);
    }
}

public sealed class ConcernUnavailableException : Exception { }
