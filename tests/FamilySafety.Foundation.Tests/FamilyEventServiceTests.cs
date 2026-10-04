using FamilySafety.Application.Events;
using FamilySafety.Application.Profiles;

namespace FamilySafety.Foundation.Tests;

public sealed class FamilyEventServiceTests
{
    [Fact]
    public async Task CreateRejectsAnEmptyAudienceBeforeCallingRepository()
    {
        var repository = new StubFamilyEventRepository();
        var service = new FamilyEventService(
            new StubCurrentAccountAccessor(new CurrentAccount(Guid.NewGuid())), repository);

        var exception = await Assert.ThrowsAsync<FamilyEventValidationException>(() =>
            service.CreateAsync(Guid.NewGuid(), Request([]), CancellationToken.None));

        Assert.Contains(nameof(CreateFamilyEventRequest.AudienceProfileIds), exception.Errors.Keys);
        Assert.Equal(0, repository.CreateCalls);
    }

    [Fact]
    public async Task CreateRequiresCurrentAccount()
    {
        var repository = new StubFamilyEventRepository();
        var service = new FamilyEventService(new StubCurrentAccountAccessor(null), repository);

        await Assert.ThrowsAsync<CurrentAccountUnavailableException>(() =>
            service.CreateAsync(Guid.NewGuid(), Request([Guid.NewGuid()]), CancellationToken.None));

        Assert.Equal(0, repository.CreateCalls);
    }

    [Fact]
    public async Task CreatePassesTheCurrentAccountAndExplicitAudienceToRepository()
    {
        var accountId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var request = Request([recipientId]);
        var expected = new FamilyEventView(
            Guid.NewGuid(), groupId, request.Name, request.Description, request.Date!.Value,
            request.Time!.Value, request.Location, Guid.NewGuid(), "Organizer", true);
        var repository = new StubFamilyEventRepository { Created = expected };
        var service = new FamilyEventService(
            new StubCurrentAccountAccessor(new CurrentAccount(accountId)), repository);

        var result = await service.CreateAsync(groupId, request, CancellationToken.None);

        Assert.Same(expected, result);
        Assert.Equal(accountId, repository.AccountId);
        Assert.Equal(groupId, repository.GroupId);
        Assert.Equal(request, repository.Request);
    }

    [Fact]
    public async Task CreateRejectsAnEditorWhoDoesNotHaveViewAccess()
    {
        var repository = new StubFamilyEventRepository();
        var service = new FamilyEventService(
            new StubCurrentAccountAccessor(new CurrentAccount(Guid.NewGuid())), repository);
        var request = Request([Guid.NewGuid()]) with { EditorProfileIds = [Guid.NewGuid()] };

        var exception = await Assert.ThrowsAsync<FamilyEventValidationException>(() =>
            service.CreateAsync(Guid.NewGuid(), request, CancellationToken.None));

        Assert.Contains(nameof(CreateFamilyEventRequest.EditorProfileIds), exception.Errors.Keys);
        Assert.Equal(0, repository.CreateCalls);
    }

    [Fact]
    public async Task UpdatePassesTheCurrentAccountAndEventDetailsToRepository()
    {
        var accountId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var request = new UpdateFamilyEventRequest(
            "Updated dinner", "Bring a favorite dish.", new DateOnly(2026, 10, 4),
            new TimeOnly(18, 30), "Community room");
        var expected = new FamilyEventView(
            eventId, groupId, request.Name, request.Description, request.Date!.Value,
            request.Time!.Value, request.Location, Guid.NewGuid(), "Organizer", true);
        var repository = new StubFamilyEventRepository { Updated = expected };
        var service = new FamilyEventService(
            new StubCurrentAccountAccessor(new CurrentAccount(accountId)), repository);

        var result = await service.UpdateAsync(groupId, eventId, request, CancellationToken.None);

        Assert.Same(expected, result);
        Assert.Equal(accountId, repository.AccountId);
        Assert.Equal(groupId, repository.GroupId);
        Assert.Equal(eventId, repository.EventId);
        Assert.Equal(request, repository.UpdateRequest);
    }

    [Fact]
    public async Task GetRejectsAnEmptyEventIdBeforeCallingRepository()
    {
        var repository = new StubFamilyEventRepository();
        var service = new FamilyEventService(
            new StubCurrentAccountAccessor(new CurrentAccount(Guid.NewGuid())), repository);

        await Assert.ThrowsAsync<FamilyEventUnavailableException>(() =>
            service.GetAsync(Guid.Empty, CancellationToken.None));

        Assert.Equal(0, repository.GetCalls);
    }

    [Fact]
    public async Task GetRequiresCurrentAccount()
    {
        var repository = new StubFamilyEventRepository();
        var service = new FamilyEventService(new StubCurrentAccountAccessor(null), repository);

        await Assert.ThrowsAsync<CurrentAccountUnavailableException>(() =>
            service.GetAsync(Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(0, repository.GetCalls);
    }

    [Fact]
    public async Task GetTreatsAMissingOrUnauthorizedEventAsUnavailable()
    {
        var repository = new StubFamilyEventRepository();
        var service = new FamilyEventService(
            new StubCurrentAccountAccessor(new CurrentAccount(Guid.NewGuid())), repository);

        await Assert.ThrowsAsync<FamilyEventUnavailableException>(() =>
            service.GetAsync(Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(1, repository.GetCalls);
    }

    [Fact]
    public async Task GetPassesTheCurrentAccountAndEventIdToRepository()
    {
        var accountId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var expected = new FamilyEventView(
            eventId, Guid.NewGuid(), "Family dinner", "Bring a favorite dish.", new DateOnly(2026, 10, 3),
            new TimeOnly(18, 30), "Community room", Guid.NewGuid(), "Organizer", false);
        var repository = new StubFamilyEventRepository { Item = expected };
        var service = new FamilyEventService(
            new StubCurrentAccountAccessor(new CurrentAccount(accountId)), repository);

        var result = await service.GetAsync(eventId, CancellationToken.None);

        Assert.Same(expected, result);
        Assert.Equal(accountId, repository.AccountId);
        Assert.Equal(eventId, repository.EventId);
    }

    private static CreateFamilyEventRequest Request(IReadOnlyList<Guid> audienceProfileIds) => new(
        "Family dinner",
        "Bring a favorite dish.",
        new DateOnly(2026, 10, 3),
        new TimeOnly(18, 30),
        "Community room",
        audienceProfileIds,
        []);

    private sealed class StubCurrentAccountAccessor(CurrentAccount? current) : ICurrentAccountAccessor
    {
        public CurrentAccount? Current { get; } = current;
    }

    private sealed class StubFamilyEventRepository : IFamilyEventRepository
    {
        public Guid AccountId { get; private set; }

        public Guid GroupId { get; private set; }

        public CreateFamilyEventRequest? Request { get; private set; }

        public Guid EventId { get; private set; }

        public UpdateFamilyEventRequest? UpdateRequest { get; private set; }

        public FamilyEventView? Created { get; init; }

        public FamilyEventView? Updated { get; init; }

        public int CreateCalls { get; private set; }

        public Task<IReadOnlyList<FamilyEventView>?> GetVisibleAsync(
            Guid accountId,
            Guid familyGroupId,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<FamilyEventView>?>(Array.Empty<FamilyEventView>());

        public FamilyEventView? Item { get; init; }

        public int GetCalls { get; private set; }

        public Task<FamilyEventView?> GetAsync(
            Guid accountId,
            Guid eventId,
            CancellationToken cancellationToken)
        {
            AccountId = accountId;
            EventId = eventId;
            GetCalls++;
            return Task.FromResult(Item);
        }

        public Task<FamilyEventView?> CreateAsync(
            Guid accountId,
            Guid familyGroupId,
            CreateFamilyEventRequest request,
            CancellationToken cancellationToken)
        {
            AccountId = accountId;
            GroupId = familyGroupId;
            Request = request;
            CreateCalls++;
            return Task.FromResult(Created);
        }

        public Task<FamilyEventView?> UpdateAsync(
            Guid accountId,
            Guid familyGroupId,
            Guid eventId,
            UpdateFamilyEventRequest request,
            CancellationToken cancellationToken)
        {
            AccountId = accountId;
            GroupId = familyGroupId;
            EventId = eventId;
            UpdateRequest = request;
            return Task.FromResult(Updated);
        }
    }
}
