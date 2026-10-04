using System.Net;
using FamilySafety.Api.Development;
using FamilySafety.Domain.Profiles;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FamilySafety.Foundation.Tests;

public sealed class DevelopmentFixtureRegistrationTests
{
    private static readonly Guid OwnerProfileId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid FamilyGroupId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid FixtureOwnerMembershipId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid AdministratorAssignmentId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly Guid RecipientAccountId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    private static readonly Guid RecipientProfileId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    private static readonly Guid FixtureRecipientMembershipId = Guid.Parse("88888888-8888-8888-8888-888888888888");

    [Fact]
    public async Task InitialFixtureStartupAndRepeatedStartupAreIdempotent()
    {
        if (!SqlTestsEnabled()) return;

        await WithTestDatabaseAsync(async connectionString =>
        {
            using (var firstRun = new DevelopmentFactory(connectionString))
            using (var client = firstRun.CreateClient())
            {
                Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);
                Assert.Contains("Development Adult", await client.GetStringAsync("/api/profile"), StringComparison.Ordinal);
            }

            var firstState = await ReadFixtureStateAsync(connectionString);
            Assert.Equal(2, firstState.MembershipsCount);
            Assert.Equal(FixtureOwnerMembershipId, firstState.OwnerMembershipId);
            Assert.Equal(FixtureRecipientMembershipId, firstState.RecipientMembershipId);
            Assert.Equal(FixtureOwnerMembershipId, firstState.AdministratorMembershipId);

            using (var secondRun = new DevelopmentFactory(connectionString))
            using (var client = secondRun.CreateClient())
            {
                Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);
            }

            var secondState = await ReadFixtureStateAsync(connectionString);
            Assert.Equal(firstState, secondState);
        });
    }

    [Fact]
    public async Task ExistingMembershipsWithDifferentIdsAreReusedWithoutMutation()
    {
        if (!SqlTestsEnabled()) return;

        var ownerMembershipId = Guid.NewGuid();
        var recipientMembershipId = Guid.NewGuid();
        await WithTestDatabaseAsync(async connectionString =>
        {
            await SeedFixtureIdentityAsync(
                connectionString,
                ownerMembershipId,
                recipientMembershipId,
                recipientIsActive: false);

            using (var factory = new DevelopmentFactory(connectionString))
            using (var client = factory.CreateClient())
                Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);

            var state = await ReadFixtureStateAsync(connectionString);
            Assert.Equal(2, state.MembershipsCount);
            Assert.Equal(ownerMembershipId, state.OwnerMembershipId);
            Assert.Equal(recipientMembershipId, state.RecipientMembershipId);
            Assert.Equal(ownerMembershipId, state.AdministratorMembershipId);
            Assert.True(state.OwnerIsActive);
            Assert.False(state.RecipientIsActive);
            Assert.Equal(OwnerProfileId, state.OwnerProfileId);
            Assert.Equal(RecipientProfileId, state.RecipientProfileId);
        });
    }

    [Fact]
    public async Task MembershipWithUnexpectedProfileFailsClearlyAndIsNotOverwritten()
    {
        if (!SqlTestsEnabled()) return;

        var unexpectedProfileId = Guid.NewGuid();
        await WithTestDatabaseAsync(async connectionString =>
        {
            await SeedFixtureIdentityAsync(
                connectionString,
                ownerMembershipId: null,
                recipientMembershipId: Guid.NewGuid(),
                recipientIsActive: false,
                recipientProfileId: unexpectedProfileId);

            using var factory = new DevelopmentFactory(connectionString);
            var exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                using var client = factory.CreateClient();
                await client.GetAsync("/health");
            });
            Assert.Contains("references unexpected AdultProfile", exception.ToString(), StringComparison.Ordinal);

            await using var db = CreateContext(connectionString);
            var membership = await db.FamilyGroupMemberships.SingleAsync(
                row => row.AccountId == RecipientAccountId && row.FamilyGroupId == FamilyGroupId);
            Assert.Equal(unexpectedProfileId, membership.AdultProfileId);
            Assert.False(membership.IsActive);
            Assert.Empty(await db.FamilyAdministratorAssignments.ToListAsync());
        });
    }

    [Fact]
    public async Task ConflictingFixtureAdministratorAssignmentFailsWithoutReassignment()
    {
        if (!SqlTestsEnabled()) return;

        var ownerMembershipId = Guid.NewGuid();
        var unrelatedMembershipId = Guid.NewGuid();
        var unrelatedFamilyId = Guid.NewGuid();
        await WithTestDatabaseAsync(async connectionString =>
        {
            await SeedFixtureIdentityAsync(connectionString, ownerMembershipId, null);
            await using (var db = CreateContext(connectionString))
            {
                var unrelatedFamily = new FamilyGroup(unrelatedFamilyId, "Unrelated family", 10);
                db.FamilyGroups.Add(unrelatedFamily);
                db.FamilyGroupMemberships.Add(new FamilyGroupMembership(
                    unrelatedMembershipId, RecipientAccountId, RecipientProfileId, unrelatedFamilyId, true));
                db.FamilyAdministratorAssignments.Add(
                    new FamilyAdministratorAssignment(AdministratorAssignmentId, unrelatedMembershipId));
                await db.SaveChangesAsync();
            }

            using var factory = new DevelopmentFactory(connectionString);
            var exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                using var client = factory.CreateClient();
                await client.GetAsync("/health");
            });
            Assert.Contains("administrator assignment is linked to a different membership", exception.ToString(), StringComparison.Ordinal);

            await using var verify = CreateContext(connectionString);
            var assignment = await verify.FamilyAdministratorAssignments.SingleAsync();
            Assert.Equal(unrelatedMembershipId, assignment.MembershipId);
        });
    }

    private static bool SqlTestsEnabled() =>
        Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") == "1";

    private static async Task SeedFixtureIdentityAsync(
        string connectionString,
        Guid? ownerMembershipId,
        Guid? recipientMembershipId,
        bool recipientIsActive = true,
        Guid? recipientProfileId = null)
    {
        await using var db = CreateContext(connectionString);
        await db.Database.MigrateAsync();

        var ownerAccountId = Guid.Parse(DevelopmentFixtureRegistration.OwnerAccountId);
        var actualRecipientProfileId = recipientProfileId ?? RecipientProfileId;
        db.FamilyGroups.Add(new FamilyGroup(FamilyGroupId, "Development Family", 100));
        db.Accounts.AddRange(
            new Account(ownerAccountId, "development-owner@example.invalid"),
            new Account(RecipientAccountId, "development-recipient@example.invalid"));
        db.AdultProfiles.AddRange(
            new AdultProfile(OwnerProfileId, ownerAccountId, "Development Adult"),
            new AdultProfile(RecipientProfileId, RecipientAccountId, "Development Recipient"));
        if (actualRecipientProfileId != RecipientProfileId)
        {
            var unrelatedAccountId = Guid.NewGuid();
            db.Accounts.Add(new Account(unrelatedAccountId, $"{unrelatedAccountId:N}@example.invalid"));
            db.AdultProfiles.Add(new AdultProfile(actualRecipientProfileId, unrelatedAccountId, "Unexpected profile"));
        }

        if (ownerMembershipId is not null)
            db.FamilyGroupMemberships.Add(new FamilyGroupMembership(
                ownerMembershipId.Value, ownerAccountId, OwnerProfileId, FamilyGroupId, true));
        if (recipientMembershipId is not null)
            db.FamilyGroupMemberships.Add(new FamilyGroupMembership(
                recipientMembershipId.Value, RecipientAccountId, actualRecipientProfileId, FamilyGroupId,
                recipientIsActive));

        await db.SaveChangesAsync();
    }

    private static async Task<FixtureState> ReadFixtureStateAsync(string connectionString)
    {
        await using var db = CreateContext(connectionString);
        var memberships = await db.FamilyGroupMemberships
            .Where(row => row.FamilyGroupId == FamilyGroupId)
            .OrderBy(row => row.AccountId)
            .ToListAsync();
        var ownerId = Guid.Parse(DevelopmentFixtureRegistration.OwnerAccountId);
        var owner = memberships.Single(row => row.AccountId == ownerId);
        var recipient = memberships.Single(row => row.AccountId == RecipientAccountId);
        var assignment = await db.FamilyAdministratorAssignments.SingleAsync();
        return new FixtureState(
            memberships.Count,
            owner.Id,
            recipient.Id,
            assignment.MembershipId,
            owner.AdultProfileId,
            recipient.AdultProfileId,
            owner.IsActive,
            recipient.IsActive);
    }

    private static FamilySafetyDbContext CreateContext(string connectionString) => new(
        new DbContextOptionsBuilder<FamilySafetyDbContext>().UseSqlServer(connectionString).Options);

    private static async Task WithTestDatabaseAsync(Func<string, Task> test)
    {
        var database = $"FamilySafety_DevelopmentFixtureTests_{Guid.NewGuid():N}";
        var connectionString = $"Server=localhost;Database={database};Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
        var cleanupRequired = false;
        try
        {
            await using (var setup = CreateContext(connectionString))
                await setup.Database.EnsureDeletedAsync();
            cleanupRequired = true;
            await test(connectionString);
        }
        finally
        {
            if (cleanupRequired)
            {
                await using var cleanup = CreateContext(connectionString);
                await cleanup.Database.EnsureDeletedAsync();
            }
        }
    }

    private sealed record FixtureState(
        int MembershipsCount,
        Guid OwnerMembershipId,
        Guid RecipientMembershipId,
        Guid AdministratorMembershipId,
        Guid OwnerProfileId,
        Guid RecipientProfileId,
        bool OwnerIsActive,
        bool RecipientIsActive);

    private sealed class DevelopmentFactory(string connectionString) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("ConnectionStrings:FamilySafety", connectionString);
            builder.UseSetting("DevelopmentIdentity:Enabled", "true");
            builder.UseSetting("DevelopmentIdentity:AccountId", DevelopmentFixtureRegistration.OwnerAccountId);
            builder.UseSetting("DevelopmentFixtures:Enabled", "true");
            builder.UseSetting("DevelopmentFixtures:InvitationScenario", "false");
            builder.UseSetting("DevelopmentFixtures:TripAudienceScenario", "false");
        }
    }
}
