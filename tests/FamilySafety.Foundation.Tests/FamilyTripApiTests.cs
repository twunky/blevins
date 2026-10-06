using System.Net;
using System.Net.Http.Json;
using FamilySafety.Application.Trips;
using FamilySafety.Application.Profiles;
using FamilySafety.Domain.Trips;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FamilySafety.Foundation.Tests;

public sealed class FamilyTripApiTests : IClassFixture<FamilyTripApiTests.TripApiFactory>
{
    private readonly HttpClient client;
    private readonly StubTripService service;

    public FamilyTripApiTests(TripApiFactory factory)
    {
        client = factory.CreateClient();
        service = factory.Service;
        service.Reset();
    }

    [Fact]
    public async Task ExplicitCompletionRouteUsesVersionAndStandardNondisclosingAndConflictResponses()
    {
        var tripId = Guid.NewGuid(); var version = Guid.NewGuid();
        service.Result = View(Guid.NewGuid()) with { IsCompleted = true, Version = Guid.NewGuid() };
        var response = await client.PostAsJsonAsync($"/api/trips/{tripId:D}/complete", new { version });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(tripId, service.LastTripId);
        Assert.Equal(version, service.LastComplete!.Version);
        using var json = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(json.RootElement.GetProperty("isCompleted").GetBoolean());
        service.Invalid = true;
        Assert.Equal(HttpStatusCode.Conflict,
            (await client.PostAsJsonAsync($"/api/trips/{tripId:D}/complete", new { version })).StatusCode);
        service.Invalid = false; service.Unavailable = true;
        response = await client.PostAsJsonAsync($"/api/trips/{tripId:D}/complete", new { version });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("", await response.Content.ReadAsStringAsync());
        service.Unavailable = false; service.Unauthenticated = true;
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.PostAsJsonAsync($"/api/trips/{tripId:D}/complete", new { version })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.PostAsJsonAsync($"/api/trips/{tripId:D}/reopen", new { version })).StatusCode);
    }

    [Fact]
    public async Task CreateEndpointPassesTripFieldsAndInitialAudience()
    {
        var groupId = Guid.NewGuid();
        var travelerId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        service.Result = View(groupId);
        var response = await client.PostAsJsonAsync($"/api/family-groups/{groupId:D}/trips", new
        {
            title = "Family visit", primaryDestination = "Atlanta",
            startDate = "2026-10-01", endDate = "2026-10-05",
            expectedArrivalDate = "2026-10-01", expectedArrivalTime = "17:00",
            expectedArrivalTimeZone = "UTC",
            travelerProfileIds = new[] { travelerId }, audienceType = "SelectedPeople",
            audienceProfileIds = new[] { recipientId },
            contacts = new[] { new { name = "Guide", phone = "+44 20 7946 0958" } },
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(groupId, service.LastGroupId);
        Assert.Equal(TripAudienceType.SelectedPeople, service.LastCreate?.AudienceType);
        Assert.Equal(travelerId, Assert.Single(service.LastCreate!.TravelerProfileIds!));
        Assert.Equal(recipientId, Assert.Single(service.LastCreate.AudienceProfileIds!));
        Assert.Equal("UTC", service.LastCreate.ExpectedArrivalTimeZone);
    }

    [Fact]
    public async Task CoreEditContractCannotBindAudienceChange()
    {
        var tripId = Guid.NewGuid();
        service.Result = View(Guid.NewGuid());
        var response = await client.PutAsJsonAsync($"/api/trips/{tripId:D}", new
        {
            title = "Changed", primaryDestination = "Atlanta",
            startDate = "2026-10-01", endDate = "2026-10-05",
            travelerProfileIds = new[] { Guid.NewGuid() },
            audienceType = "FamilyGroup", audienceProfileIds = new[] { Guid.NewGuid() },
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(tripId, service.LastTripId);
        Assert.NotNull(service.LastUpdate);
        Assert.DoesNotContain("Audience", typeof(UpdateFamilyTripRequest).GetProperties()
            .Select(property => property.Name).Aggregate(string.Empty, (all, name) => all + name),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task AudiencePreviewAndConfirmationUseDedicatedCreatorOnlyPaths()
    {
        var tripId = Guid.NewGuid();
        var recipient = Guid.NewGuid();
        var audience = new { audienceType = "SelectedPeople", audienceProfileIds = new[] { recipient } };
        service.Result = View(Guid.NewGuid());
        var preview = await client.PostAsJsonAsync($"/api/trips/{tripId:D}/audience/preview", audience);
        Assert.Equal(HttpStatusCode.OK, preview.StatusCode);
        var previewJson = await preview.Content.ReadAsStringAsync();
        Assert.DoesNotContain("location", previewJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("check-in", previewJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sharingSession", previewJson, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(tripId, service.LastTripId);
        Assert.Equal(recipient, Assert.Single(service.LastAudienceRequest!.AudienceProfileIds!));

        var confirm = await client.PostAsJsonAsync($"/api/trips/{tripId:D}/audience/confirm", new
        {
            audienceType = "SelectedPeople", audienceProfileIds = new[] { recipient }, previewToken = "preview-token",
        });
        Assert.Equal(HttpStatusCode.OK, confirm.StatusCode);
        Assert.Equal("preview-token", service.LastPreviewToken);
        Assert.Equal(tripId, service.LastTripId);

        service.Unauthenticated = true;
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.PostAsJsonAsync($"/api/trips/{tripId:D}/audience/preview", audience)).StatusCode);
        service.Unauthenticated = false;
        service.Unavailable = true;
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.PostAsJsonAsync($"/api/trips/{tripId:D}/audience/preview", audience)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.PostAsJsonAsync($"/api/trips/{tripId:D}/audience/confirm", new
            {
                audienceType = "SelectedPeople", audienceProfileIds = new[] { recipient }, previewToken = "x",
            })).StatusCode);
    }

    [Fact]
    public async Task StaleAudienceConfirmationReturnsConflict()
    {
        service.Result = View(Guid.NewGuid());
        service.Invalid = true;
        var response = await client.PostAsJsonAsync($"/api/trips/{Guid.NewGuid():D}/audience/confirm", new
        {
            audienceType = "FamilyGroup", audienceProfileIds = Array.Empty<Guid>(), previewToken = "stale",
        });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("refresh the preview", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnavailableTripAndValidationUseEstablishedResponses()
    {
        service.Unavailable = true;
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/trips/{Guid.NewGuid():D}")).StatusCode);
        service.Unavailable = false;
        service.Invalid = true;
        var response = await client.PostAsJsonAsync($"/api/family-groups/{Guid.NewGuid():D}/trips", new
        {
            title = "", primaryDestination = "Atlanta",
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("title", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnauthenticatedAndUnauthorizedReadsDoNotReturnTripDetails()
    {
        var tripId = Guid.NewGuid();
        service.Unauthenticated = true;
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.GetAsync($"/api/trips/{tripId:D}")).StatusCode);
        service.Unauthenticated = false;
        service.Unavailable = true;
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/trips/{tripId:D}")).StatusCode);
    }

    private static FamilyTripView View(Guid groupId) => new(
        Guid.NewGuid(), groupId, "Family", Guid.NewGuid(), "Creator", true,
        "Family visit", "Atlanta", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5),
        null, null, null, null, null, null, TripAudienceType.SelectedPeople, [], [], []);

    public sealed class TripApiFactory : WebApplicationFactory<Program>
    {
        public StubTripService Service { get; } = new();
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:FamilySafety", "Server=localhost;Database=Unused;Trusted_Connection=True;TrustServerCertificate=True");
            builder.UseSetting("DevelopmentIdentity:Enabled", "false");
            builder.UseSetting("DevelopmentFixtures:Enabled", "false");
            builder.UseSetting("DevelopmentInvitationOutbox:Enabled", "false");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IFamilyTripService>();
                services.AddSingleton<IFamilyTripService>(Service);
            });
        }
    }

    public sealed class StubTripService : IFamilyTripService
    {
        public CompleteFamilyTripRequest? LastComplete { get; private set; }
        public Task<FamilyTripView> CompleteAsync(Guid tripId, CompleteFamilyTripRequest request,
            CancellationToken cancellationToken)
        {
            LastTripId = tripId; LastComplete = request;
            if (Unauthenticated) throw new CurrentAccountUnavailableException();
            if (Unavailable) throw new FamilyTripUnavailableException();
            if (Invalid) throw new FamilyTripCompletionConflictException();
            return Task.FromResult(Result!);
        }
        public Guid LastGroupId { get; private set; }
        public Guid LastTripId { get; private set; }
        public CreateFamilyTripRequest? LastCreate { get; private set; }
        public UpdateFamilyTripRequest? LastUpdate { get; private set; }
        public FamilyTripView? Result { get; set; }
        public bool Invalid { get; set; }
        public bool Unavailable { get; set; }
        public bool Unauthenticated { get; set; }
        public void Reset()
        {
            LastGroupId = Guid.Empty; LastTripId = Guid.Empty;
            LastCreate = null; LastUpdate = null; LastAudienceRequest = null; LastPreviewToken = null; Result = null;
            Invalid = false; Unavailable = false; Unauthenticated = false;
        }
        public Task<IReadOnlyList<FamilyTripView>> GetVisibleAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<FamilyTripView>>([]);
        public Task<FamilyTripView> GetAsync(Guid tripId, CancellationToken cancellationToken) =>
            Unauthenticated ? throw new CurrentAccountUnavailableException()
            : Unavailable ? throw new FamilyTripUnavailableException()
            : Task.FromResult(Result!);
        public Task<FamilyTripView> CreateAsync(Guid familyGroupId, CreateFamilyTripRequest request, CancellationToken cancellationToken)
        {
            LastGroupId = familyGroupId; LastCreate = request;
            if (Invalid) throw new FamilyTripValidationException(new Dictionary<string, string[]> { ["Title"] = ["Enter a title."] });
            return Task.FromResult(Result!);
        }
        public Task<FamilyTripView> UpdateAsync(Guid tripId, UpdateFamilyTripRequest request, CancellationToken cancellationToken)
        {
            LastTripId = tripId; LastUpdate = request;
            return Task.FromResult(Result!);
        }
        public Task<TripAudiencePreview> PreviewAudienceReplacementAsync(Guid tripId,
            ReplaceTripAudienceRequest request, CancellationToken cancellationToken)
        {
            LastTripId = tripId; LastAudienceRequest = request;
            if (Unauthenticated) throw new CurrentAccountUnavailableException();
            if (Unavailable) throw new FamilyTripUnavailableException();
            if (Invalid) throw new FamilyTripValidationException(new Dictionary<string, string[]> { ["AudienceProfileIds"] = ["Invalid audience."] });
            return Task.FromResult(new TripAudiencePreview("preview-token", request.AudienceType!.Value, [], [], []));
        }
        public ReplaceTripAudienceRequest? LastAudienceRequest { get; private set; }
        public string? LastPreviewToken { get; private set; }
        public Task<FamilyTripView> ReplaceAudienceAsync(Guid tripId, ReplaceTripAudienceRequest request,
            string previewToken, CancellationToken cancellationToken)
        {
            LastTripId = tripId; LastAudienceRequest = request; LastPreviewToken = previewToken;
            if (Unauthenticated) throw new CurrentAccountUnavailableException();
            if (Unavailable) throw new FamilyTripUnavailableException();
            if (Invalid) throw new FamilyTripAudienceConflictException();
            return Task.FromResult(Result!);
        }
    }
}
