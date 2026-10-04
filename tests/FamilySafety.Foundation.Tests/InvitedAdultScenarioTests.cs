using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FamilySafety.Api.Development;
using FamilySafety.Application.Invitations;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FamilySafety.Foundation.Tests;

public sealed class InvitedAdultScenarioTests
{
    private static readonly string[] PreviewFields = ["familyGroupName", "inviterDisplayName"];
    [Fact]
    public async Task RealDevelopmentFixtureAndOutboxSupportNonmemberAcceptanceAndPrivacy()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        await using var factory = new ScenarioFactory();
        using var client = factory.CreateClient();
        try
        {
            var identities = await client.GetAsync("/api/development/identities");
            identities.EnsureSuccessStatusCode();
            using (var identityDocument = JsonDocument.Parse(await identities.Content.ReadAsStringAsync()))
            {
                var identityLabels = identityDocument.RootElement.GetProperty("identities")
                    .EnumerateArray().Select(identity => identity.GetProperty("label").GetString()).ToArray();
                Assert.Equal(2, identityLabels.Length);
                Assert.DoesNotContain(identityLabels, label => label!.StartsWith("Adult C", StringComparison.Ordinal));
            }
            Select(client, CurrentAccountRegistration.RecipientAccountId);
            Assert.Equal("[]", await client.GetStringAsync("/api/family-groups/mine"));
            Assert.Equal("[]", await client.GetStringAsync("/api/family-groups/administered"));
            var groupId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/family-groups/{groupId}/members")).StatusCode);
            Select(client, CurrentAccountRegistration.OwnerAccountId);
            var originalMembers = await client.GetFromJsonAsync<JsonElement[]>($"/api/family-groups/{groupId}/members");
            Assert.Equal("Development Adult", Assert.Single(originalMembers!).GetProperty("displayName").GetString());
            var issued = await client.PostAsJsonAsync("/api/membership-invitations", new { familyGroupId = groupId, recipientEmail = "invitation-only@example.invalid" });
            Assert.Equal(HttpStatusCode.Accepted, issued.StatusCode);
            var messages = await client.GetFromJsonAsync<DevelopmentInvitationMessage[]>("/api/development/invitation-outbox");
            var token = Uri.UnescapeDataString(Assert.Single(messages!).InvitationUrl.Split("#invite=")[1]);
            Select(client, CurrentAccountRegistration.RecipientAccountId);
            var preview = await client.PostAsJsonAsync("/api/membership-invitations/preview", new { token });
            preview.EnsureSuccessStatusCode();
            var fields = (await preview.Content.ReadFromJsonAsync<JsonElement>()).EnumerateObject().Select(p => p.Name).ToArray();
            Assert.Equal(PreviewFields, fields);
            Assert.Equal("[]", await client.GetStringAsync("/api/family-groups/mine"));
            var accepted = await client.PostAsJsonAsync("/api/membership-invitations/accept", new { token });
            accepted.EnsureSuccessStatusCode();
            Assert.Equal(groupId, (await accepted.Content.ReadFromJsonAsync<MembershipInvitationAccepted>())!.FamilyGroupId);
            Assert.Contains(groupId.ToString(), await client.GetStringAsync("/api/family-groups/mine"));
            Assert.Contains("Development Recipient", await client.GetStringAsync($"/api/family-groups/{groupId}/members"));
            Assert.Equal("[]", await client.GetStringAsync($"/api/family-groups/{groupId}/relationships"));
            Assert.Equal("[]", await client.GetStringAsync("/api/family-groups/administered"));
            var profile = await client.GetStringAsync("/api/profile");
            Assert.DoesNotContain("invitation-only", profile);
            Assert.Contains("OnlyMe", profile);
            Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync("/api/membership-invitations/accept", new { token })).StatusCode);
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FamilySafetyDbContext>();
            Assert.Equal(1, await db.FamilyGroupMemberships.CountAsync(m => m.AccountId == CurrentAccountRegistration.RecipientAccountId));
        }
        finally
        {
            // This factory owns a unique test database; ordinary Development data is never reset.
            using var scope = factory.Services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<FamilySafetyDbContext>().Database.EnsureDeletedAsync();
        }
    }

    private static void Select(HttpClient client, Guid id)
    {
        client.DefaultRequestHeaders.Remove(CurrentAccountRegistration.AccountHeader);
        client.DefaultRequestHeaders.Add(CurrentAccountRegistration.AccountHeader, id.ToString());
    }

    private sealed class ScenarioFactory : WebApplicationFactory<Program>
    {
        private readonly string database = "FamilySafety_Invitation_Test_" + Guid.NewGuid().ToString("N");
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("ConnectionStrings:FamilySafety", $"Server=localhost;Database={database};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");
            foreach (var key in new[] { "DevelopmentIdentity:Enabled", "DevelopmentIdentity:AllowSwitching", "DevelopmentFixtures:Enabled", "DevelopmentFixtures:InvitationScenario", "DevelopmentInvitationOutbox:Enabled" })
                builder.UseSetting(key, "true");
            builder.UseSetting("DevelopmentIdentity:AccountId", DevelopmentFixtureRegistration.OwnerAccountId);
            builder.UseSetting("DevelopmentInvitationOutbox:ClientBaseUrl", "http://localhost:4200");
        }
    }
}
