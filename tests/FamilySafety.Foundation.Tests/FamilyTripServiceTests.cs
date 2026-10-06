using FamilySafety.Application.Profiles;
using FamilySafety.Application.Trips;
using FamilySafety.Domain.Trips;

namespace FamilySafety.Foundation.Tests;

public sealed class FamilyTripServiceTests
{
    [Fact]
    public async Task CreateRejectsMissingFieldsAndReversedDatesBeforeRepository()
    {
        var repository = new StubRepository();
        var service = Service(repository);
        var request = CreateRequest() with
        {
            Title = "   ", PrimaryDestination = "", StartDate = null,
            EndDate = null, TravelerProfileIds = [], AudienceType = null,
        };
        var error = await Assert.ThrowsAsync<FamilyTripValidationException>(() =>
            service.CreateAsync(Guid.NewGuid(), request, CancellationToken.None));
        Assert.Contains("Title", error.Errors.Keys);
        Assert.Contains("PrimaryDestination", error.Errors.Keys);
        Assert.Contains("StartDate", error.Errors.Keys);
        Assert.Contains("EndDate", error.Errors.Keys);
        Assert.Contains("TravelerProfileIds", error.Errors.Keys);
        Assert.Contains("AudienceType", error.Errors.Keys);
        Assert.Equal(0, repository.CreateCalls);

        error = await Assert.ThrowsAsync<FamilyTripValidationException>(() =>
            service.CreateAsync(Guid.NewGuid(), CreateRequest() with
            {
                EndDate = new DateOnly(2026, 9, 30),
            }, CancellationToken.None));
        Assert.Contains("EndDate", error.Errors.Keys);
    }

    [Fact]
    public async Task SelectedPeopleRequiresAnotherRecipientAndGroupModeRejectsPeopleList()
    {
        var service = Service(new StubRepository());
        var selectedError = await Assert.ThrowsAsync<FamilyTripValidationException>(() =>
            service.CreateAsync(Guid.NewGuid(), CreateRequest() with
            {
                AudienceType = TripAudienceType.SelectedPeople,
            }, CancellationToken.None));
        Assert.Contains("AudienceProfileIds", selectedError.Errors.Keys);

        var groupError = await Assert.ThrowsAsync<FamilyTripValidationException>(() =>
            service.CreateAsync(Guid.NewGuid(), CreateRequest() with
            {
                AudienceProfileIds = [Guid.NewGuid()],
            }, CancellationToken.None));
        Assert.Contains("AudienceProfileIds", groupError.Errors.Keys);
    }

    [Fact]
    public async Task ExpectedArrivalRequiresDateTimeAndRecognizedZone()
    {
        var service = Service(new StubRepository());
        var missingZone = await Assert.ThrowsAsync<FamilyTripValidationException>(() =>
            service.CreateAsync(Guid.NewGuid(), CreateRequest() with
            {
                ExpectedArrivalDate = new DateOnly(2026, 10, 2),
                ExpectedArrivalTime = new TimeOnly(17, 0),
            }, CancellationToken.None));
        Assert.Contains("ExpectedArrivalDate", missingZone.Errors.Keys);

        var unknownZone = await Assert.ThrowsAsync<FamilyTripValidationException>(() =>
            service.CreateAsync(Guid.NewGuid(), CreateRequest() with
            {
                ExpectedArrivalDate = new DateOnly(2026, 10, 2),
                ExpectedArrivalTime = new TimeOnly(17, 0),
                ExpectedArrivalTimeZone = "No/Such_Zone",
            }, CancellationToken.None));
        Assert.Contains("ExpectedArrivalTimeZone", unknownZone.Errors.Keys);
    }

    [Fact]
    public async Task ContactAndTextLimitsReuseExistingValidationConventions()
    {
        var service = Service(new StubRepository());
        var error = await Assert.ThrowsAsync<FamilyTripValidationException>(() =>
            service.CreateAsync(Guid.NewGuid(), CreateRequest() with
            {
                Description = new string('x', 1001),
                Contacts = [new TripContactInput("   ", null, "+44 20 7946 0958", "not-an-email")],
            }, CancellationToken.None));
        Assert.Contains("Description", error.Errors.Keys);
        Assert.Contains("Contacts[0].Name", error.Errors.Keys);
        Assert.Contains("Contacts[0].Email", error.Errors.Keys);
        Assert.DoesNotContain("Contacts[0].Phone", error.Errors.Keys);
    }

    [Fact]
    public async Task SameDayTripAndInternationalPhonePassValidation()
    {
        var repository = new StubRepository();
        var service = Service(repository);
        await Assert.ThrowsAsync<FamilyTripUnavailableException>(() =>
            service.CreateAsync(Guid.NewGuid(), CreateRequest() with
            {
                EndDate = new DateOnly(2026, 10, 1),
                ExpectedArrivalDate = new DateOnly(2026, 10, 1),
                ExpectedArrivalTime = new TimeOnly(18, 30),
                ExpectedArrivalTimeZone = "UTC",
                Contacts = [new TripContactInput(" Guide ", "Tour guide", "+44 20 7946 0958", "guide@example.com")],
            }, CancellationToken.None));
        Assert.Equal(1, repository.CreateCalls);
    }

    [Fact]
    public async Task AudienceReplacementValidatesExactlyOneCompleteAudienceBeforeRepository()
    {
        var repository = new StubRepository();
        var service = Service(repository);
        var error = await Assert.ThrowsAsync<FamilyTripValidationException>(() =>
            service.PreviewAudienceReplacementAsync(Guid.NewGuid(),
                new ReplaceTripAudienceRequest(TripAudienceType.FamilyGroup, [Guid.NewGuid()]), CancellationToken.None));
        Assert.Contains("AudienceProfileIds", error.Errors.Keys);
        error = await Assert.ThrowsAsync<FamilyTripValidationException>(() =>
            service.PreviewAudienceReplacementAsync(Guid.NewGuid(),
                new ReplaceTripAudienceRequest(TripAudienceType.SelectedPeople, []), CancellationToken.None));
        Assert.Contains("AudienceProfileIds", error.Errors.Keys);
        var duplicate = Guid.NewGuid();
        error = await Assert.ThrowsAsync<FamilyTripValidationException>(() =>
            service.PreviewAudienceReplacementAsync(Guid.NewGuid(),
                new ReplaceTripAudienceRequest(TripAudienceType.SelectedPeople, [duplicate, duplicate]), CancellationToken.None));
        Assert.Contains("AudienceProfileIds", error.Errors.Keys);
    }

    private static FamilyTripService Service(StubRepository repository) => new(
        new AccountAccessor(), repository);

    private static CreateFamilyTripRequest CreateRequest() => new(
        "Family visit", "Atlanta", new DateOnly(2026, 10, 1),
        new DateOnly(2026, 10, 5), null, null, null, null, null, null,
        [Guid.NewGuid()], TripAudienceType.FamilyGroup, [], []);

    private sealed class AccountAccessor : ICurrentAccountAccessor
    {
        public CurrentAccount? Current { get; } = new(Guid.NewGuid());
    }

    private sealed class StubRepository : IFamilyTripRepository
    {
        public Task<FamilyTripView?> CompleteAsync(Guid accountId, Guid tripId,
            Guid version, CancellationToken cancellationToken) => Task.FromResult<FamilyTripView?>(null);
        public int CreateCalls { get; private set; }
        public Task<IReadOnlyList<FamilyTripView>> GetVisibleAsync(Guid accountId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<FamilyTripView>>([]);
        public Task<FamilyTripView?> GetAsync(Guid accountId, Guid tripId, CancellationToken cancellationToken) =>
            Task.FromResult<FamilyTripView?>(null);
        public Task<FamilyTripView?> CreateAsync(Guid accountId, Guid familyGroupId,
            CreateFamilyTripRequest request, CancellationToken cancellationToken)
        {
            CreateCalls++;
            return Task.FromResult<FamilyTripView?>(null);
        }
        public Task<FamilyTripView?> UpdateAsync(Guid accountId, Guid tripId,
            UpdateFamilyTripRequest request, CancellationToken cancellationToken) =>
            Task.FromResult<FamilyTripView?>(null);
        public Task<TripAudiencePreview?> PreviewAudienceReplacementAsync(Guid accountId, Guid tripId,
            ReplaceTripAudienceRequest request, CancellationToken cancellationToken) =>
            Task.FromResult<TripAudiencePreview?>(null);
        public Task<FamilyTripView?> ReplaceAudienceAsync(Guid accountId, Guid tripId,
            ReplaceTripAudienceRequest request, string previewToken, CancellationToken cancellationToken) =>
            Task.FromResult<FamilyTripView?>(null);
    }
}
