using System.Net;
using System.Net.Http.Json;
using FamilySafety.Application.Events;
using FamilySafety.Application.Profiles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FamilySafety.Foundation.Tests;

public sealed class EventUpdateApiTests : IClassFixture<EventUpdateApiTests.Factory>
{
    private readonly Factory factory;
    private readonly HttpClient client;
    private static readonly Guid ActorId = Guid.NewGuid(), EventId = Guid.NewGuid(), UpdateId = Guid.NewGuid(), Version = Guid.NewGuid();
    public EventUpdateApiTests(Factory factory)
    {
        this.factory = factory; client = factory.CreateClient(); factory.Account.Current = new(ActorId);
        factory.Repository.Unavailable = false; factory.Repository.Conflict = false; factory.Repository.Invalid = false;
    }
    [Theory]
    [InlineData("read")] [InlineData("create")] [InlineData("edit")] [InlineData("hide")]
    public async Task EndpointsRequireServerAccountAndReturnNondisclosingUnavailable(string action)
    {
        factory.Account.Current = null;
        Assert.Equal(HttpStatusCode.Unauthorized, (await Send(action)).StatusCode);
        factory.Account.Current = new(ActorId); factory.Repository.Unavailable = true;
        var denied = await Send(action);
        Assert.Equal(HttpStatusCode.NotFound, denied.StatusCode); Assert.Empty(await denied.Content.ReadAsByteArrayAsync());
    }
    [Theory]
    [InlineData("create")] [InlineData("edit")] [InlineData("hide")]
    public async Task WritesBindEventUpdateAndVersionAndReturnEmptyConflict(string action)
    {
        Assert.Equal(action == "hide" ? HttpStatusCode.NoContent : HttpStatusCode.OK, (await Send(action)).StatusCode);
        Assert.Equal(ActorId, factory.Repository.LastAccount); Assert.Equal(EventId, factory.Repository.LastEvent);
        if (action != "create") { Assert.Equal(Version, factory.Repository.LastVersion); Assert.Equal(UpdateId, factory.Repository.LastUpdate); }
        factory.Repository.Conflict = true;
        var conflict = await Send(action); Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        Assert.Empty(await conflict.Content.ReadAsByteArrayAsync());
    }
    [Fact]
    public async Task ContractContainsOnlyVisibleProjectionValidatesTextAndIgnoresForgedIdentityAndFamily()
    {
        var body = await (await Send("read")).Content.ReadAsStringAsync();
        Assert.Contains("\"authorName\":\"Organizer\"", body); Assert.Contains("\"updatedAtUtc\":null", body);
        Assert.DoesNotContain("hidden", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("revision", body, StringComparison.OrdinalIgnoreCase);
        var forged = await client.PostAsJsonAsync($"/api/events/{EventId}/updates", new {
            text = "Update", accountId = Guid.NewGuid(), authorProfileId = Guid.NewGuid(), familyGroupId = Guid.NewGuid(),
        });
        Assert.Equal(HttpStatusCode.OK, forged.StatusCode); Assert.Equal(ActorId, factory.Repository.LastAccount);
        factory.Repository.Invalid = true;
        var invalid = await Send("create"); Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Contains("Text", await invalid.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/events/{EventId}/updates/{UpdateId}/history")).StatusCode);
    }
    private Task<HttpResponseMessage> Send(string action)
    {
        var url = $"/api/events/{EventId}/updates";
        return action switch {
            "read" => client.GetAsync(url),
            "create" => client.PostAsJsonAsync(url, new { text = "Update" }),
            "edit" => client.PutAsJsonAsync($"{url}/{UpdateId}", new { version = Version, text = "Edited" }),
            _ => client.PostAsJsonAsync($"{url}/{UpdateId}/hide", new { version = Version }),
        };
    }
    public sealed class Factory : WebApplicationFactory<Program>
    {
        public Accessor Account { get; } = new();
        public Repository Repository { get; } = new();
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:FamilySafety", "Server=localhost;Database=Unused;Trusted_Connection=True;TrustServerCertificate=True");
            builder.UseSetting("DevelopmentIdentity:Enabled", "false"); builder.UseSetting("DevelopmentFixtures:Enabled", "false");
            builder.UseSetting("DevelopmentInvitationOutbox:Enabled", "false");
            builder.ConfigureTestServices(services => {
                services.RemoveAll<ICurrentAccountAccessor>(); services.RemoveAll<IEventUpdateRepository>();
                services.AddSingleton<ICurrentAccountAccessor>(Account); services.AddSingleton<IEventUpdateRepository>(Repository);
            });
        }
    }
    public sealed class Accessor : ICurrentAccountAccessor { public CurrentAccount? Current { get; set; } }
    public sealed class Repository : IEventUpdateRepository
    {
        public bool Unavailable { get; set; } public bool Conflict { get; set; } public bool Invalid { get; set; }
        public Guid LastAccount { get; private set; } public Guid LastEvent { get; private set; }
        public Guid LastVersion { get; private set; } public Guid LastUpdate { get; private set; }
        private static readonly EventUpdateView Row = new(UpdateId, ActorId, "Organizer", "Current text", DateTimeOffset.UtcNow, null, Version);
        private Task<T?> Result<T>(Guid accountId, Guid eventId, T value) where T : class
        {
            LastAccount = accountId; LastEvent = eventId;
            if (Conflict) throw new EventUpdateConflictException();
            if (Invalid) throw new FamilyEventValidationException(new Dictionary<string, string[]> { ["Text"] = ["Enter Update text."] });
            return Task.FromResult(Unavailable ? null : value);
        }
        public Task<EventUpdatesView?> GetAsync(Guid accountId, Guid eventId, CancellationToken ct) => Result(accountId, eventId, new EventUpdatesView(false, [Row]));
        public Task<EventUpdateView?> CreateAsync(Guid accountId, Guid eventId, CreateEventUpdateRequest request, CancellationToken ct) => Result(accountId, eventId, Row);
        public Task<EventUpdateView?> EditAsync(Guid accountId, Guid eventId, Guid updateId, EditEventUpdateRequest request, CancellationToken ct)
        { LastUpdate = updateId; LastVersion = request.Version; return Result(accountId, eventId, Row); }
        public async Task<bool> HideAsync(Guid accountId, Guid eventId, Guid updateId, Guid version, CancellationToken ct)
        { LastUpdate = updateId; LastVersion = version; return await Result(accountId, eventId, Row) is not null; }
    }
}
