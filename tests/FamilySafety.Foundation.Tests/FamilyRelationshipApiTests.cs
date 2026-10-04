using System.Net;
using System.Net.Http.Json;
using FamilySafety.Application.Profiles;
using FamilySafety.Application.Relationships;
using FamilySafety.Domain.Relationships;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FamilySafety.Foundation.Tests;

public sealed class FamilyRelationshipApiTests : IClassFixture<FamilyRelationshipApiTests.RelationshipApiFactory>
{
    private readonly HttpClient client;
    private readonly StubRelationshipService service;

    public FamilyRelationshipApiTests(RelationshipApiFactory factory)
    {
        client = factory.CreateClient();
        service = factory.Service;
        service.Reset();
    }

    [Fact]
    public async Task RelationshipResponseUsesOnlyApprovedRelationshipFields()
    {
        service.Relationships = [new(
            Guid.NewGuid(), Guid.NewGuid(), "Alex Adult", FamilyRelationshipLabel.Parent,
            Guid.NewGuid(), "Sam Adult", FamilyRelationshipLabel.AdultChild,
            FamilyRelationshipStatus.Accepted, false, false)];

        var response = await client.GetAsync($"/api/family-groups/{StubRelationshipService.GroupId:D}/relationships");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains("proposerLabel\":\"Parent\"", body, StringComparison.Ordinal);
        Assert.Contains("recipientLabel\":\"AdultChild\"", body, StringComparison.Ordinal);
        Assert.DoesNotContain("accountId", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("contactEmail", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("permission", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProposalEndpointPassesOnlyRecipientAndLabelToService()
    {
        var recipientId = Guid.NewGuid();
        var response = await client.PostAsJsonAsync(
            $"/api/family-groups/{StubRelationshipService.GroupId:D}/relationship-proposals",
            new { recipientProfileId = recipientId, proposerLabel = "Parent" });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal(StubRelationshipService.GroupId, service.LastGroupId);
        Assert.Equal(recipientId, service.LastRequest?.RecipientProfileId);
        Assert.Equal(FamilyRelationshipLabel.Parent, service.LastRequest?.ProposerLabel);
    }

    [Fact]
    public async Task UnavailableFamilyGroupReturnsNotFound()
    {
        service.Unavailable = true;
        var response = await client.GetAsync($"/api/family-groups/{Guid.NewGuid():D}/relationships");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public sealed class RelationshipApiFactory : WebApplicationFactory<Program>
    {
        public StubRelationshipService Service { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:FamilySafety", "Server=localhost;Database=Unused;Trusted_Connection=True;TrustServerCertificate=True");
            builder.UseSetting("DevelopmentIdentity:Enabled", "false");
            builder.UseSetting("DevelopmentFixtures:Enabled", "false");
            builder.UseSetting("DevelopmentInvitationOutbox:Enabled", "false");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IFamilyRelationshipService>();
                services.AddSingleton<IFamilyRelationshipService>(Service);
            });
        }
    }

    public sealed class StubRelationshipService : IFamilyRelationshipService
    {
        public static readonly Guid GroupId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        public IReadOnlyList<FamilyRelationshipView> Relationships { get; set; } = [];
        public bool Unavailable { get; set; }
        public Guid LastGroupId { get; private set; }
        public ProposeFamilyRelationshipRequest? LastRequest { get; private set; }

        public void Reset()
        {
            Relationships = [];
            Unavailable = false;
            LastGroupId = Guid.Empty;
            LastRequest = null;
        }

        public Task<IReadOnlyList<FamilyRelationshipView>> GetForGroupAsync(
            Guid familyGroupId,
            CancellationToken cancellationToken)
        {
            if (Unavailable || familyGroupId != GroupId)
            {
                throw new FamilyRelationshipUnavailableException();
            }

            return Task.FromResult(Relationships);
        }

        public Task ProposeAsync(
            Guid familyGroupId,
            ProposeFamilyRelationshipRequest request,
            CancellationToken cancellationToken)
        {
            LastGroupId = familyGroupId;
            LastRequest = request;
            return Task.CompletedTask;
        }

        public Task RespondAsync(
            Guid relationshipId,
            FamilyRelationshipAction action,
            CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
