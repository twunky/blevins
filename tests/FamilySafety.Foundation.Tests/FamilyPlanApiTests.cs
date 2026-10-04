using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FamilySafety.Application.Plans;
using FamilySafety.Application.Profiles;
using FamilySafety.Domain.Plans;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FamilySafety.Foundation.Tests;

public sealed class FamilyPlanApiTests : IClassFixture<FamilyPlanApiTests.PlanApiFactory>
{
    private static readonly string[] RecipientIds = ["77777777-7777-7777-7777-777777777777"];
    private readonly HttpClient client;
    private readonly StubFamilyPlanService service;

    public FamilyPlanApiTests(PlanApiFactory factory)
    {
        client = factory.CreateClient();
        service = factory.Service;
        service.Reset();
    }

    [Fact]
    public async Task CreateEndpointPassesSelectedAudienceToService()
    {
        var planId = Guid.NewGuid();
        service.Created = View(planId, PlanAudienceType.SelectedPeople);

        var response = await client.PostAsJsonAsync(
            $"/api/family-groups/{StubFamilyPlanService.GroupId:D}/plans",
            new
            {
                activityOrDestination = "Visit the lake",
                date = "2026-10-03",
                time = "10:00",
                expectedReturnDate = (string?)null,
                expectedReturnTime = (string?)null,
                expectedCheckInDate = (string?)null,
                expectedCheckInTime = (string?)null,
                audienceType = "SelectedPeople",
                audienceProfileIds = RecipientIds,
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(StubFamilyPlanService.GroupId, service.LastGroupId);
        Assert.Equal(PlanAudienceType.SelectedPeople, service.LastRequest?.AudienceType);
        Assert.Equal(Guid.Parse("77777777-7777-7777-7777-777777777777"),
            Assert.Single(service.LastRequest!.AudienceProfileIds));
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"audienceType\":\"SelectedPeople\"", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateEndpointReturnsValidationProblemForInvalidAudience()
    {
        service.ValidationFailure = true;
        var response = await client.PostAsJsonAsync(
            $"/api/family-groups/{StubFamilyPlanService.GroupId:D}/plans",
            new
            {
                activityOrDestination = "Visit the lake",
                date = "2026-10-03",
                time = "10:00",
                audienceType = "SelectedPeople",
                audienceProfileIds = Array.Empty<Guid>(),
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("audienceProfileIds", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ItemEndpointReturnsOnlyTheListEntryPropertiesForAnAuthorizedPlan()
    {
        var planId = Guid.NewGuid();
        service.Item = View(planId, PlanAudienceType.SelectedPeople) with { Notes = "Bring water." };

        var response = await client.GetAsync($"/api/plans/{planId:D}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(planId, service.LastPlanId);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(
            ["activityOrDestination", "audienceType", "creatorDisplayName", "creatorProfileId", "date",
                "expectedCheckInDate", "expectedCheckInTime", "expectedReturnDate", "expectedReturnTime",
                "familyGroupId", "notes", "planId", "time"],
            json.RootElement.EnumerateObject().Select(property => property.Name).Order(StringComparer.Ordinal));
        Assert.Equal(StubFamilyPlanService.GroupId, json.RootElement.GetProperty("familyGroupId").GetGuid());
        Assert.Equal("Bring water.", json.RootElement.GetProperty("notes").GetString());
    }

    [Fact]
    public async Task ItemEndpointGivesMissingAndUnauthorizedPlansTheSameEmptyNotFound()
    {
        service.Item = View(Guid.NewGuid(), PlanAudienceType.FamilyGroup);

        var missing = await client.GetAsync($"/api/plans/{Guid.NewGuid():D}");
        var unauthorized = await client.GetAsync($"/api/plans/{Guid.NewGuid():D}");
        var notAGuid = await client.GetAsync("/api/plans/not-an-id");

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

        var response = await client.GetAsync($"/api/plans/{Guid.NewGuid():D}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task FamilyScopedListRouteStillAnswers()
    {
        var response = await client.GetAsync(
            $"/api/family-groups/{StubFamilyPlanService.GroupId:D}/plans");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("[]", await response.Content.ReadAsStringAsync());
    }

    private static FamilyPlanView View(Guid planId, PlanAudienceType audienceType) => new(
        planId,
        StubFamilyPlanService.GroupId,
        "Visit the lake",
        new DateOnly(2026, 10, 3),
        new TimeOnly(10, 0),
        null,
        null,
        null,
        null,
        audienceType,
        Guid.NewGuid(),
        "Organizer");

    public sealed class PlanApiFactory : WebApplicationFactory<Program>
    {
        public StubFamilyPlanService Service { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:FamilySafety", "Server=localhost;Database=Unused;Trusted_Connection=True;TrustServerCertificate=True");
            builder.UseSetting("DevelopmentIdentity:Enabled", "false");
            builder.UseSetting("DevelopmentFixtures:Enabled", "false");
            builder.UseSetting("DevelopmentInvitationOutbox:Enabled", "false");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IFamilyPlanService>();
                services.AddSingleton<IFamilyPlanService>(Service);
            });
        }
    }

    public sealed class StubFamilyPlanService : IFamilyPlanService
    {
        public static readonly Guid GroupId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        public Guid LastGroupId { get; private set; }
        public CreateFamilyPlanRequest? LastRequest { get; private set; }
        public FamilyPlanView? Created { get; set; }
        public bool ValidationFailure { get; set; }
        public Guid LastPlanId { get; private set; }
        public FamilyPlanView? Item { get; set; }
        public bool NoCurrentAccount { get; set; }

        public void Reset()
        {
            LastGroupId = Guid.Empty;
            LastRequest = null;
            Created = null;
            ValidationFailure = false;
            LastPlanId = Guid.Empty;
            Item = null;
            NoCurrentAccount = false;
        }

        public Task<IReadOnlyList<FamilyPlanView>> GetVisibleAsync(
            Guid familyGroupId,
            CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<FamilyPlanView>>([]);

        // Mirrors the real service: only the one readable Plan is returned; any other
        // ID, missing or unauthorized alike, is unavailable.
        public Task<FamilyPlanView> GetAsync(Guid planId, CancellationToken cancellationToken)
        {
            LastPlanId = planId;
            if (NoCurrentAccount) throw new CurrentAccountUnavailableException();
            if (Item is null || Item.PlanId != planId) throw new FamilyPlanUnavailableException();
            return Task.FromResult(Item);
        }

        public Task<FamilyPlanView> CreateAsync(
            Guid familyGroupId,
            CreateFamilyPlanRequest request,
            CancellationToken cancellationToken)
        {
            LastGroupId = familyGroupId;
            LastRequest = request;
            if (ValidationFailure)
            {
                throw new FamilyPlanValidationException(new Dictionary<string, string[]>
                {
                    [nameof(CreateFamilyPlanRequest.AudienceProfileIds)] = ["Choose plan recipients."],
                });
            }

            return Task.FromResult(Created!);
        }
    }
}
