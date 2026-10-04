using System.Net;
using System.Net.Http.Json;
using FamilySafety.Application.Incidents;
using FamilySafety.Application.Profiles;
using FamilySafety.Domain.Incidents;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FamilySafety.Foundation.Tests;

public sealed class IncidentApiTests : IClassFixture<IncidentApiTests.ApiFactory>
{
    private static readonly Guid IncidentId = Guid.NewGuid();
    private static readonly Guid TaskId = Guid.NewGuid();
    private readonly HttpClient client;
    private readonly StubIncidentService service;

    public IncidentApiTests(ApiFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        client = factory.CreateClient();
        service = factory.Service;
        service.Reset();
    }

    [Fact]
    public async Task EveryRouteMapsUnavailableToTheSameNondisclosing404()
    {
        service.Unavailable = true;
        var bodies = new List<string>();
        foreach (var send in Routes())
        {
            using var response = await send(client);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            bodies.Add(await response.Content.ReadAsStringAsync());
        }
        Assert.Single(bodies.Distinct());
        Assert.DoesNotContain("Subject", bodies[0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EveryRouteMapsMissingAccountTo401()
    {
        service.Unauthenticated = true;
        using (var list = await client.GetAsync(new Uri("/api/incidents", UriKind.Relative)))
            Assert.Equal(HttpStatusCode.Unauthorized, list.StatusCode);
        foreach (var send in Routes())
        {
            using var response = await send(client);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }

    [Fact]
    public async Task SuccessfulRoutesReturnCreatedOrOk()
    {
        using (var list = await client.GetAsync(new Uri("/api/incidents", UriKind.Relative)))
            Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var expected = new[]
        {
            HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.Created,
            HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK,
        };
        var routes = Routes();
        for (var i = 0; i < routes.Count; i++)
        {
            using var response = await routes[i](client);
            Assert.Equal(expected[i], response.StatusCode);
        }
        using var created = await routes[1](client);
        Assert.StartsWith("/api/incidents/", created.Headers.Location!.OriginalString, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ValidationFailuresReturnProblemDetails()
    {
        service.Invalid = true;
        foreach (var send in Routes().Skip(1))
        {
            using var response = await send(client);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Reason", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task NoEditDeleteReopenReassignOrAuditRoutesExist()
    {
        var id = IncidentId.ToString("D");
        var entry = Guid.NewGuid().ToString("D");
        var task = TaskId.ToString("D");
        var attempts = new Func<Task<HttpResponseMessage>>[]
        {
            () => client.PutAsJsonAsync($"/api/incidents/{id}/entries/{entry}", new { text = "edit" }),
            () => client.DeleteAsync(new Uri($"/api/incidents/{id}/entries/{entry}", UriKind.Relative)),
            () => client.DeleteAsync(new Uri($"/api/incidents/{id}", UriKind.Relative)),
            () => client.PostAsJsonAsync($"/api/incidents/{id}/reopen", new { }),
            () => client.PostAsJsonAsync($"/api/incidents/{id}/tasks/{task}/reassign", new { }),
            () => client.PutAsJsonAsync($"/api/incidents/{id}/tasks/{task}", new { description = "edit" }),
            () => client.GetAsync(new Uri($"/api/incidents/{id}/audit", UriKind.Relative)),
            () => client.PostAsJsonAsync($"/api/concerns/{Guid.NewGuid():D}/incidents", new { }),
        };
        foreach (var attempt in attempts)
        {
            using var response = await attempt();
            Assert.True(response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed,
                $"Unexpected {response.StatusCode} for {response.RequestMessage?.RequestUri}");
        }
    }

    private static List<Func<HttpClient, Task<HttpResponseMessage>>> Routes()
    {
        var id = IncidentId.ToString("D");
        var task = TaskId.ToString("D");
        return
        [
            c => c.GetAsync(new Uri($"/api/incidents/{id}", UriKind.Relative)),
            c => c.PostAsJsonAsync($"/api/family-groups/{Guid.NewGuid():D}/incidents", new
            {
                subjectProfileId = Guid.NewGuid(), reason = "Contact not confirmed", reasonCategory = "Reported",
                reasonSourceDescription = "Phone call", participantProfileIds = Array.Empty<Guid>(),
            }),
            c => c.PutAsJsonAsync($"/api/incidents/{id}/participants", new { participantProfileIds = Array.Empty<Guid>() }),
            c => c.PostAsJsonAsync($"/api/incidents/{id}/coordinator", new { participantProfileId = Guid.NewGuid() }),
            c => c.PostAsJsonAsync($"/api/incidents/{id}/entries", new
            {
                kind = "Unknown", category = "Unknown", text = "Not known", sourceUnknown = true,
            }),
            c => c.PostAsJsonAsync($"/api/incidents/{id}/tasks", new { description = "Call", assigneeProfileId = Guid.NewGuid() }),
            c => c.PostAsJsonAsync($"/api/incidents/{id}/tasks/{task}/complete", new
            {
                resultText = "Done", category = "Reported", sourceDescription = "Call",
            }),
            c => c.PostAsJsonAsync($"/api/incidents/{id}/tasks/{task}/cancel", new { reason = "Not needed" }),
            c => c.PostAsJsonAsync($"/api/incidents/{id}/resolve", new
            {
                summary = "Ended", outcomeText = "Reached", outcomeCategory = "Reported",
                outcomeSourceDescription = "Call", contactRestoredTimeUnknown = true,
            }),
        ];
    }

    public sealed class ApiFactory : WebApplicationFactory<Program>
    {
        public StubIncidentService Service { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:FamilySafety", "Server=localhost;Database=Unused;Trusted_Connection=True;TrustServerCertificate=True");
            builder.UseSetting("DevelopmentIdentity:Enabled", "false");
            builder.UseSetting("DevelopmentFixtures:Enabled", "false");
            builder.UseSetting("DevelopmentInvitationOutbox:Enabled", "false");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IIncidentService>();
                services.AddSingleton<IIncidentService>(Service);
            });
        }
    }

    public sealed class StubIncidentService : IIncidentService
    {
        public bool Unavailable { get; set; }
        public bool Unauthenticated { get; set; }
        public bool Invalid { get; set; }
        public void Reset() { Unavailable = false; Unauthenticated = false; Invalid = false; }

        private static IncidentView Result() => new(IncidentId, Guid.NewGuid(), "Family", Guid.NewGuid(), "Subject",
            Guid.NewGuid(), "Creator", Guid.NewGuid(), "Creator", IncidentState.Open, DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(30), null, null, null, null, null, [], [], [], []);

        private Task<T> Check<T>(T value)
        {
            if (Unauthenticated) throw new CurrentAccountUnavailableException();
            if (Unavailable) throw new IncidentUnavailableException();
            if (Invalid) throw new IncidentValidationException(new Dictionary<string, string[]> { ["Reason"] = ["Required."] });
            return Task.FromResult(value);
        }

        public Task<IReadOnlyList<IncidentSummaryView>> GetVisibleAsync(CancellationToken cancellationToken) =>
            Check<IReadOnlyList<IncidentSummaryView>>([]);
        public Task<IncidentView> GetAsync(Guid incidentId, CancellationToken cancellationToken) => Check(Result());
        public Task<IncidentView> CreateAsync(Guid familyGroupId, CreateIncidentRequest request,
            CancellationToken cancellationToken) => Check(Result());
        public Task<IncidentView> ReplaceParticipantsAsync(Guid incidentId, ReplaceIncidentParticipantsRequest request,
            CancellationToken cancellationToken) => Check(Result());
        public Task<IncidentView> TransferCoordinatorAsync(Guid incidentId, TransferIncidentCoordinatorRequest request,
            CancellationToken cancellationToken) => Check(Result());
        public Task<IncidentView> AddEntryAsync(Guid incidentId, AddIncidentEntryRequest request,
            CancellationToken cancellationToken) => Check(Result());
        public Task<IncidentView> CreateTaskAsync(Guid incidentId, CreateIncidentTaskRequest request,
            CancellationToken cancellationToken) => Check(Result());
        public Task<IncidentView> CompleteTaskAsync(Guid incidentId, Guid taskId, CompleteIncidentTaskRequest request,
            CancellationToken cancellationToken) => Check(Result());
        public Task<IncidentView> CancelTaskAsync(Guid incidentId, Guid taskId, CancelIncidentTaskRequest request,
            CancellationToken cancellationToken) => Check(Result());
        public Task<IncidentView> ResolveAsync(Guid incidentId, ResolveIncidentRequest request,
            CancellationToken cancellationToken) => Check(Result());
    }
}
