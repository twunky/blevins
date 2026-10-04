using System.Net;
using System.Net.Http.Json;
using FamilySafety.Application.Profiles;
using FamilySafety.Domain.Profiles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FamilySafety.Foundation.Tests;

public sealed class AdultProfileApiTests : IClassFixture<AdultProfileApiTests.ProfileApiFactory>
{
    private readonly HttpClient client;

    public AdultProfileApiTests(ProfileApiFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task CurrentProfileEndpointReturnsOwnerContract()
    {
        var response = await client.GetAsync("/api/profile", CancellationToken.None);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(CancellationToken.None);
        Assert.Contains("\"displayName\":\"API Adult\"", content, StringComparison.Ordinal);
        Assert.Contains("\"contactEmailVisibility\":\"OnlyMe\"", content, StringComparison.Ordinal);
        Assert.DoesNotContain("signInIdentifier", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateEndpointAcceptsExplicitVisibility()
    {
        var response = await client.PutAsJsonAsync(
            "/api/profile",
            new
            {
                displayName = "API Adult",
                contactEmail = "shared@example.com",
                contactEmailVisibility = "ActiveFamilyGroup",
                contactPhone = (string?)null,
                contactPhoneVisibility = "OnlyMe",
                birthdayMonth = (int?)null,
                birthdayDay = (int?)null,
                birthdayVisibility = "OnlyMe",
            },
            CancellationToken.None);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task SharedEndpointUsesNotFoundForUnavailableProfile()
    {
        var response = await client.GetAsync(
            $"/api/family-members/{Guid.NewGuid():D}/profile",
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public sealed class ProfileApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting(
                "ConnectionStrings:FamilySafety",
                "Server=localhost;Database=FamilySafetyTests;Trusted_Connection=True;TrustServerCertificate=True");
            builder.UseSetting("DevelopmentIdentity:Enabled", "false");
            builder.UseSetting("DevelopmentFixtures:Enabled", "false");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IAdultProfileService>();
                services.AddSingleton<IAdultProfileService, StubAdultProfileService>();
            });
        }
    }

    private sealed class StubAdultProfileService : IAdultProfileService
    {
        private static readonly Guid ProfileId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        public Task<AdultProfileOwnerView> GetCurrentAsync(CancellationToken cancellationToken) =>
            Task.FromResult(CreateOwnerView());

        public Task<AdultProfileOwnerView> UpdateCurrentAsync(
            UpdateAdultProfileRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(CreateOwnerView() with
            {
                ContactEmail = request.ContactEmail,
                ContactEmailVisibility = request.ContactEmailVisibility,
            });

        public Task<AdultProfileSharedView?> GetSharedAsync(
            Guid profileId,
            CancellationToken cancellationToken) =>
            Task.FromResult<AdultProfileSharedView?>(null);

        private static AdultProfileOwnerView CreateOwnerView() =>
            new(
                ProfileId,
                "API Adult",
                null,
                ProfileFieldVisibility.OnlyMe,
                null,
                ProfileFieldVisibility.OnlyMe,
                null,
                null,
                ProfileFieldVisibility.OnlyMe);
    }
}
