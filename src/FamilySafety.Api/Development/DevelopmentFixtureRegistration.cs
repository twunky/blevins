using FamilySafety.Domain.Profiles;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Api.Development;

public static class DevelopmentFixtureRegistration
{
    public const string OwnerAccountId = "11111111-1111-1111-1111-111111111111";

    private static readonly Guid ProfileId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid FamilyGroupId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid MembershipId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid AdministratorAssignmentId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly Guid RecipientAccountId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    private static readonly Guid RecipientProfileId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    private static readonly Guid RecipientMembershipId = Guid.Parse("88888888-8888-8888-8888-888888888888");
    public static readonly Guid TripAudienceAccountId = Guid.Parse("99999999-9999-9999-9999-999999999999");
    private static readonly Guid TripAudienceProfileId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid TripAudienceMembershipId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    // Second-Family E2E scenario (WI-009-005, OD-E): A and B are active members; C, when seeded, is not.
    private static readonly Guid SecondFamilyGroupId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid SecondFamilyOwnerMembershipId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    private static readonly Guid SecondFamilyRecipientMembershipId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

    public static bool IsTripAudienceScenarioEnabled(IConfiguration configuration) =>
        configuration.GetValue<bool>("DevelopmentFixtures:TripAudienceScenario");

    public static bool IsSecondFamilyScenarioEnabled(IConfiguration configuration) =>
        configuration.GetValue<bool>("DevelopmentFixtures:SecondFamilyScenario");

    public static void EnsureTripAudienceScenarioAllowed(bool isDevelopment, IConfiguration configuration) =>
        EnsureIsolatedScenarioFixtureAllowed(
            isDevelopment, configuration, IsTripAudienceScenarioEnabled(configuration), "The Trip audience");

    public static void EnsureSecondFamilyScenarioAllowed(bool isDevelopment, IConfiguration configuration) =>
        EnsureIsolatedScenarioFixtureAllowed(
            isDevelopment, configuration, IsSecondFamilyScenarioEnabled(configuration), "The second Family");

    private static void EnsureIsolatedScenarioFixtureAllowed(
        bool isDevelopment, IConfiguration configuration, bool enabled, string fixture)
    {
        DevelopmentOnlyFeatureGuard.EnsureAllowed(isDevelopment, enabled, $"{fixture} scenario fixture");
        if (!enabled) return;

        var database = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(
            configuration.GetConnectionString("FamilySafety")).InitialCatalog;
        if (!configuration.GetValue<bool>("DevelopmentFixtures:Enabled")
            || !configuration.GetValue<bool>("DevelopmentFixtures:InvitationScenario")
            || !database.StartsWith("FamilySafety_Invitation_", StringComparison.Ordinal)
            || database == "FamilySafety")
        {
            throw new InvalidOperationException(
                $"{fixture} fixture requires the isolated FamilySafety_Invitation_* scenario.");
        }
    }

    public static IServiceCollection AddDevelopmentFixtures(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        var enabled = configuration.GetValue<bool>("DevelopmentFixtures:Enabled");
        var invitationScenario = configuration.GetValue<bool>("DevelopmentFixtures:InvitationScenario");
        EnsureTripAudienceScenarioAllowed(environment.IsDevelopment(), configuration);
        EnsureSecondFamilyScenarioAllowed(environment.IsDevelopment(), configuration);
        DevelopmentOnlyFeatureGuard.EnsureAllowed(environment.IsDevelopment(), invitationScenario,
            "The invitation scenario fixture");
        if (invitationScenario)
        {
            var database = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(
                configuration.GetConnectionString("FamilySafety")).InitialCatalog;
            if (!enabled || !database.StartsWith("FamilySafety_Invitation_", StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "The invitation scenario requires fixtures and an isolated FamilySafety_Invitation_ database.");
        }
        DevelopmentOnlyFeatureGuard.EnsureAllowed(
            environment.IsDevelopment(),
            enabled,
            "Development fixtures");

        if (enabled)
        {
            services.AddHostedService<DevelopmentFixtureInitializer>();
        }

        return services;
    }

    private sealed class DevelopmentFixtureInitializer(IServiceScopeFactory scopeFactory, IConfiguration configuration) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var tripAudienceScenario = IsTripAudienceScenarioEnabled(configuration);
            await using var scope = scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FamilySafetyDbContext>();
            await dbContext.Database.MigrateAsync(cancellationToken);

            var ownerAccountId = Guid.Parse(OwnerAccountId);
            if (!await dbContext.FamilyGroups.AnyAsync(group => group.Id == FamilyGroupId, cancellationToken))
            {
                dbContext.FamilyGroups.Add(new FamilyGroup(FamilyGroupId, "Development Family", 100));
            }
            else
            {
                var familyGroup = await dbContext.FamilyGroups.SingleAsync(
                    group => group.Id == FamilyGroupId, cancellationToken);
                familyGroup.Configure("Development Family", 100);
            }

            if (!await dbContext.Accounts.AnyAsync(account => account.Id == ownerAccountId, cancellationToken))
            {
                dbContext.Accounts.Add(new Account(ownerAccountId, "development-owner@example.invalid"));
            }

            if (!await dbContext.AdultProfiles.AnyAsync(profile => profile.Id == ProfileId, cancellationToken))
            {
                dbContext.AdultProfiles.Add(new AdultProfile(ProfileId, ownerAccountId, "Development Adult"));
            }

            if (!await dbContext.Accounts.AnyAsync(account => account.Id == RecipientAccountId, cancellationToken))
            {
                dbContext.Accounts.Add(new Account(RecipientAccountId, "development-recipient@example.invalid"));
            }

            if (!await dbContext.AdultProfiles.AnyAsync(profile => profile.Id == RecipientProfileId, cancellationToken))
            {
                dbContext.AdultProfiles.Add(new AdultProfile(
                    RecipientProfileId, RecipientAccountId, "Development Recipient"));
            }

            if (tripAudienceScenario)
            {
                if (!await dbContext.Accounts.AnyAsync(account => account.Id == TripAudienceAccountId, cancellationToken))
                    dbContext.Accounts.Add(new Account(TripAudienceAccountId, "development-trip-audience@example.invalid"));

                if (!await dbContext.AdultProfiles.AnyAsync(profile => profile.Id == TripAudienceProfileId, cancellationToken))
                    dbContext.AdultProfiles.Add(new AdultProfile(
                        TripAudienceProfileId, TripAudienceAccountId, "Development Audience Member"));
            }

            var ownerMembership = await ResolveFixtureMembershipAsync(
                dbContext, ownerAccountId, ProfileId, FamilyGroupId, MembershipId, cancellationToken);

            if (!configuration.GetValue<bool>("DevelopmentFixtures:InvitationScenario"))
            {
                await ResolveFixtureMembershipAsync(
                    dbContext, RecipientAccountId, RecipientProfileId, FamilyGroupId,
                    RecipientMembershipId, cancellationToken);
            }

            if (tripAudienceScenario)
            {
                await ResolveFixtureMembershipAsync(
                    dbContext, TripAudienceAccountId, TripAudienceProfileId, FamilyGroupId,
                    TripAudienceMembershipId, cancellationToken);
            }

            if (IsSecondFamilyScenarioEnabled(configuration))
            {
                // Test support only: no items and no administrator assignment are seeded here.
                if (!await dbContext.FamilyGroups.AnyAsync(group => group.Id == SecondFamilyGroupId, cancellationToken))
                    dbContext.FamilyGroups.Add(new FamilyGroup(SecondFamilyGroupId, "Development Second Family", 100));

                await ResolveFixtureMembershipAsync(
                    dbContext, ownerAccountId, ProfileId, SecondFamilyGroupId,
                    SecondFamilyOwnerMembershipId, cancellationToken);
                await ResolveFixtureMembershipAsync(
                    dbContext, RecipientAccountId, RecipientProfileId, SecondFamilyGroupId,
                    SecondFamilyRecipientMembershipId, cancellationToken);
            }

            var fixtureAdministratorAssignment = await dbContext.FamilyAdministratorAssignments
                .SingleOrDefaultAsync(
                    assignment => assignment.Id == AdministratorAssignmentId,
                    cancellationToken);
            if (fixtureAdministratorAssignment is not null
                && fixtureAdministratorAssignment.MembershipId != ownerMembership.Id)
                throw new InvalidOperationException(
                    "The Development Family administrator assignment is linked to a different membership than the resolved owner membership.");

            var ownerAdministratorAssignment = await dbContext.FamilyAdministratorAssignments
                .SingleOrDefaultAsync(
                    assignment => assignment.MembershipId == ownerMembership.Id,
                    cancellationToken);
            if (ownerAdministratorAssignment is null && fixtureAdministratorAssignment is null)
            {
                dbContext.FamilyAdministratorAssignments.Add(
                    new FamilyAdministratorAssignment(AdministratorAssignmentId, ownerMembership.Id));
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private static async Task<FamilyGroupMembership> ResolveFixtureMembershipAsync(
        FamilySafetyDbContext dbContext,
        Guid accountId,
        Guid adultProfileId,
        Guid familyGroupId,
        Guid fixtureMembershipId,
        CancellationToken cancellationToken)
    {
        var membership = await dbContext.FamilyGroupMemberships.SingleOrDefaultAsync(
            candidate => candidate.AccountId == accountId && candidate.FamilyGroupId == familyGroupId,
            cancellationToken);

        if (membership is not null)
        {
            if (membership.AdultProfileId != adultProfileId)
                throw new InvalidOperationException(
                    $"Development fixture membership for account '{accountId}' and Family '{familyGroupId}' " +
                    $"references unexpected AdultProfile '{membership.AdultProfileId}'; expected '{adultProfileId}'. " +
                    "The existing membership was not changed.");

            return membership;
        }

        membership = new FamilyGroupMembership(
            fixtureMembershipId, accountId, adultProfileId, familyGroupId, isActive: true);
        dbContext.FamilyGroupMemberships.Add(membership);
        return membership;
    }
}
