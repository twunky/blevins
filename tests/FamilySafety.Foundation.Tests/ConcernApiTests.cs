using System.Net;
using System.Net.Http.Json;
using FamilySafety.Api.Concerns;
using FamilySafety.Application.Concerns;
using FamilySafety.Application.Profiles;
using FamilySafety.Domain.Concerns;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FamilySafety.Foundation.Tests;

public sealed class ConcernApiTests : IClassFixture<ConcernApiTests.ApiFactory>
{
    private readonly HttpClient client;
    private readonly StubConcernService service;

    public ConcernApiTests(ApiFactory factory)
    {
        client = factory.CreateClient();
        service = factory.Service;
        service.Reset();
    }

    [Fact]
    public async Task RoutesMapUnavailableAndValidationWithoutExposingConcernData()
    {
        service.Unavailable = true;
        var id = Guid.NewGuid();
        var requests = new[]
        {
            client.GetAsync($"/api/concerns/{id:D}"),
            client.PutAsJsonAsync($"/api/concerns/{id:D}/participants", new { participantProfileIds = Array.Empty<Guid>() }),
            client.PostAsJsonAsync($"/api/concerns/{id:D}/entries", new
            {
                kind = "Unknown", category = "Unknown", text = "Unknown", sourceUnknown = true,
            }),
            client.PostAsJsonAsync($"/api/concerns/{id:D}/resolve", new { note = "Closed" }),
        };
        foreach (var request in requests)
        {
            using var response = await request;
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.DoesNotContain("secret subject", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
        }
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.PutAsJsonAsync($"/api/concerns/{id:D}/entries/{Guid.NewGuid():D}", new { text = "edit" })).StatusCode);
        service.Unavailable = false;
        Assert.Equal(HttpStatusCode.Created,
            (await client.PostAsJsonAsync($"/api/family-groups/{Guid.NewGuid():D}/concerns", new
            {
                subjectProfileId = Guid.NewGuid(), reason = "Not reachable yet", participantProfileIds = Array.Empty<Guid>(),
            })).StatusCode);

        service.Unauthenticated = true;
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.GetAsync("/api/concerns")).StatusCode);
    }

    public sealed class ApiFactory : WebApplicationFactory<Program>
    {
        public StubConcernService Service { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:FamilySafety", "Server=localhost;Database=Unused;Trusted_Connection=True;TrustServerCertificate=True");
            builder.UseSetting("DevelopmentIdentity:Enabled", "false");
            builder.UseSetting("DevelopmentFixtures:Enabled", "false");
            builder.UseSetting("DevelopmentInvitationOutbox:Enabled", "false");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IConcernService>();
                services.AddSingleton<IConcernService>(Service);
            });
        }
    }

    public sealed class StubConcernService : IConcernService
    {
        public bool Unavailable { get; set; }
        public bool Unauthenticated { get; set; }
        public void Reset() { Unavailable = false; Unauthenticated = false; }
        private static ConcernView Result() => new(Guid.NewGuid(), Guid.NewGuid(), "Family", Guid.NewGuid(),
            "Creator", Guid.NewGuid(), "Subject", "Reason", ConcernState.Open,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null, null, null, null, null, [], []);
        private T Check<T>(T value)
        {
            if (Unauthenticated) throw new CurrentAccountUnavailableException();
            if (Unavailable) throw new ConcernUnavailableException();
            return value;
        }
        public Task<IReadOnlyList<ConcernView>> GetVisibleAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<ConcernView>>(Check<IReadOnlyList<ConcernView>>([]));
        public Task<ConcernView> GetAsync(Guid concernId, CancellationToken cancellationToken) =>
            Task.FromResult(Check(Result()));
        public Task<ConcernView> CreateAsync(Guid familyGroupId, CreateConcernRequest request, CancellationToken cancellationToken) =>
            Task.FromResult(Check(Result()));
        public Task<ConcernView> ReplaceParticipantsAsync(Guid concernId, ReplaceConcernParticipantsRequest request, CancellationToken cancellationToken) =>
            Task.FromResult(Check(Result()));
        public Task<ConcernView> AddEntryAsync(Guid concernId, AddConcernEntryRequest request, CancellationToken cancellationToken) =>
            Task.FromResult(Check(Result()));
        public Task<ConcernView> ResolveAsync(Guid concernId, ResolveConcernRequest request, CancellationToken cancellationToken) =>
            Task.FromResult(Check(Result()));
    }
}
