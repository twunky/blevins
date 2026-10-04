using FamilySafety.Application.Profiles;

namespace FamilySafety.Api.Development;

public static class CurrentAccountRegistration
{
    public const string AccountHeader = "X-Development-Account";

    public static readonly Guid OwnerAccountId = Guid.Parse(DevelopmentFixtureRegistration.OwnerAccountId);
    public static readonly Guid RecipientAccountId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    public static readonly Guid TripAudienceAccountId = DevelopmentFixtureRegistration.TripAudienceAccountId;

    public static IServiceCollection AddCurrentAccount(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        var enabled = configuration.GetValue<bool>("DevelopmentIdentity:Enabled");
        var allowSwitching = configuration.GetValue<bool>("DevelopmentIdentity:AllowSwitching");
        DevelopmentFixtureRegistration.EnsureTripAudienceScenarioAllowed(environment.IsDevelopment(), configuration);
        var tripAudienceScenario = DevelopmentFixtureRegistration.IsTripAudienceScenarioEnabled(configuration);
        DevelopmentOnlyFeatureGuard.EnsureAllowed(environment.IsDevelopment(), allowSwitching,
            "The development account switcher");
        if (allowSwitching && !enabled)
            throw new InvalidOperationException("Development account switching requires DevelopmentIdentity:Enabled.");
        DevelopmentOnlyFeatureGuard.EnsureAllowed(
            environment.IsDevelopment(),
            enabled,
            "The development current-account adapter");

        if (!enabled)
        {
            services.AddSingleton<ICurrentAccountAccessor, UnavailableCurrentAccountAccessor>();
            return services;
        }

        var accountId = configuration.GetValue<Guid?>("DevelopmentIdentity:AccountId");
        if (accountId is null || accountId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "DevelopmentIdentity:AccountId must be a non-empty GUID when the adapter is enabled.");
        }

        if (accountId == TripAudienceAccountId && !tripAudienceScenario)
            throw new InvalidOperationException("Adult C is available only in the opted-in Trip audience scenario.");

        if (allowSwitching)
        {
            if (!IsFixtureAccount(accountId.Value, tripAudienceScenario))
                throw new InvalidOperationException("The default account must be an allowlisted Development fixture identity.");
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentAccountAccessor>(provider =>
            {
                var request = provider.GetRequiredService<IHttpContextAccessor>().HttpContext?.Request;
                var selected = accountId.Value;
                if (request?.Headers.TryGetValue(AccountHeader, out var header) == true)
                {
                    if (header.Count != 1 || !Guid.TryParse(header[0], out selected)
                        || !IsFixtureAccount(selected, tripAudienceScenario))
                        return new UnavailableCurrentAccountAccessor();
                }
                return new DevelopmentCurrentAccountAccessor(selected);
            });
        }
        else
        {
            services.AddSingleton<ICurrentAccountAccessor>(new DevelopmentCurrentAccountAccessor(accountId.Value));
        }
        return services;
    }

    private static bool IsFixtureAccount(Guid id, bool tripAudienceScenario) =>
        id == OwnerAccountId || id == RecipientAccountId
        || (tripAudienceScenario && id == TripAudienceAccountId);

    public static IEndpointRouteBuilder MapDevelopmentIdentities(
        this IEndpointRouteBuilder endpoints, IHostEnvironment environment, IConfiguration configuration)
    {
        var enabled = configuration.GetValue<bool>("DevelopmentIdentity:AllowSwitching");
        DevelopmentFixtureRegistration.EnsureTripAudienceScenarioAllowed(environment.IsDevelopment(), configuration);
        DevelopmentOnlyFeatureGuard.EnsureAllowed(environment.IsDevelopment(), enabled, "Development identities endpoint");
        if (enabled)
            endpoints.MapGet("/api/development/identities", () =>
            {
                object[] identities =
                {
                    new { accountId = OwnerAccountId, label = "Adult A — Family administrator" },
                    new { accountId = RecipientAccountId, label = "Adult B — invited adult" }
                };
                if (DevelopmentFixtureRegistration.IsTripAudienceScenarioEnabled(configuration))
                    identities =
                    [
                        .. identities,
                        new { accountId = TripAudienceAccountId, label = "Adult C — Trip audience member" }
                    ];
                return Results.Ok(new
                {
                    defaultAccountId = configuration.GetValue<Guid>("DevelopmentIdentity:AccountId"),
                    identities
                });
            });
        return endpoints;
    }

    private sealed class DevelopmentCurrentAccountAccessor(Guid accountId) : ICurrentAccountAccessor
    {
        public CurrentAccount Current { get; } = new(accountId);
    }

    private sealed class UnavailableCurrentAccountAccessor : ICurrentAccountAccessor
    {
        public CurrentAccount? Current => null;
    }
}
