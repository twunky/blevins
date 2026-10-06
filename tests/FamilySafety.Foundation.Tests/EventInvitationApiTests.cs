using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FamilySafety.Application.Events;
using FamilySafety.Application.Profiles;
using FamilySafety.Domain.Events;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FamilySafety.Foundation.Tests;

public sealed class EventInvitationApiTests : IClassFixture<EventInvitationApiTests.Factory>
{
    private readonly HttpClient client;
    private readonly Factory factory;
    public EventInvitationApiTests(Factory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
        factory.Account.Current = new(ActorId);
        factory.Repository.Unavailable = false;
        factory.Repository.Conflict = false;
    }
    private static readonly Guid ActorId = Guid.NewGuid();
    private static readonly Guid EventId = Guid.NewGuid();
    private static readonly string[] PendingFields = ["date", "eventId", "invitationId", "name", "time"];
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    { Converters = { new JsonStringEnumConverter() } };
    private static readonly EventInvitationView Row = new(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, null, EventRsvpStatus.Pending, null, Guid.NewGuid());

    [Theory]
    [InlineData("organizer")]
    [InlineData("own")]
    [InlineData("pending")]
    [InlineData("issue")]
    [InlineData("revoke")]
    [InlineData("respond")]
    public async Task EveryEndpointRequiresAccountAndUsesEmptyNondisclosingUnavailable(string action)
    {
        factory.Account.Current = null;
        var unauthenticated = await Send(action);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthenticated.StatusCode);
        factory.Account.Current = new(ActorId);
        factory.Repository.Unavailable = true;
        var unavailable = await Send(action);
        Assert.Equal(HttpStatusCode.NotFound, unavailable.StatusCode);
        Assert.Empty(await unavailable.Content.ReadAsByteArrayAsync());
    }

    [Theory]
    [InlineData("issue")]
    [InlineData("revoke")]
    [InlineData("respond")]
    public async Task ConflictsHaveNoInvitationDisclosure(string action)
    {
        factory.Repository.Conflict = true;
        var response = await Send(action);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task ReadContractsExposeOnlyActorSpecificFieldsAndInitialPending()
    {
        var organizer = await (await Send("organizer")).Content.ReadFromJsonAsync<OrganizerInvitationsView>(JsonOptions);
        Assert.Equal("Invited adult", Assert.Single(organizer!.Invitations).DisplayName);
        var own = await (await Send("own")).Content.ReadFromJsonAsync<OwnEventInvitationView>(JsonOptions);
        Assert.Equal(Row, own!.Invitation);
        var pending = await (await Send("pending")).Content.ReadFromJsonAsync<PendingEventInvitationView[]>();
        Assert.Equal(EventId, Assert.Single(pending!).EventId);
        Assert.Equal(ActorId, factory.Repository.LastAccount);
        using var pendingBody = JsonDocument.Parse(await (await Send("pending")).Content.ReadAsStringAsync());
        Assert.Equal(PendingFields,
            pendingBody.RootElement[0].EnumerateObject().Select(property => property.Name).Order());
        var body = await (await Send("own")).Content.ReadAsStringAsync();
        Assert.DoesNotContain("displayName", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"rsvpStatus\":\"Pending\"", body, StringComparison.Ordinal);
        Assert.Contains("\"rsvpUpdatedAtUtc\":null", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MutationContractsIgnoreForgedActorAndCarryIdentityAndVersion()
    {
        var response = await client.PutAsJsonAsync($"/api/events/{EventId}/invitation/mine/rsvp", new
        {
            invitationId = Row.InvitationId, version = Row.Version, rsvpStatus = "Going",
            accountId = Guid.NewGuid(), inviteeProfileId = Guid.NewGuid(), organizerProfileId = Guid.NewGuid(),
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(ActorId, factory.Repository.LastAccount);
        Assert.Equal(new UpdateEventRsvpRequest(Row.InvitationId, Row.Version, EventRsvpStatus.Going), factory.Repository.LastResponse);
        Assert.Equal(HttpStatusCode.OK, (await Send("issue")).StatusCode);
        Assert.Equal(Row.InviteeProfileId, factory.Repository.LastTarget);
        Assert.Equal(HttpStatusCode.OK, (await Send("revoke")).StatusCode);
        Assert.Equal(Row.Version, factory.Repository.LastVersion);
    }

    [Fact]
    public async Task InvalidStatusStringIsRejected()
    {
        var response = await client.PutAsJsonAsync($"/api/events/{EventId}/invitation/mine/rsvp",
            new { invitationId = Row.InvitationId, version = Row.Version, rsvpStatus = "Unknown" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private Task<HttpResponseMessage> Send(string action) => action switch
    {
        "organizer" => client.GetAsync($"/api/events/{EventId}/invitations"),
        "own" => client.GetAsync($"/api/events/{EventId}/invitation/mine"),
        "pending" => client.GetAsync($"/api/family-groups/{Guid.NewGuid()}/event-invitations/mine"),
        "issue" => client.PostAsJsonAsync($"/api/events/{EventId}/invitations", new IssueEventInvitationRequest(Row.InviteeProfileId)),
        "revoke" => client.PostAsJsonAsync($"/api/events/{EventId}/invitations/{Row.InvitationId}/revoke", new RevokeEventInvitationRequest(Row.Version)),
        _ => client.PutAsJsonAsync($"/api/events/{EventId}/invitation/mine/rsvp", new UpdateEventRsvpRequest(Row.InvitationId, Row.Version, EventRsvpStatus.Going)),
    };

    public sealed class Factory : WebApplicationFactory<Program>
    {
        public Accessor Account { get; } = new();
        public Repository Repository { get; } = new();
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:FamilySafety", "Server=localhost;Database=Unused;Trusted_Connection=True;TrustServerCertificate=True");
            builder.UseSetting("DevelopmentIdentity:Enabled", "false");
            builder.UseSetting("DevelopmentFixtures:Enabled", "false");
            builder.UseSetting("DevelopmentInvitationOutbox:Enabled", "false");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ICurrentAccountAccessor>();
                services.RemoveAll<IEventInvitationRepository>();
                services.AddSingleton<ICurrentAccountAccessor>(Account);
                services.AddSingleton<IEventInvitationRepository>(Repository);
            });
        }
    }
    public sealed class Accessor : ICurrentAccountAccessor { public CurrentAccount? Current { get; set; } }
    public sealed class Repository : IEventInvitationRepository
    {
        public bool Unavailable { get; set; }
        public bool Conflict { get; set; }
        public Guid LastAccount { get; private set; }
        public Guid LastTarget { get; private set; }
        public Guid LastVersion { get; private set; }
        public UpdateEventRsvpRequest? LastResponse { get; private set; }
        private Task<T?> Result<T>(Guid account, T value) where T : class
        {
            LastAccount = account;
            if (Conflict) throw new EventInvitationConflictException();
            return Task.FromResult(Unavailable ? null : value);
        }
        public Task<OrganizerInvitationsView?> GetOrganizerAsync(Guid accountId, Guid eventId, CancellationToken cancellationToken) => Result(accountId,
            new OrganizerInvitationsView([new(Row.InviteeProfileId, "Invited adult")], [new(Row, "Invited adult")]));
        public Task<OwnEventInvitationView?> GetOwnAsync(Guid accountId, Guid eventId, CancellationToken cancellationToken) => Result(accountId, new OwnEventInvitationView(Row));
        public Task<IReadOnlyList<PendingEventInvitationView>?> GetPendingAsync(Guid accountId, Guid familyGroupId, CancellationToken cancellationToken) =>
            Result<IReadOnlyList<PendingEventInvitationView>>(accountId, [new(Row.InvitationId, EventId, "Gathering", new(2020, 1, 1), new(18, 0))]);
        public Task<EventInvitationView?> IssueAsync(Guid accountId, Guid eventId, IssueEventInvitationRequest request, CancellationToken cancellationToken)
        { LastTarget = request.InviteeProfileId; return Result(accountId, Row); }
        public Task<EventInvitationView?> RevokeAsync(Guid accountId, Guid eventId, Guid invitationId, RevokeEventInvitationRequest request, CancellationToken cancellationToken)
        { LastVersion = request.Version; return Result(accountId, Row); }
        public Task<EventInvitationView?> RespondAsync(Guid accountId, Guid eventId, UpdateEventRsvpRequest request, CancellationToken cancellationToken)
        { LastResponse = request; return Result(accountId, Row); }
    }
}
