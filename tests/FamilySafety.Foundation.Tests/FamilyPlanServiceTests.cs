using FamilySafety.Application.Plans;
using FamilySafety.Application.Profiles;
using FamilySafety.Domain.Plans;

namespace FamilySafety.Foundation.Tests;

public sealed class FamilyPlanServiceTests
{
    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task CorrectionRejectsEitherHalfOfEachOptionalTiming(bool returnPair, bool dateOnly)
    {
        var service = CreateService(Guid.NewGuid(), new StubRepository());
        var date = new DateOnly(2026, 10, 4);
        var time = new TimeOnly(12, 0);
        var request = new CorrectFamilyPlanActivityTimingRequest("Lake", date, time,
            returnPair && dateOnly ? date : null, returnPair && !dateOnly ? time : null,
            !returnPair && dateOnly ? date : null, !returnPair && !dateOnly ? time : null, Guid.NewGuid());
        var error = await Assert.ThrowsAsync<FamilyPlanValidationException>(() =>
            service.CorrectActivityTimingAsync(Guid.NewGuid(), request, CancellationToken.None));
        Assert.Single(error.Errors);
        Assert.Contains(returnPair ? nameof(request.ExpectedReturnDate) : nameof(request.ExpectedCheckInDate), error.Errors.Keys);
    }

    [Fact]
    public async Task CorrectionReusesCreationValidationAndRequiresVersion()
    {
        var repository = new StubRepository();
        var service = CreateService(Guid.NewGuid(), repository);
        var invalid = new CorrectFamilyPlanActivityTimingRequest(new string('x', 201), null, null,
            new DateOnly(2026, 10, 3), null, null, new TimeOnly(12, 0), Guid.Empty);
        var error = await Assert.ThrowsAsync<FamilyPlanValidationException>(() =>
            service.CorrectActivityTimingAsync(Guid.NewGuid(), invalid, CancellationToken.None));
        Assert.Equal(6, error.Errors.Count);
        Assert.Contains(nameof(invalid.Version), error.Errors.Keys);
        var blank = invalid with { ActivityOrDestination = "   ", Date = DateOnly.MinValue };
        error = await Assert.ThrowsAsync<FamilyPlanValidationException>(() =>
            service.CorrectActivityTimingAsync(Guid.NewGuid(), blank, CancellationToken.None));
        Assert.Contains(nameof(invalid.ActivityOrDestination), error.Errors.Keys);
        Assert.Contains(nameof(invalid.Date), error.Errors.Keys);
    }

    [Fact]
    public async Task CorrectionPassesAccountAndCompleteFactsAndPreservesRepositoryFailures()
    {
        var account = Guid.NewGuid();
        var id = Guid.NewGuid();
        var repository = new StubRepository { Updated = View(Guid.NewGuid()) };
        var service = CreateService(account, repository);
        var request = new CorrectFamilyPlanActivityTimingRequest("Lake", new DateOnly(2026, 10, 4),
            new TimeOnly(12, 0), null, null, null, null, Guid.NewGuid());
        Assert.Same(repository.Updated, await service.CorrectActivityTimingAsync(id, request, CancellationToken.None));
        Assert.Equal(account, repository.AccountId);
        Assert.Equal(id, repository.PlanId);
        Assert.Equal(request, repository.CorrectionRequest);
        await Assert.ThrowsAsync<FamilyPlanUnavailableException>(() =>
            CreateService(account, new StubRepository()).CorrectActivityTimingAsync(id, request, CancellationToken.None));
        await Assert.ThrowsAsync<CurrentAccountUnavailableException>(() =>
            new FamilyPlanService(new StubCurrentAccountAccessor(null), repository)
                .CorrectActivityTimingAsync(id, request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateRejectsSelectedPeopleAudienceWithoutRecipients()
    {
        var repository = new StubRepository();
        var service = CreateService(Guid.NewGuid(), repository);

        var exception = await Assert.ThrowsAsync<FamilyPlanValidationException>(() =>
            service.CreateAsync(Guid.NewGuid(), Request(PlanAudienceType.SelectedPeople, []), CancellationToken.None));

        Assert.Contains(nameof(CreateFamilyPlanRequest.AudienceProfileIds), exception.Errors.Keys);
        Assert.Equal(0, repository.CreateCalls);
    }

    [Fact]
    public async Task CreateRejectsPeopleListForWholeGroupAudience()
    {
        var repository = new StubRepository();
        var service = CreateService(Guid.NewGuid(), repository);

        var exception = await Assert.ThrowsAsync<FamilyPlanValidationException>(() =>
            service.CreateAsync(
                Guid.NewGuid(),
                Request(PlanAudienceType.FamilyGroup, [Guid.NewGuid()]),
                CancellationToken.None));

        Assert.Contains(nameof(CreateFamilyPlanRequest.AudienceProfileIds), exception.Errors.Keys);
        Assert.Equal(0, repository.CreateCalls);
    }

    [Fact]
    public async Task CreateRequiresExpectedReturnAndCheckInDateTimePairs()
    {
        var repository = new StubRepository();
        var service = CreateService(Guid.NewGuid(), repository);
        var request = Request(PlanAudienceType.FamilyGroup, []) with
        {
            ExpectedReturnDate = new DateOnly(2026, 10, 3),
            ExpectedCheckInTime = new TimeOnly(18, 0),
        };

        var exception = await Assert.ThrowsAsync<FamilyPlanValidationException>(() =>
            service.CreateAsync(Guid.NewGuid(), request, CancellationToken.None));

        Assert.Contains(nameof(CreateFamilyPlanRequest.ExpectedReturnDate), exception.Errors.Keys);
        Assert.Contains(nameof(CreateFamilyPlanRequest.ExpectedCheckInDate), exception.Errors.Keys);
        Assert.Equal(0, repository.CreateCalls);
    }

    [Fact]
    public async Task CreateRejectsNotesLongerThanOneThousandCharacters()
    {
        var repository = new StubRepository();
        var service = CreateService(Guid.NewGuid(), repository);
        var request = Request(PlanAudienceType.FamilyGroup, []) with { Notes = new string('n', 1001) };

        var exception = await Assert.ThrowsAsync<FamilyPlanValidationException>(() =>
            service.CreateAsync(Guid.NewGuid(), request, CancellationToken.None));

        Assert.Contains(nameof(CreateFamilyPlanRequest.Notes), exception.Errors.Keys);
        Assert.Equal(0, repository.CreateCalls);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAllowsEmptyOrWhitespaceNotes(string notes)
    {
        var repository = new StubRepository { Created = View(Guid.NewGuid()) };
        var service = CreateService(Guid.NewGuid(), repository);

        await service.CreateAsync(Guid.NewGuid(), Request(PlanAudienceType.FamilyGroup, []) with { Notes = notes }, CancellationToken.None);

        Assert.NotNull(repository.Request);
    }

    [Fact]
    public void PlanStoresNonblankNotesUnchangedAndTreatsWhitespaceAsAbsent()
    {
        var lineBreaks = "Meet outside.\nBring water.";
        var withNote = new FamilyPlan(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Visit the park",
            new DateOnly(2026, 10, 3), new TimeOnly(10, 0), null, null, null, null,
            PlanAudienceType.FamilyGroup, lineBreaks);
        var withoutNote = new FamilyPlan(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Visit the park",
            new DateOnly(2026, 10, 3), new TimeOnly(10, 0), null, null, null, null,
            PlanAudienceType.FamilyGroup, "  \r\n ");

        Assert.Equal(lineBreaks, withNote.Notes);
        Assert.Null(withoutNote.Notes);
    }

    [Fact]
    public void PlanUpdatesAndClearsOnlyItsOptionalNote()
    {
        var plan = new FamilyPlan(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Visit the park",
            new DateOnly(2026, 10, 3), new TimeOnly(10, 0), null, null, null, null,
            PlanAudienceType.FamilyGroup, null);

        plan.UpdateNotes("Meet outside.\nBring water.");
        Assert.Equal("Meet outside.\nBring water.", plan.Notes);
        plan.UpdateNotes("   ");
        Assert.Null(plan.Notes);
        plan.UpdateNotes("New note");
        plan.UpdateNotes(null);
        Assert.Null(plan.Notes);
        Assert.Throws<ArgumentOutOfRangeException>(() => plan.UpdateNotes(new string('n', 1001)));
        Assert.Null(plan.Notes);
        Assert.Equal("Visit the park", plan.ActivityOrDestination);
    }

    [Fact]
    public async Task UpdateNotesPassesCurrentAccountPlanAndNoteToRepository()
    {
        var accountId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var repository = new StubRepository { Updated = View(Guid.NewGuid()) with { Notes = "Updated" } };
        var service = CreateService(accountId, repository);

        var result = await service.UpdateNotesAsync(planId, new("Updated"), CancellationToken.None);

        Assert.Same(repository.Updated, result);
        Assert.Equal(accountId, repository.AccountId);
        Assert.Equal(planId, repository.PlanId);
        Assert.Equal("Updated", repository.UpdatedNotes);
    }

    [Fact]
    public async Task UpdateNotesRejectsTooLongAndTreatsMissingOrUnauthorizedAsUnavailable()
    {
        var repository = new StubRepository();
        var service = CreateService(Guid.NewGuid(), repository);
        var planId = Guid.NewGuid();

        var invalid = await Assert.ThrowsAsync<FamilyPlanValidationException>(() =>
            service.UpdateNotesAsync(planId, new(new string('n', 1001)), CancellationToken.None));
        Assert.Contains(nameof(UpdateFamilyPlanNotesRequest.Notes), invalid.Errors.Keys);
        Assert.Equal(0, repository.UpdateCalls);
        await Assert.ThrowsAsync<FamilyPlanUnavailableException>(() =>
            service.UpdateNotesAsync(planId, new("Changed"), CancellationToken.None));
        Assert.Equal(1, repository.UpdateCalls);
        await Assert.ThrowsAsync<FamilyPlanUnavailableException>(() =>
            service.UpdateNotesAsync(Guid.Empty, new(null), CancellationToken.None));
        Assert.Equal(1, repository.UpdateCalls);
    }

    [Fact]
    public async Task UpdateNotesRequiresCurrentAccount()
    {
        var repository = new StubRepository();
        var service = new FamilyPlanService(new StubCurrentAccountAccessor(null), repository);

        await Assert.ThrowsAsync<CurrentAccountUnavailableException>(() =>
            service.UpdateNotesAsync(Guid.NewGuid(), new(null), CancellationToken.None));
        Assert.Equal(0, repository.UpdateCalls);
    }

    [Fact]
    public async Task CreatePassesTheCurrentAccountAndAudienceToRepository()
    {
        var accountId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var request = Request(PlanAudienceType.FamilyGroup, []);
        var repository = new StubRepository { Created = View(groupId) };
        var service = CreateService(accountId, repository);

        var result = await service.CreateAsync(groupId, request, CancellationToken.None);

        Assert.Same(repository.Created, result);
        Assert.Equal(accountId, repository.AccountId);
        Assert.Equal(groupId, repository.GroupId);
        Assert.Equal(request, repository.Request);
    }

    [Fact]
    public async Task GetRejectsAnEmptyPlanIdBeforeCallingRepository()
    {
        var repository = new StubRepository();
        var service = CreateService(Guid.NewGuid(), repository);

        await Assert.ThrowsAsync<FamilyPlanUnavailableException>(() =>
            service.GetAsync(Guid.Empty, CancellationToken.None));

        Assert.Equal(0, repository.GetCalls);
    }

    [Fact]
    public async Task GetRequiresCurrentAccount()
    {
        var repository = new StubRepository();
        var service = new FamilyPlanService(new StubCurrentAccountAccessor(null), repository);

        await Assert.ThrowsAsync<CurrentAccountUnavailableException>(() =>
            service.GetAsync(Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(0, repository.GetCalls);
    }

    [Fact]
    public async Task GetTreatsAMissingOrUnauthorizedPlanAsUnavailable()
    {
        var repository = new StubRepository();
        var service = CreateService(Guid.NewGuid(), repository);

        await Assert.ThrowsAsync<FamilyPlanUnavailableException>(() =>
            service.GetAsync(Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(1, repository.GetCalls);
    }

    [Fact]
    public async Task GetPassesTheCurrentAccountAndPlanIdToRepository()
    {
        var accountId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var repository = new StubRepository { Item = View(Guid.NewGuid()) };
        var service = CreateService(accountId, repository);

        var result = await service.GetAsync(planId, CancellationToken.None);

        Assert.Same(repository.Item, result);
        Assert.Equal(accountId, repository.AccountId);
        Assert.Equal(planId, repository.PlanId);
    }

    private static FamilyPlanService CreateService(Guid accountId, StubRepository repository) => new(
        new StubCurrentAccountAccessor(new CurrentAccount(accountId)), repository);

    private static CreateFamilyPlanRequest Request(
        PlanAudienceType audienceType,
        IReadOnlyList<Guid> audienceProfileIds) => new(
        "Visit the lake",
        new DateOnly(2026, 10, 3),
        new TimeOnly(10, 0),
        null,
        null,
        null,
        null,
        audienceType,
        audienceProfileIds);

    private static FamilyPlanView View(Guid groupId) => new(
        Guid.NewGuid(), groupId, "Visit the lake", new DateOnly(2026, 10, 3), new TimeOnly(10, 0),
        null, null, null, null, PlanAudienceType.FamilyGroup, Guid.NewGuid(), "Planner");

    private sealed class StubCurrentAccountAccessor(CurrentAccount? current) : ICurrentAccountAccessor
    {
        public CurrentAccount? Current { get; } = current;
    }

    private sealed class StubRepository : IFamilyPlanRepository
    {
        public CorrectFamilyPlanActivityTimingRequest? CorrectionRequest { get; private set; }

        public Task<FamilyPlanView?> CorrectActivityTimingAsync(Guid accountId, Guid planId,
            CorrectFamilyPlanActivityTimingRequest request, CancellationToken cancellationToken)
        {
            AccountId = accountId;
            PlanId = planId;
            CorrectionRequest = request;
            return Task.FromResult(Updated);
        }

        public Guid AccountId { get; private set; }
        public Guid GroupId { get; private set; }
        public CreateFamilyPlanRequest? Request { get; private set; }
        public FamilyPlanView? Created { get; init; }
        public int CreateCalls { get; private set; }

        public Task<IReadOnlyList<FamilyPlanView>?> GetVisibleAsync(
            Guid accountId,
            Guid familyGroupId,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<FamilyPlanView>?>(Array.Empty<FamilyPlanView>());

        public Guid PlanId { get; private set; }
        public FamilyPlanView? Item { get; init; }
        public int GetCalls { get; private set; }
        public FamilyPlanView? Updated { get; init; }
        public string? UpdatedNotes { get; private set; }
        public int UpdateCalls { get; private set; }

        public Task<FamilyPlanView?> GetAsync(
            Guid accountId,
            Guid planId,
            CancellationToken cancellationToken)
        {
            AccountId = accountId;
            PlanId = planId;
            GetCalls++;
            return Task.FromResult(Item);
        }

        public Task<FamilyPlanView?> CreateAsync(
            Guid accountId,
            Guid familyGroupId,
            CreateFamilyPlanRequest request,
            CancellationToken cancellationToken)
        {
            AccountId = accountId;
            GroupId = familyGroupId;
            Request = request;
            CreateCalls++;
            return Task.FromResult(Created);
        }

        public Task<FamilyPlanView?> UpdateNotesAsync(
            Guid accountId, Guid planId, string? notes, CancellationToken cancellationToken)
        {
            AccountId = accountId;
            PlanId = planId;
            UpdatedNotes = notes;
            UpdateCalls++;
            return Task.FromResult(Updated);
        }
    }
}
