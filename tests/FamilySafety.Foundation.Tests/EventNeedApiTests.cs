using System.Net;
using System.Net.Http.Json;
using FamilySafety.Application.Events;
using FamilySafety.Application.Profiles;
using FamilySafety.Domain.Events;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FamilySafety.Foundation.Tests;

public sealed class EventNeedApiTests : IClassFixture<EventNeedApiTests.Factory>
{
    private readonly Factory factory;
    private readonly HttpClient client;
    private static readonly Guid ActorId = Guid.NewGuid();
    private static readonly Guid EventId = Guid.NewGuid();
    private static readonly Guid NeedId = Guid.NewGuid();
    private static readonly Guid Version = Guid.NewGuid();
    public EventNeedApiTests(Factory factory)
    {
        this.factory = factory; client = factory.CreateClient(); factory.Account.Current = new(ActorId);
        factory.Repository.Unavailable = false; factory.Repository.Conflict = false; factory.Repository.Invalid = false;
    }

    [Theory]
    [InlineData("read")] [InlineData("create")] [InlineData("edit")] [InlineData("remove")]
    [InlineData("claim")] [InlineData("release")] [InlineData("assign")] [InlineData("accept")]
    [InlineData("decline")] [InlineData("complete")]
    public async Task EveryEndpointRequiresServerAccountAndReturnsNondisclosingUnavailable(string action)
    {
        factory.Account.Current = null;
        Assert.Equal(HttpStatusCode.Unauthorized, (await Send(action)).StatusCode);
        factory.Account.Current = new(ActorId); factory.Repository.Unavailable = true;
        var denied = await Send(action);
        Assert.Equal(HttpStatusCode.NotFound, denied.StatusCode); Assert.Empty(await denied.Content.ReadAsByteArrayAsync());
    }

    [Theory]
    [InlineData("create")] [InlineData("edit")] [InlineData("remove")] [InlineData("claim")]
    [InlineData("release")] [InlineData("assign")] [InlineData("accept")] [InlineData("decline")] [InlineData("complete")]
    public async Task MutationsBindTheVersionAndRouteActionAndReturnEmptyConflict(string action)
    {
        var ok = await Send(action);
        Assert.Equal(action == "remove" ? HttpStatusCode.NoContent : HttpStatusCode.OK, ok.StatusCode);
        Assert.Equal(ActorId, factory.Repository.LastAccount);
        if (action != "create") Assert.Equal(Version, factory.Repository.LastVersion);
        if (action is not ("create" or "edit" or "remove")) Assert.Equal(action, factory.Repository.LastAction);
        factory.Repository.Conflict = true;
        var conflict = await Send(action);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode); Assert.Empty(await conflict.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task ContractUsesStringStatesValidationAndIgnoresForgedActor()
    {
        var body = await (await Send("read")).Content.ReadAsStringAsync();
        Assert.Contains("\"status\":\"Open\"", body); Assert.Contains("\"responsibleProfileId\":null", body);
        var forged = await client.PostAsJsonAsync($"/api/events/{EventId}/needs/{NeedId}/claim", new { version = Version, accountId = Guid.NewGuid(), profileId = Guid.NewGuid() });
        Assert.Equal(HttpStatusCode.OK, forged.StatusCode); Assert.Equal(ActorId, factory.Repository.LastAccount);
        factory.Repository.Invalid = true;
        var invalid = await Send("create"); Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Contains("Title", await invalid.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync($"/api/events/{EventId}/needs", new { title = "Need", category = "Unknown" })).StatusCode);
    }

    private Task<HttpResponseMessage> Send(string action)
    {
        var url = $"/api/events/{EventId}/needs";
        return action switch
        {
            "read" => client.GetAsync(url),
            "create" => client.PostAsJsonAsync(url, new { title = "Need", detail = (string?)null, category = "Help" }),
            "edit" => client.PutAsJsonAsync($"{url}/{NeedId}", new { version = Version, title = "Need", detail = (string?)null, category = "Help" }),
            "assign" => client.PostAsJsonAsync($"{url}/{NeedId}/assign", new { version = Version, profileId = ActorId }),
            _ => client.PostAsJsonAsync($"{url}/{NeedId}/{action}", new { version = Version }),
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
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ICurrentAccountAccessor>(); services.RemoveAll<IEventNeedRepository>();
                services.AddSingleton<ICurrentAccountAccessor>(Account); services.AddSingleton<IEventNeedRepository>(Repository);
            });
        }
    }
    public sealed class Accessor : ICurrentAccountAccessor { public CurrentAccount? Current { get; set; } }
    public sealed class Repository : IEventNeedRepository
    {
        public bool Unavailable { get; set; } public bool Conflict { get; set; } public bool Invalid { get; set; }
        public Guid LastAccount { get; private set; } public Guid LastVersion { get; private set; } public string? LastAction { get; private set; }
        private static readonly EventNeedView Row = new(NeedId, "Need", null, EventNeedCategory.Help, EventNeedStatus.Open, null, null, false,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, Version, []);
        private Task<T?> Result<T>(Guid accountId, T value) where T : class
        {
            LastAccount = accountId;
            if (Conflict) throw new EventNeedConflictException();
            if (Invalid) throw new FamilyEventValidationException(new Dictionary<string, string[]> { ["Title"] = ["Enter a title."] });
            return Task.FromResult(Unavailable ? null : value);
        }
        public Task<EventNeedsView?> GetAsync(Guid accountId, Guid eventId, CancellationToken ct) => Result(accountId, new EventNeedsView(false, true, [], [Row]));
        public Task<EventNeedView?> CreateAsync(Guid accountId, Guid eventId, CreateEventNeedRequest request, CancellationToken ct) => Result(accountId, Row);
        public Task<EventNeedView?> EditAsync(Guid accountId, Guid eventId, Guid needId, EditEventNeedRequest request, CancellationToken ct)
        { LastVersion = request.Version; return Result(accountId, Row); }
        public async Task<bool> RemoveAsync(Guid accountId, Guid eventId, Guid needId, Guid version, CancellationToken ct)
        { LastVersion = version; return await Result(accountId, Row) is not null; }
        public Task<EventNeedView?> ActAsync(Guid accountId, Guid eventId, Guid needId, Guid version, string action, Guid? assigneeId, CancellationToken ct)
        { LastVersion = version; LastAction = action; return Result(accountId, Row); }
    }
}
