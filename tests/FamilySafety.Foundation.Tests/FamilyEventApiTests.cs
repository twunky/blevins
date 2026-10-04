using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FamilySafety.Application.Events;
using FamilySafety.Application.Profiles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FamilySafety.Foundation.Tests;

public sealed class FamilyEventApiTests : IClassFixture<FamilyEventApiTests.EventApiFactory>
{
    private readonly HttpClient client;
    private readonly StubFamilyEventService service;

    public FamilyEventApiTests(EventApiFactory factory)
    {
        client = factory.CreateClient();
        service = factory.Service;
        service.Reset();
    }

    [Fact]
    public async Task UpdateEndpointPassesOnlyEventDetailsToService()
    {
        var eventId = Guid.NewGuid();
        service.Updated = new FamilyEventView(
            eventId,
            StubFamilyEventService.GroupId,
            "Updated dinner",
            "Bring a dish.",
            new DateOnly(2026, 10, 4),
            new TimeOnly(18, 30),
            "Community room",
            Guid.NewGuid(),
            "Organizer",
            true);

        var response = await client.PutAsJsonAsync(
            $"/api/family-groups/{StubFamilyEventService.GroupId:D}/events/{eventId:D}",
            new
            {
                name = "Updated dinner",
                description = "Bring a dish.",
                date = "2026-10-04",
                time = "18:30",
                location = "Community room",
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(StubFamilyEventService.GroupId, service.LastGroupId);
        Assert.Equal(eventId, service.LastEventId);
        Assert.Equal("Updated dinner", service.LastUpdateRequest?.Name);
        Assert.Equal(new DateOnly(2026, 10, 4), service.LastUpdateRequest?.Date);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"canEdit\":true", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UpdateEndpointHidesUnavailableEventOrEditor()
    {
        service.Unavailable = true;
        var response = await client.PutAsJsonAsync(
            $"/api/family-groups/{StubFamilyEventService.GroupId:D}/events/{Guid.NewGuid():D}",
            new
            {
                name = "Updated dinner",
                description = "Bring a dish.",
                date = "2026-10-04",
                time = "18:30",
                location = "Community room",
            });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ItemEndpointReturnsOnlyTheListEntryPropertiesForAnAuthorizedEvent()
    {
        var eventId = Guid.NewGuid();
        service.Item = new FamilyEventView(
            eventId, StubFamilyEventService.GroupId, "Family dinner", "Bring a dish.",
            new DateOnly(2026, 10, 3), new TimeOnly(18, 30), "Community room",
            Guid.NewGuid(), "Organizer", false);

        var response = await client.GetAsync($"/api/events/{eventId:D}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(eventId, service.LastEventId);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(
            ["canEdit", "date", "description", "eventId", "familyGroupId", "location", "name",
                "organizerDisplayName", "organizerProfileId", "time"],
            json.RootElement.EnumerateObject().Select(property => property.Name).Order(StringComparer.Ordinal));
        Assert.Equal(StubFamilyEventService.GroupId, json.RootElement.GetProperty("familyGroupId").GetGuid());
        Assert.False(json.RootElement.GetProperty("canEdit").GetBoolean());
    }

    [Fact]
    public async Task ItemEndpointGivesMissingAndUnauthorizedEventsTheSameEmptyNotFound()
    {
        service.Item = new FamilyEventView(
            Guid.NewGuid(), StubFamilyEventService.GroupId, "Family dinner", "Bring a dish.",
            new DateOnly(2026, 10, 3), new TimeOnly(18, 30), "Community room",
            Guid.NewGuid(), "Organizer", true);

        var missing = await client.GetAsync($"/api/events/{Guid.NewGuid():D}");
        var unauthorized = await client.GetAsync($"/api/events/{Guid.NewGuid():D}");
        var notAGuid = await client.GetAsync("/api/events/not-an-id");

        foreach (var response in new[] { missing, unauthorized, notAGuid })
        {
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Empty(await response.Content.ReadAsByteArrayAsync());
        }

        Assert.Equal(missing.Content.Headers.ToString(), unauthorized.Content.Headers.ToString());
        Assert.Null(missing.Content.Headers.ContentType);
    }

    [Fact]
    public async Task ItemEndpointRequiresCurrentAccount()
    {
        service.NoCurrentAccount = true;

        var response = await client.GetAsync($"/api/events/{Guid.NewGuid():D}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task FamilyScopedListRouteStillAnswers()
    {
        var response = await client.GetAsync(
            $"/api/family-groups/{StubFamilyEventService.GroupId:D}/events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("[]", await response.Content.ReadAsStringAsync());
    }

    public sealed class EventApiFactory : WebApplicationFactory<Program>
    {
        public StubFamilyEventService Service { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:FamilySafety", "Server=localhost;Database=Unused;Trusted_Connection=True;TrustServerCertificate=True");
            builder.UseSetting("DevelopmentIdentity:Enabled", "false");
            builder.UseSetting("DevelopmentFixtures:Enabled", "false");
            builder.UseSetting("DevelopmentInvitationOutbox:Enabled", "false");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IFamilyEventService>();
                services.AddSingleton<IFamilyEventService>(Service);
            });
        }
    }

    public sealed class StubFamilyEventService : IFamilyEventService
    {
        public static readonly Guid GroupId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        public Guid LastGroupId { get; private set; }
        public Guid LastEventId { get; private set; }
        public UpdateFamilyEventRequest? LastUpdateRequest { get; private set; }
        public FamilyEventView? Updated { get; set; }
        public bool Unavailable { get; set; }
        public FamilyEventView? Item { get; set; }
        public bool NoCurrentAccount { get; set; }

        public void Reset()
        {
            LastGroupId = Guid.Empty;
            LastEventId = Guid.Empty;
            LastUpdateRequest = null;
            Updated = null;
            Unavailable = false;
            Item = null;
            NoCurrentAccount = false;
        }

        public Task<IReadOnlyList<FamilyEventView>> GetVisibleAsync(
            Guid familyGroupId,
            CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<FamilyEventView>>([]);

        // Mirrors the real service: only the one readable Event is returned; any other
        // ID, missing or unauthorized alike, is unavailable.
        public Task<FamilyEventView> GetAsync(Guid eventId, CancellationToken cancellationToken)
        {
            LastEventId = eventId;
            if (NoCurrentAccount) throw new CurrentAccountUnavailableException();
            if (Item is null || Item.EventId != eventId) throw new FamilyEventUnavailableException();
            return Task.FromResult(Item);
        }

        public Task<FamilyEventView> CreateAsync(
            Guid familyGroupId,
            CreateFamilyEventRequest request,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException("Event creation is not exercised in this API test.");

        public Task<FamilyEventView> UpdateAsync(
            Guid familyGroupId,
            Guid eventId,
            UpdateFamilyEventRequest request,
            CancellationToken cancellationToken)
        {
            LastGroupId = familyGroupId;
            LastEventId = eventId;
            LastUpdateRequest = request;
            if (Unavailable) throw new FamilyEventUnavailableException();
            return Task.FromResult(Updated!);
        }
    }
}
