using System.Net;
using System.Net.Http.Json;
using FamilySafety.Application.Invitations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FamilySafety.Foundation.Tests;

public sealed class MembershipInvitationApiTests : IClassFixture<MembershipInvitationApiTests.InvitationApiFactory>
{
    private readonly HttpClient client;

    public MembershipInvitationApiTests(InvitationApiFactory factory) => client = factory.CreateClient();

    [Fact]
    public async Task SuccessfulAcceptReturnsFamilyIdForMembershipReconciliation()
    {
        var response = await client.PostAsJsonAsync("/api/membership-invitations/accept", new { token = "opaque" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var accepted = await response.Content.ReadFromJsonAsync<MembershipInvitationAccepted>();
        Assert.Equal(Guid.Parse("33333333-3333-3333-3333-333333333333"), accepted!.FamilyGroupId);
    }

    [Fact]
    public async Task AdministeredGroupsReturnOnlyTheServiceContract()
    {
        var response = await client.GetAsync("/api/family-groups/administered");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("API Family", body, StringComparison.Ordinal);
        Assert.DoesNotContain("capacity", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PreviewReturnsOnlyApprovedConfirmationFields()
    {
        var response = await client.PostAsJsonAsync(
            "/api/membership-invitations/preview", new { token = "opaque" });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("familyGroupName", body, StringComparison.Ordinal);
        Assert.Contains("inviterDisplayName", body, StringComparison.Ordinal);
        Assert.DoesNotContain("recipient", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnavailableInvitationUsesUniformNotFound()
    {
        var response = await client.PostAsJsonAsync(
            "/api/membership-invitations/accept", new { token = "unavailable" });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public sealed class InvitationApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:FamilySafety", "Server=localhost;Database=Unused;Trusted_Connection=True;TrustServerCertificate=True");
            builder.UseSetting("DevelopmentIdentity:Enabled", "false");
            builder.UseSetting("DevelopmentFixtures:Enabled", "false");
            builder.UseSetting("DevelopmentInvitationOutbox:Enabled", "false");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IMembershipInvitationService>();
                services.AddSingleton<IMembershipInvitationService, StubInvitationService>();
            });
        }
    }

    private sealed class StubInvitationService : IMembershipInvitationService
    {
        private static readonly Guid GroupId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        public Task<IReadOnlyList<AdministeredFamilyGroupView>> GetAdministeredGroupsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<AdministeredFamilyGroupView>>([new(GroupId, "API Family")]);
        public Task IssueAsync(IssueMembershipInvitationRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task RevokeAsync(RevokeMembershipInvitationRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<MembershipInvitationPreview> PreviewAsync(InvitationTokenRequest request, CancellationToken cancellationToken) =>
            Task.FromResult(new MembershipInvitationPreview("API Family", "API Inviter"));
        public Task<MembershipInvitationAccepted> AcceptAsync(InvitationTokenRequest request, CancellationToken cancellationToken) =>
            request.Token == "unavailable" ? Task.FromException<MembershipInvitationAccepted>(new MembershipInvitationUnavailableException()) : Task.FromResult(new MembershipInvitationAccepted(GroupId));
        public Task DeclineAsync(InvitationTokenRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
