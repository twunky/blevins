using FamilySafety.Application.Incidents;
using FamilySafety.Application.Profiles;
using FamilySafety.Domain.Incidents;

namespace FamilySafety.Foundation.Tests;

public sealed class IncidentServiceTests
{
    private static readonly Guid Account = Guid.NewGuid();
    private static readonly Guid Id = Guid.NewGuid();
    private readonly RecordingRepository repository = new();

    [Fact]
    public async Task CreationRequiresReasonCategoryAndSourceAndAppliesNoDefaultCategory()
    {
        var service = Service();
        await AssertInvalid(() => service.CreateAsync(Id, Create() with { Reason = " " }, default), "Reason");
        await AssertInvalid(() => service.CreateAsync(Id, Create() with { ReasonCategory = null }, default), "ReasonCategory");
        await AssertInvalid(() => service.CreateAsync(Id, Create() with
        { ReasonCategory = IncidentInformationCategory.Outdated }, default), "ReasonCategory");
        await AssertInvalid(() => service.CreateAsync(Id, Create() with
        { ReasonSourceDescription = null, ReasonSourceUnknown = false }, default), "ReasonSourceDescription");
        await AssertInvalid(() => service.CreateAsync(Id, Create() with
        {
            ReasonCategory = IncidentInformationCategory.Confirmed, ReasonSourceDescription = null,
            ReasonSourceUnknown = true,
        }, default), "ReasonSourceDescription");
        await AssertInvalid(() => service.CreateAsync(Id, Create() with { Reason = new string('x', 1001) }, default), "Reason");
        await AssertInvalid(() => service.CreateAsync(Id, Create() with { SubjectProfileId = Guid.Empty }, default),
            "SubjectProfileId");
        await AssertInvalid(() => service.CreateAsync(Id, Create() with { OriginatingConcernId = Guid.Empty }, default),
            "OriginatingConcernId");
        Assert.Equal(0, repository.Calls);

        var created = await service.CreateAsync(Id, Create() with
        { ReasonCategory = IncidentInformationCategory.Unknown, ReasonSourceDescription = null, ReasonSourceUnknown = true },
            default);
        Assert.NotNull(created);
        Assert.Equal(1, repository.Calls);
    }

    [Fact]
    public async Task ParticipantSetMustBeCompleteDistinctAndAtMost250()
    {
        var service = Service();
        await AssertInvalid(() => service.CreateAsync(Id, Create() with { ParticipantProfileIds = null }, default),
            "ParticipantProfileIds");
        var duplicate = Guid.NewGuid();
        await AssertInvalid(() => service.ReplaceParticipantsAsync(Id,
            new ReplaceIncidentParticipantsRequest([duplicate, duplicate]), default), "ParticipantProfileIds");
        await AssertInvalid(() => service.ReplaceParticipantsAsync(Id,
            new ReplaceIncidentParticipantsRequest([Guid.Empty]), default), "ParticipantProfileIds");
        await AssertInvalid(() => service.ReplaceParticipantsAsync(Id, new ReplaceIncidentParticipantsRequest(
            [.. Enumerable.Range(0, 251).Select(_ => Guid.NewGuid())]), default), "ParticipantProfileIds");
        await service.ReplaceParticipantsAsync(Id, new ReplaceIncidentParticipantsRequest(
            [.. Enumerable.Range(0, 250).Select(_ => Guid.NewGuid())]), default);
        Assert.Equal(1, repository.Calls);
    }

    [Fact]
    public async Task EntryValidationCoversKindsCategoriesOutdatedLinksAndContactAttempts()
    {
        var service = Service();
        var observation = new AddIncidentEntryRequest(IncidentEntryKind.Observation, IncidentInformationCategory.Reported,
            "Seen leaving work", "Coworker", false);
        await AssertInvalid(() => service.AddEntryAsync(Id, observation with { Kind = null }, default), "Kind");
        await AssertInvalid(() => service.AddEntryAsync(Id, observation with { Kind = IncidentEntryKind.TaskResult }, default), "Kind");
        await AssertInvalid(() => service.AddEntryAsync(Id, observation with { Category = null }, default), "Category");
        await AssertInvalid(() => service.AddEntryAsync(Id, observation with { Category = IncidentInformationCategory.Unknown }, default), "Category");
        await AssertInvalid(() => service.AddEntryAsync(Id, observation with { Kind = IncidentEntryKind.Unknown }, default), "Category");
        await AssertInvalid(() => service.AddEntryAsync(Id, observation with { Category = IncidentInformationCategory.Outdated }, default),
            "OriginalCategory");
        await AssertInvalid(() => service.AddEntryAsync(Id, observation with
        { Category = IncidentInformationCategory.Outdated, OriginalCategory = IncidentInformationCategory.Unknown }, default),
            "OriginalCategory");
        await AssertInvalid(() => service.AddEntryAsync(Id, observation with
        { OriginalCategory = IncidentInformationCategory.Reported }, default), "OriginalCategory");
        await AssertInvalid(() => service.AddEntryAsync(Id, observation with
        {
            Category = IncidentInformationCategory.Outdated, OriginalCategory = IncidentInformationCategory.Confirmed,
            SourceDescription = null, SourceUnknown = true,
        }, default), "SourceDescription");
        await AssertInvalid(() => service.AddEntryAsync(Id, observation with { Text = null }, default), "Text");
        await AssertInvalid(() => service.AddEntryAsync(Id, observation with { RelatedEntryId = Guid.NewGuid() }, default),
            "RelatedEntryId");
        await AssertInvalid(() => service.AddEntryAsync(Id, observation with
        { Kind = IncidentEntryKind.Correction, RelatedEntryId = Guid.NewGuid() }, default), "RelatedEntryId");
        await AssertInvalid(() => service.AddEntryAsync(Id, observation with { EventAt = DateTimeOffset.UtcNow, EventTimeUnknown = true }, default),
            "EventAt");
        await AssertInvalid(() => service.AddEntryAsync(Id, observation with { TargetLabel = "Phone" }, default), "TargetLabel");

        var attempt = new AddIncidentEntryRequest(IncidentEntryKind.ContactAttempt, IncidentInformationCategory.Reported,
            null, "My own attempt", false, EventTimeUnknown: true, TargetLabel: "Sam's mobile",
            ContactMethod: IncidentContactMethod.Call, ContactOutcome: IncidentContactOutcome.NoAnswer);
        await AssertInvalid(() => service.AddEntryAsync(Id, attempt with { EventTimeUnknown = false }, default), "EventAt");
        await AssertInvalid(() => service.AddEntryAsync(Id, attempt with { TargetLabel = null }, default), "TargetLabel");
        await AssertInvalid(() => service.AddEntryAsync(Id, attempt with { TargetLabel = new string('x', 201) }, default), "TargetLabel");
        await AssertInvalid(() => service.AddEntryAsync(Id, attempt with { ContactMethod = null }, default), "ContactMethod");
        await AssertInvalid(() => service.AddEntryAsync(Id, attempt with { ContactOutcome = (IncidentContactOutcome)42 }, default),
            "ContactOutcome");
        Assert.Equal(0, repository.Calls);

        await service.AddEntryAsync(Id, attempt, default);
        await service.AddEntryAsync(Id, observation with
        { Category = IncidentInformationCategory.Outdated, OriginalCategory = IncidentInformationCategory.Reported }, default);
        Assert.Equal(2, repository.Calls);
        Assert.Equal(IncidentInformationCategory.Reported, IncidentService.StoredCategory(observation with
        { Category = IncidentInformationCategory.Outdated, OriginalCategory = IncidentInformationCategory.Reported }));
    }

    [Fact]
    public async Task TaskAndResolutionValidationRequireAttributedCategorizedContent()
    {
        var service = Service();
        await AssertInvalid(() => service.CreateTaskAsync(Id, new CreateIncidentTaskRequest(" ", Guid.NewGuid()), default),
            "Description");
        await AssertInvalid(() => service.CreateTaskAsync(Id, new CreateIncidentTaskRequest("Call", Guid.Empty), default),
            "AssigneeProfileId");
        await AssertInvalid(() => service.CompleteTaskAsync(Id, Id, new CompleteIncidentTaskRequest("Done", null, "Call", false), default),
            "Category");
        await AssertInvalid(() => service.CompleteTaskAsync(Id, Id, new CompleteIncidentTaskRequest("Done",
            IncidentInformationCategory.Outdated, "Call", false), default), "Category");
        await AssertInvalid(() => service.CompleteTaskAsync(Id, Id, new CompleteIncidentTaskRequest(null,
            IncidentInformationCategory.Reported, "Call", false), default), "ResultText");
        await AssertInvalid(() => service.CancelTaskAsync(Id, Id, new CancelIncidentTaskRequest(null), default), "Reason");

        var resolve = new ResolveIncidentRequest("Coordination ended", "Reached by phone",
            IncidentInformationCategory.Reported, "Phone call", false, null, true);
        await AssertInvalid(() => service.ResolveAsync(Id, resolve with { ContactRestoredTimeUnknown = false }, default),
            "ContactRestoredAt");
        await AssertInvalid(() => service.ResolveAsync(Id, resolve with { ContactRestoredAt = DateTimeOffset.UtcNow }, default),
            "ContactRestoredAt");
        await AssertInvalid(() => service.ResolveAsync(Id, resolve with { OutcomeCategory = IncidentInformationCategory.Outdated }, default),
            "OutcomeCategory");
        await AssertInvalid(() => service.ResolveAsync(Id, resolve with { Summary = null }, default), "Summary");
        await AssertInvalid(() => service.ResolveAsync(Id, resolve with { OutcomeText = null }, default), "OutcomeText");
        await AssertInvalid(() => service.ResolveAsync(Id, resolve with { Lessons = new string('x', 2001) }, default), "Lessons");
        await AssertInvalid(() => service.TransferCoordinatorAsync(Id, new TransferIncidentCoordinatorRequest(Guid.Empty), default),
            "ParticipantProfileId");
        Assert.Equal(0, repository.Calls);
    }

    [Fact]
    public async Task RepositoryNullBecomesUnavailableAndMissingAccountIsRejectedFirst()
    {
        repository.ReturnNull = true;
        var service = Service();
        await Assert.ThrowsAsync<IncidentUnavailableException>(() => service.GetAsync(Id, default));
        await Assert.ThrowsAsync<IncidentUnavailableException>(() => service.CreateAsync(Id, Create(), default));
        await Assert.ThrowsAsync<IncidentUnavailableException>(() => service.CancelTaskAsync(Id, Id,
            new CancelIncidentTaskRequest("Not needed"), default));

        var anonymous = new IncidentService(new Accessor(null), repository);
        var before = repository.Calls;
        await Assert.ThrowsAsync<CurrentAccountUnavailableException>(() => anonymous.GetAsync(Id, default));
        await Assert.ThrowsAsync<CurrentAccountUnavailableException>(() => anonymous.GetVisibleAsync(default));
        await Assert.ThrowsAsync<CurrentAccountUnavailableException>(() => anonymous.CreateAsync(Id, Create(), default));
        Assert.Equal(before, repository.Calls);
    }

    private IncidentService Service() => new(new Accessor(new CurrentAccount(Account)), repository);

    private static CreateIncidentRequest Create() => new(Guid.NewGuid(), "Contact has not been confirmed",
        IncidentInformationCategory.Reported, "Phone call", false, []);

    private static async Task AssertInvalid(Func<Task> action, string key)
    {
        var exception = await Assert.ThrowsAsync<IncidentValidationException>(action);
        Assert.Contains(key, exception.Errors.Keys);
    }

    private sealed class Accessor(CurrentAccount? current) : ICurrentAccountAccessor
    {
        public CurrentAccount? Current { get; } = current;
    }

    private sealed class RecordingRepository : IIncidentRepository
    {
        public int Calls { get; private set; }
        public bool ReturnNull { get; set; }

        private Task<IncidentView?> Record()
        {
            Calls++;
            return Task.FromResult(ReturnNull ? null : new IncidentView(Id, Guid.NewGuid(), "Family", Guid.NewGuid(),
                "Subject", Guid.NewGuid(), "Creator", Guid.NewGuid(), "Creator", IncidentState.Open,
                DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null, null, null, null, null, null, [], [], [], []));
        }

        public Task<IReadOnlyList<IncidentSummaryView>> GetVisibleAsync(Guid accountId, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult<IReadOnlyList<IncidentSummaryView>>([]);
        }
        public Task<IncidentView?> GetAsync(Guid accountId, Guid incidentId, CancellationToken cancellationToken) => Record();
        public Task<IncidentView?> CreateAsync(Guid accountId, Guid familyGroupId, CreateIncidentRequest request,
            CancellationToken cancellationToken) => Record();
        public Task<IncidentView?> ReplaceParticipantsAsync(Guid accountId, Guid incidentId,
            ReplaceIncidentParticipantsRequest request, CancellationToken cancellationToken) => Record();
        public Task<IncidentView?> TransferCoordinatorAsync(Guid accountId, Guid incidentId,
            TransferIncidentCoordinatorRequest request, CancellationToken cancellationToken) => Record();
        public Task<IncidentView?> AddEntryAsync(Guid accountId, Guid incidentId, AddIncidentEntryRequest request,
            CancellationToken cancellationToken) => Record();
        public Task<IncidentView?> CreateTaskAsync(Guid accountId, Guid incidentId, CreateIncidentTaskRequest request,
            CancellationToken cancellationToken) => Record();
        public Task<IncidentView?> CompleteTaskAsync(Guid accountId, Guid incidentId, Guid taskId,
            CompleteIncidentTaskRequest request, CancellationToken cancellationToken) => Record();
        public Task<IncidentView?> CancelTaskAsync(Guid accountId, Guid incidentId, Guid taskId,
            CancelIncidentTaskRequest request, CancellationToken cancellationToken) => Record();
        public Task<IncidentView?> ResolveAsync(Guid accountId, Guid incidentId, ResolveIncidentRequest request,
            CancellationToken cancellationToken) => Record();
        public Task ReconcileAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
