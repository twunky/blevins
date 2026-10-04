using System.Net;
using FamilySafety.Application.Directory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FamilySafety.Foundation.Tests;

public sealed class FamilyDirectoryApiTests : IClassFixture<FamilyDirectoryApiTests.DirectoryApiFactory>
{
    private readonly HttpClient client;

    public FamilyDirectoryApiTests(DirectoryApiFactory factory) => client = factory.CreateClient();

    [Fact]
    public async Task DirectoryResponseContainsOnlyProfileIdentity()
    {
        var response = await client.GetAsync($"/api/family-groups/{StubDirectoryService.GroupId:D}/members?search=adult");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains("profileId", body, StringComparison.Ordinal);
        Assert.Contains("displayName", body, StringComparison.Ordinal);
        Assert.DoesNotContain("contact", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("signIn", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("capacity", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnavailableGroupUsesNotFound()
    {
        var response = await client.GetAsync($"/api/family-groups/{Guid.NewGuid():D}/members");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public sealed class DirectoryApiFactory : WebApplicationFactory<Program>
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
                services.RemoveAll<IFamilyDirectoryService>();
                services.AddSingleton<IFamilyDirectoryService, StubDirectoryService>();
            });
        }
    }

    private sealed class StubDirectoryService : IFamilyDirectoryService
    {
        public static readonly Guid GroupId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        private static readonly Guid ProfileId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        public Task<IReadOnlyList<ActiveFamilyGroupView>> GetActiveGroupsAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<ActiveFamilyGroupView>>([new(GroupId, "API Family")]);

        public Task<IReadOnlyList<FamilyDirectoryEntry>> GetMembersAsync(
            Guid familyGroupId,
            string? search,
            CancellationToken cancellationToken) =>
            familyGroupId == GroupId
                ? Task.FromResult<IReadOnlyList<FamilyDirectoryEntry>>([new(ProfileId, "API Adult")])
                : Task.FromException<IReadOnlyList<FamilyDirectoryEntry>>(new FamilyDirectoryUnavailableException());
    }
}
