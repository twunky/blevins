using FamilySafety.Domain.Profiles;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Foundation.Tests;

public sealed class FamilyDirectoryPersistenceTests
{
    private const string ConnectionString =
        "Server=localhost;Database=FamilySafety_WI005003_DirectoryTests;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

    [Fact]
    public async Task RepositoryScopesGroupsAndMembersToActiveMembership()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;

        await using var dbContext = CreateContext();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
        var viewer = AddAdult(dbContext, "Viewer");
        var activeMember = AddAdult(dbContext, "Active Member");
        var inactiveMember = AddAdult(dbContext, "Inactive Member");
        var activeGroup = new FamilyGroup(Guid.NewGuid(), "Active Family", 100);
        var unrelatedGroup = new FamilyGroup(Guid.NewGuid(), "Unrelated Family", 100);
        dbContext.AddRange(activeGroup, unrelatedGroup);
        dbContext.AddRange(
            new FamilyGroupMembership(Guid.NewGuid(), viewer.AccountId, viewer.ProfileId, activeGroup.Id, true),
            new FamilyGroupMembership(Guid.NewGuid(), activeMember.AccountId, activeMember.ProfileId, activeGroup.Id, true),
            new FamilyGroupMembership(Guid.NewGuid(), inactiveMember.AccountId, inactiveMember.ProfileId, activeGroup.Id, false));
        await dbContext.SaveChangesAsync();
        var repository = new EfFamilyDirectoryRepository(dbContext);

        var groups = await repository.GetActiveGroupsAsync(viewer.AccountId, CancellationToken.None);
        var members = await repository.GetActiveMembersAsync(viewer.AccountId, activeGroup.Id, CancellationToken.None);
        var unrelated = await repository.GetActiveMembersAsync(viewer.AccountId, unrelatedGroup.Id, CancellationToken.None);

        Assert.Collection(groups, group => Assert.Equal(activeGroup.Id, group.FamilyGroupId));
        Assert.Equal(2, members!.Count);
        Assert.DoesNotContain(members, member => member.ProfileId == inactiveMember.ProfileId);
        Assert.Null(unrelated);
    }

    private static (Guid AccountId, Guid ProfileId) AddAdult(FamilySafetyDbContext dbContext, string name)
    {
        var accountId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        dbContext.AddRange(
            new Account(accountId, $"{accountId:N}@example.invalid"),
            new AdultProfile(profileId, accountId, name));
        return (accountId, profileId);
    }

    private static FamilySafetyDbContext CreateContext() => new(
        new DbContextOptionsBuilder<FamilySafetyDbContext>().UseSqlServer(ConnectionString).Options);
}
