using System.Security.Cryptography;
using System.Text;
using FamilySafety.Application.Invitations;
using FamilySafety.Domain.Profiles;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Foundation.Tests;

public sealed class MembershipInvitationConcurrencyTests
{
    private const string ConnectionString =
        "Server=localhost;Database=FamilySafety_WI005002_ConcurrencyTests;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

    [Fact]
    public async Task ParallelAcceptancesCannotOverflowCapacity()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;

        var fixture = await CreateFixtureAsync(capacity: 2, invitationCount: 2);
        var outcomes = await Task.WhenAll(
            AcceptAsync(fixture.RecipientAccountIds[0], fixture.TokenHashes[0]),
            AcceptAsync(fixture.RecipientAccountIds[1], fixture.TokenHashes[1]));

        Assert.Single(outcomes, outcome => outcome == InvitationRepositoryOutcome.Success);
        Assert.Single(outcomes, outcome => outcome == InvitationRepositoryOutcome.CapacityReached);
        await using var dbContext = CreateContext();
        Assert.Equal(2, await dbContext.FamilyGroupMemberships.CountAsync(membership => membership.IsActive));
    }

    [Fact]
    public async Task ParallelUseOfOneTokenCreatesAtMostOneMembership()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;

        var fixture = await CreateFixtureAsync(capacity: 3, invitationCount: 1);
        var outcomes = await Task.WhenAll(
            AcceptAsync(fixture.RecipientAccountIds[0], fixture.TokenHashes[0]),
            AcceptAsync(fixture.RecipientAccountIds[0], fixture.TokenHashes[0]));

        Assert.Single(outcomes, outcome => outcome == InvitationRepositoryOutcome.Success);
        Assert.Single(outcomes, outcome => outcome == InvitationRepositoryOutcome.Unavailable);
        await using var dbContext = CreateContext();
        Assert.Equal(1, await dbContext.FamilyGroupMemberships.CountAsync(membership =>
            membership.AccountId == fixture.RecipientAccountIds[0]));
    }

    [Fact]
    public async Task AuthorizationAndResendAreEnforcedByPersistence()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;

        var fixture = await CreateFixtureAsync(capacity: 4, invitationCount: 0);
        await using var dbContext = CreateContext();
        var repository = new EfMembershipInvitationRepository(dbContext);
        var now = DateTimeOffset.UtcNow;
        var unauthorized = await repository.IssueAsync(
            fixture.RecipientAccountIds[0], fixture.GroupId, "new@example.invalid", "NEW@EXAMPLE.INVALID",
            Hash("unauthorized"), now, now.AddDays(7), CancellationToken.None);
        var first = await repository.IssueAsync(
            fixture.OwnerAccountId, fixture.GroupId, "new@example.invalid", "NEW@EXAMPLE.INVALID",
            Hash("first"), now, now.AddDays(7), CancellationToken.None);
        var resent = await repository.IssueAsync(
            fixture.OwnerAccountId, fixture.GroupId, "NEW@example.invalid", "NEW@EXAMPLE.INVALID",
            Hash("second"), now.AddMinutes(1), now.AddDays(7).AddMinutes(1), CancellationToken.None);

        Assert.Equal(InvitationRepositoryOutcome.Unavailable, unauthorized);
        Assert.Equal(InvitationRepositoryOutcome.Success, first);
        Assert.Equal(InvitationRepositoryOutcome.Success, resent);
        Assert.Equal(1, await dbContext.MembershipInvitations.CountAsync(invitation =>
            invitation.Status == MembershipInvitationStatus.Pending));
        Assert.Equal(1, await dbContext.MembershipInvitations.CountAsync(invitation =>
            invitation.Status == MembershipInvitationStatus.Revoked));
    }

    private static async Task<InvitationRepositoryOutcome> AcceptAsync(Guid accountId, string tokenHash)
    {
        await using var dbContext = CreateContext();
        var repository = new EfMembershipInvitationRepository(dbContext);
        return (await repository.AcceptAsync(accountId, tokenHash, DateTimeOffset.UtcNow, CancellationToken.None)).Outcome;
    }

    private static async Task<TestFixture> CreateFixtureAsync(int capacity, int invitationCount)
    {
        await using var dbContext = CreateContext();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
        var groupId = Guid.NewGuid();
        var ownerAccountId = Guid.NewGuid();
        var ownerProfileId = Guid.NewGuid();
        var ownerMembershipId = Guid.NewGuid();
        dbContext.AddRange(
            new Account(ownerAccountId, "owner@example.invalid"),
            new AdultProfile(ownerProfileId, ownerAccountId, "Owner"),
            new FamilyGroup(groupId, "Concurrency Family", capacity),
            new FamilyGroupMembership(ownerMembershipId, ownerAccountId, ownerProfileId, groupId, true),
            new FamilyAdministratorAssignment(Guid.NewGuid(), ownerMembershipId));

        var accounts = new List<Guid>();
        var hashes = new List<string>();
        var requiredRecipients = Math.Max(invitationCount, 2);
        for (var index = 0; index < requiredRecipients; index++)
        {
            var accountId = Guid.NewGuid();
            accounts.Add(accountId);
            var profileId = Guid.NewGuid();
            dbContext.AddRange(
                new Account(accountId, $"recipient-{index}@example.invalid"),
                new AdultProfile(profileId, accountId, $"Recipient {index}"));
            if (index < invitationCount)
            {
                var hash = Hash($"token-{index}");
                hashes.Add(hash);
                dbContext.Add(new MembershipInvitation(
                    Guid.NewGuid(), groupId, ownerMembershipId, $"recipient-{index}@example.invalid",
                    $"RECIPIENT-{index}@EXAMPLE.INVALID", hash, DateTimeOffset.UtcNow,
                    DateTimeOffset.UtcNow.AddDays(7)));
            }
        }

        await dbContext.SaveChangesAsync();
        return new TestFixture(ownerAccountId, groupId, accounts, hashes);
    }

    private static FamilySafetyDbContext CreateContext() => new(
        new DbContextOptionsBuilder<FamilySafetyDbContext>().UseSqlServer(ConnectionString).Options);

    private static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private sealed record TestFixture(
        Guid OwnerAccountId,
        Guid GroupId,
        IReadOnlyList<Guid> RecipientAccountIds,
        IReadOnlyList<string> TokenHashes);
}
