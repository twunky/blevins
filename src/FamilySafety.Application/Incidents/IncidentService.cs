using FamilySafety.Application.Profiles;
using FamilySafety.Domain.Incidents;

namespace FamilySafety.Application.Incidents;

/// <summary>
/// Validates request shape and the current account. Only target-independent checks run here;
/// every target-dependent decision is made by the repository at commit time and surfaces as the
/// uniform <see cref="IncidentUnavailableException"/>.
/// </summary>
public sealed class IncidentService(ICurrentAccountAccessor currentAccountAccessor,
    IIncidentRepository repository) : IIncidentService
{
    public const int MaxParticipants = 250;
    private const int ReasonMax = 1000;
    private const int TextMax = 2000;
    private const int SourceMax = 500;
    private const int ExplanationMax = 1000;
    private const int TaskDescriptionMax = 500;
    private const int LabelMax = 200;
    private const int CancelReasonMax = 1000;

    public Task<IReadOnlyList<IncidentSummaryView>> GetVisibleAsync(CancellationToken cancellationToken) =>
        repository.GetVisibleAsync(RequireAccountId(), cancellationToken);

    public async Task<IncidentView> GetAsync(Guid incidentId, CancellationToken cancellationToken)
    {
        var accountId = RequireAccountId();
        return await repository.GetAsync(accountId, incidentId, cancellationToken)
            ?? throw new IncidentUnavailableException();
    }

    public async Task<IncidentView> CreateAsync(Guid familyGroupId, CreateIncidentRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var accountId = RequireAccountId();
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (request.SubjectProfileId == Guid.Empty)
            errors[nameof(request.SubjectProfileId)] = ["Choose the person this Incident is about."];
        Required(errors, nameof(request.Reason), request.Reason, ReasonMax);
        if (request.ReasonCategory is not { } category || !Enum.IsDefined(category)
            || category == IncidentInformationCategory.Outdated)
            errors[nameof(request.ReasonCategory)] = ["Choose Confirmed, Reported, Assumption, or Unknown."];
        Source(errors, nameof(request.ReasonSourceDescription), request.ReasonCategory,
            request.ReasonSourceDescription, request.ReasonSourceUnknown);
        ValidateParticipantIds(errors, nameof(request.ParticipantProfileIds), request.ParticipantProfileIds);
        if (request.OriginatingConcernId == Guid.Empty)
            errors[nameof(request.OriginatingConcernId)] = ["Leave the originating Concern empty or provide its ID."];
        ThrowIfInvalid(errors);
        return await repository.CreateAsync(accountId, familyGroupId, request, cancellationToken)
            ?? throw new IncidentUnavailableException();
    }

    public async Task<IncidentView> ReplaceParticipantsAsync(Guid incidentId,
        ReplaceIncidentParticipantsRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var accountId = RequireAccountId();
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        ValidateParticipantIds(errors, nameof(request.ParticipantProfileIds), request.ParticipantProfileIds);
        ThrowIfInvalid(errors);
        return await repository.ReplaceParticipantsAsync(accountId, incidentId, request, cancellationToken)
            ?? throw new IncidentUnavailableException();
    }

    public async Task<IncidentView> TransferCoordinatorAsync(Guid incidentId,
        TransferIncidentCoordinatorRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var accountId = RequireAccountId();
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (request.ParticipantProfileId == Guid.Empty)
            errors[nameof(request.ParticipantProfileId)] = ["Choose the participant who will coordinate."];
        ThrowIfInvalid(errors);
        return await repository.TransferCoordinatorAsync(accountId, incidentId, request, cancellationToken)
            ?? throw new IncidentUnavailableException();
    }

    public async Task<IncidentView> AddEntryAsync(Guid incidentId, AddIncidentEntryRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var accountId = RequireAccountId();
        var errors = ValidateEntry(request);
        ThrowIfInvalid(errors);
        return await repository.AddEntryAsync(accountId, incidentId, request, cancellationToken)
            ?? throw new IncidentUnavailableException();
    }

    public async Task<IncidentView> CreateTaskAsync(Guid incidentId, CreateIncidentTaskRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var accountId = RequireAccountId();
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        Required(errors, nameof(request.Description), request.Description, TaskDescriptionMax);
        if (request.AssigneeProfileId == Guid.Empty)
            errors[nameof(request.AssigneeProfileId)] = ["Choose who the task is assigned to."];
        ThrowIfInvalid(errors);
        return await repository.CreateTaskAsync(accountId, incidentId, request, cancellationToken)
            ?? throw new IncidentUnavailableException();
    }

    public async Task<IncidentView> CompleteTaskAsync(Guid incidentId, Guid taskId,
        CompleteIncidentTaskRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var accountId = RequireAccountId();
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        Required(errors, nameof(request.ResultText), request.ResultText, TextMax);
        if (request.Category is not { } category || !Enum.IsDefined(category)
            || category == IncidentInformationCategory.Outdated)
            errors[nameof(request.Category)] = ["Choose Confirmed, Reported, Assumption, or Unknown."];
        Source(errors, nameof(request.SourceDescription), request.Category,
            request.SourceDescription, request.SourceUnknown);
        ThrowIfInvalid(errors);
        return await repository.CompleteTaskAsync(accountId, incidentId, taskId, request, cancellationToken)
            ?? throw new IncidentUnavailableException();
    }

    public async Task<IncidentView> CancelTaskAsync(Guid incidentId, Guid taskId,
        CancelIncidentTaskRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var accountId = RequireAccountId();
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        Required(errors, nameof(request.Reason), request.Reason, CancelReasonMax);
        ThrowIfInvalid(errors);
        return await repository.CancelTaskAsync(accountId, incidentId, taskId, request, cancellationToken)
            ?? throw new IncidentUnavailableException();
    }

    public async Task<IncidentView> ResolveAsync(Guid incidentId, ResolveIncidentRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var accountId = RequireAccountId();
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        Required(errors, nameof(request.Summary), request.Summary, TextMax);
        Required(errors, nameof(request.OutcomeText), request.OutcomeText, TextMax);
        if (request.OutcomeCategory is not { } category || !Enum.IsDefined(category)
            || category == IncidentInformationCategory.Outdated)
            errors[nameof(request.OutcomeCategory)] = ["Choose Confirmed, Reported, Assumption, or Unknown."];
        Source(errors, nameof(request.OutcomeSourceDescription), request.OutcomeCategory,
            request.OutcomeSourceDescription, request.OutcomeSourceUnknown);
        if (request.ContactRestoredTimeUnknown == request.ContactRestoredAt.HasValue)
            errors[nameof(request.ContactRestoredAt)] = ["Provide when contact was restored, or mark it Unknown."];
        Optional(errors, nameof(request.AssistanceProvided), request.AssistanceProvided, TextMax);
        Optional(errors, nameof(request.UnresolvedFollowUp), request.UnresolvedFollowUp, TextMax);
        Optional(errors, nameof(request.Lessons), request.Lessons, TextMax);
        ThrowIfInvalid(errors);
        return await repository.ResolveAsync(accountId, incidentId, request, cancellationToken)
            ?? throw new IncidentUnavailableException();
    }

    /// <summary>The stored classification: the original category when the request marks it Outdated.</summary>
    public static IncidentInformationCategory StoredCategory(AddIncidentEntryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return request.Category == IncidentInformationCategory.Outdated
            ? request.OriginalCategory!.Value : request.Category!.Value;
    }

    private static Dictionary<string, string[]> ValidateEntry(AddIncidentEntryRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (request.Kind is not { } kind || !Enum.IsDefined(kind) || kind == IncidentEntryKind.TaskResult)
        {
            errors[nameof(request.Kind)] = ["Choose a valid entry type. Task results are recorded by completing the task."];
            return errors;
        }
        if (request.Category is not { } category || !Enum.IsDefined(category))
        {
            errors[nameof(request.Category)] = ["Choose a valid information category."];
            return errors;
        }

        if (kind == IncidentEntryKind.Unknown && category != IncidentInformationCategory.Unknown)
            errors[nameof(request.Category)] = ["An explicit unknown entry must use the Unknown category."];
        if (kind == IncidentEntryKind.Observation && category == IncidentInformationCategory.Unknown)
            errors[nameof(request.Category)] = ["Record an unknown as an explicit Unknown entry."];
        if (category == IncidentInformationCategory.Outdated
            && (request.OriginalCategory is not { } original || !Enum.IsDefined(original)
                || original is IncidentInformationCategory.Outdated or IncidentInformationCategory.Unknown))
            errors[nameof(request.OriginalCategory)] = ["Outdated information must preserve its earlier classification."];
        if (category != IncidentInformationCategory.Outdated && request.OriginalCategory is not null)
            errors[nameof(request.OriginalCategory)] = ["An earlier classification is provided only when marking information outdated."];
        var stored = category == IncidentInformationCategory.Outdated ? request.OriginalCategory : category;
        Source(errors, nameof(request.SourceDescription), stored, request.SourceDescription, request.SourceUnknown);

        if (kind == IncidentEntryKind.ContactAttempt) Optional(errors, nameof(request.Text), request.Text, TextMax);
        else Required(errors, nameof(request.Text), request.Text, TextMax);
        if (request.Explanation?.Trim().Length > ExplanationMax)
            errors[nameof(request.Explanation)] = [$"Explanation must be {ExplanationMax} characters or fewer."];

        var linksEntry = kind is IncidentEntryKind.Correction or IncidentEntryKind.Challenge;
        if (linksEntry && (request.RelatedEntryId is null || request.RelatedEntryId == Guid.Empty
            || string.IsNullOrWhiteSpace(request.Explanation)))
            errors[nameof(request.RelatedEntryId)] = ["A correction or challenge needs a target and explanation."];
        if (!linksEntry && request.RelatedEntryId is not null)
            errors[nameof(request.RelatedEntryId)] = ["Only a correction or challenge references another entry."];

        if (request.EventTimeUnknown && request.EventAt is not null)
            errors[nameof(request.EventAt)] = ["Provide an event time or mark it unknown, not both."];

        if (kind == IncidentEntryKind.ContactAttempt)
        {
            if (!request.EventTimeUnknown && request.EventAt is null)
                errors[nameof(request.EventAt)] = ["Provide the attempt time or mark it Unknown."];
            Required(errors, nameof(request.TargetLabel), request.TargetLabel, LabelMax);
            Optional(errors, nameof(request.AttemptedByDescription), request.AttemptedByDescription, LabelMax);
            if (request.ContactMethod is not { } method || !Enum.IsDefined(method))
                errors[nameof(request.ContactMethod)] = ["Choose how contact was attempted."];
            if (request.ContactOutcome is not { } outcome || !Enum.IsDefined(outcome))
                errors[nameof(request.ContactOutcome)] = ["Choose the outcome of the attempt."];
        }
        else if (request.TargetLabel is not null || request.AttemptedByDescription is not null
            || request.ContactMethod is not null || request.ContactOutcome is not null)
            errors[nameof(request.TargetLabel)] = ["Contact-attempt details belong only to contact attempts."];
        return errors;
    }

    private Guid RequireAccountId() => currentAccountAccessor.Current?.AccountId
        ?? throw new CurrentAccountUnavailableException();

    private static void ValidateParticipantIds(Dictionary<string, string[]> errors, string key,
        IReadOnlyList<Guid>? ids)
    {
        if (ids is null) { errors[key] = ["Provide the complete participant list."]; return; }
        if (ids.Any(id => id == Guid.Empty) || ids.Distinct().Count() != ids.Count)
            errors[key] = ["Select each participant once."];
        else if (ids.Count > MaxParticipants)
            errors[key] = [$"Select at most {MaxParticipants} participants."];
    }

    private static void Source(Dictionary<string, string[]> errors, string key,
        IncidentInformationCategory? category, string? description, bool unknown)
    {
        if (unknown == !string.IsNullOrWhiteSpace(description))
            errors[key] = ["Provide a source or mark it explicitly unknown."];
        else if (description?.Trim().Length > SourceMax)
            errors[key] = [$"Source description must be {SourceMax} characters or fewer."];
        else if (category == IncidentInformationCategory.Confirmed && unknown)
            errors[key] = ["Confirmed information must include its verification source."];
    }

    private static void Required(Dictionary<string, string[]> errors, string key, string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > max)
            errors[key] = [$"Enter between 1 and {max} characters."];
    }

    private static void Optional(Dictionary<string, string[]> errors, string key, string? value, int max)
    {
        if (value?.Trim().Length > max) errors[key] = [$"Enter {max} characters or fewer."];
    }

    private static void ThrowIfInvalid(Dictionary<string, string[]> errors)
    {
        if (errors.Count > 0) throw new IncidentValidationException(errors);
    }
}
