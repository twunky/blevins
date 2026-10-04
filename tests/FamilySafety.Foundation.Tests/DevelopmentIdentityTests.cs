using System.Net;
using FamilySafety.Api.Development;
using FamilySafety.Application.Profiles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace FamilySafety.Foundation.Tests;

public sealed class DevelopmentIdentityTests
{
    [Theory]
    [InlineData(null, "11111111-1111-1111-1111-111111111111")]
    [InlineData("66666666-6666-6666-6666-666666666666", "66666666-6666-6666-6666-666666666666")]
    [InlineData("99999999-9999-9999-9999-999999999999", null)]
    [InlineData("not-an-account", null)]
    public void RequestSelectionUsesOnlyAllowlistedAccounts(string? header, string? expected)
    {
        var services = new ServiceCollection();
        services.AddCurrentAccount(new TestEnvironment("Development"), Configuration());
        using var provider = services.BuildServiceProvider();
        var context = new DefaultHttpContext();
        if (header is not null) context.Request.Headers[CurrentAccountRegistration.AccountHeader] = header;
        provider.GetRequiredService<IHttpContextAccessor>().HttpContext = context;
        using var scope = provider.CreateScope();
        Assert.Equal(expected, scope.ServiceProvider.GetRequiredService<ICurrentAccountAccessor>().Current?.AccountId.ToString());
    }

    [Fact]
    public void SeparateRequestScopesDoNotShareSelectedIdentity()
    {
        var services = new ServiceCollection();
        services.AddCurrentAccount(new TestEnvironment("Development"), Configuration());
        using var provider = services.BuildServiceProvider();
        var accessor = provider.GetRequiredService<IHttpContextAccessor>();
        accessor.HttpContext = new DefaultHttpContext();
        using var first = provider.CreateScope();
        var adultA = first.ServiceProvider.GetRequiredService<ICurrentAccountAccessor>();
        accessor.HttpContext = new DefaultHttpContext();
        accessor.HttpContext.Request.Headers[CurrentAccountRegistration.AccountHeader] = CurrentAccountRegistration.RecipientAccountId.ToString();
        using var second = provider.CreateScope();
        Assert.Equal(CurrentAccountRegistration.RecipientAccountId, second.ServiceProvider.GetRequiredService<ICurrentAccountAccessor>().Current!.AccountId);
        Assert.Equal(CurrentAccountRegistration.OwnerAccountId, adultA.Current!.AccountId);
    }

    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    public void SwitchingAndScenarioConfigurationFailOutsideDevelopment(string environment)
    {
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddCurrentAccount(
            new TestEnvironment(environment), Configuration(identityEnabled: false)));
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddDevelopmentFixtures(
            new TestEnvironment(environment), Configuration(scenario: true)));
    }

    [Fact]
    public void ScenarioCannotSeedTheOrdinaryDevelopmentDatabase()
    {
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddDevelopmentFixtures(
            new TestEnvironment("Development"), Configuration(scenario: true)));
    }

    [Fact]
    public void TripAudienceIdentityRequiresTheOptedInInvitationScenario()
    {
        var services = new ServiceCollection();
        services.AddCurrentAccount(new TestEnvironment("Development"), Configuration(
            scenario: true, tripAudienceScenario: true,
            database: "FamilySafety_Invitation_TripAudience"));
        using var provider = services.BuildServiceProvider();
        var context = new DefaultHttpContext();
        context.Request.Headers[CurrentAccountRegistration.AccountHeader] =
            CurrentAccountRegistration.TripAudienceAccountId.ToString();
        provider.GetRequiredService<IHttpContextAccessor>().HttpContext = context;
        using var scope = provider.CreateScope();
        Assert.Equal(CurrentAccountRegistration.TripAudienceAccountId,
            scope.ServiceProvider.GetRequiredService<ICurrentAccountAccessor>().Current!.AccountId);
    }

    [Fact]
    public void TripAudienceIdentityIsRejectedWithoutTheOptInOrDedicatedScenario()
    {
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddCurrentAccount(
            new TestEnvironment("Development"), Configuration(
                scenario: true, tripAudienceScenario: true, database: "FamilySafety")));
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddCurrentAccount(
            new TestEnvironment("Production"), Configuration(
                scenario: true, tripAudienceScenario: true,
                database: "FamilySafety_Invitation_TripAudience")));
    }

    [Fact]
    public async Task ProductionHasNoDevelopmentEndpointsAndIgnoresIdentityHeader()
    {
        await using var factory = new ProductionFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(CurrentAccountRegistration.AccountHeader, CurrentAccountRegistration.OwnerAccountId.ToString());
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/development/identities")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/development/invitation-outbox")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/profile")).StatusCode);
    }

    private static IConfiguration Configuration(bool identityEnabled = true, bool scenario = false,
        bool tripAudienceScenario = false, string database = "FamilySafety") =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DevelopmentIdentity:Enabled"] = identityEnabled.ToString(),
            ["DevelopmentIdentity:AllowSwitching"] = "true",
            ["DevelopmentIdentity:AccountId"] = DevelopmentFixtureRegistration.OwnerAccountId,
            ["DevelopmentFixtures:Enabled"] = "true",
            ["DevelopmentFixtures:InvitationScenario"] = scenario.ToString(),
            ["DevelopmentFixtures:TripAudienceScenario"] = tripAudienceScenario.ToString(),
            ["ConnectionStrings:FamilySafety"] = $"Server=localhost;Database={database};Trusted_Connection=True;TrustServerCertificate=True"
        }).Build();

    private sealed class TestEnvironment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = ".";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class ProductionFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("ConnectionStrings:FamilySafety", "Server=localhost;Database=Unused;Trusted_Connection=True;TrustServerCertificate=True");
            foreach (var key in new[] { "DevelopmentIdentity:Enabled", "DevelopmentIdentity:AllowSwitching", "DevelopmentFixtures:Enabled", "DevelopmentFixtures:InvitationScenario", "DevelopmentInvitationOutbox:Enabled" })
                builder.UseSetting(key, "false");
        }
    }
}
