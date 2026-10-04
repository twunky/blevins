using FamilySafety.Application.Events;
using FamilySafety.Domain.Events;
using FamilySafety.Domain.Profiles;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Foundation.Tests;

public sealed class FamilyEventPersistenceTests
{
    private static string CreateConnectionString() =>
        $"Server=localhost;Database=FamilySafety_WI006002_EventTests_{Guid.NewGuid():N};Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

    [Fact]
    public async Task EventAccessIsLimitedToCreatorAndSelectedActiveFamilyMembers()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;

        await WithTestDatabaseAsync(async dbContext =>
        {
            var creator = AddAdult(dbContext, "Creator");
            var selected = AddAdult(dbContext, "Selected");
            var other = AddAdult(dbContext, "Other");
            var ended = AddAdult(dbContext, "Former member");
            var group = new FamilyGroup(Guid.NewGuid(), "Event family", 100);
            dbContext.FamilyGroups.Add(group);
            dbContext.FamilyGroupMemberships.AddRange(
                new FamilyGroupMembership(Guid.NewGuid(), creator.AccountId, creator.ProfileId, group.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), selected.AccountId, selected.ProfileId, group.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), other.AccountId, other.ProfileId, group.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), ended.AccountId, ended.ProfileId, group.Id, false));
            await dbContext.SaveChangesAsync();

            var repository = new EfFamilyEventRepository(dbContext);
            var request = new CreateFamilyEventRequest(
                "Family dinner", "Bring a dish.", new DateOnly(2026, 10, 3), new TimeOnly(18, 30),
                "Community room", [selected.ProfileId], [selected.ProfileId]);
            var created = await repository.CreateAsync(
                creator.AccountId, group.Id, request, CancellationToken.None);

            Assert.NotNull(created);
            Assert.True(Assert.Single((await repository.GetVisibleAsync(
                creator.AccountId, group.Id, CancellationToken.None))!).CanEdit);
            Assert.True(Assert.Single((await repository.GetVisibleAsync(
                selected.AccountId, group.Id, CancellationToken.None))!).CanEdit);
            Assert.Empty((await repository.GetVisibleAsync(
                other.AccountId, group.Id, CancellationToken.None))!);
            Assert.Null(await repository.GetVisibleAsync(
                ended.AccountId, group.Id, CancellationToken.None));
        });
    }

    [Fact]
    public async Task CreateRejectsAudienceOutsideTheCreatorsActiveGroup()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;

        await WithTestDatabaseAsync(async dbContext =>
        {
            var creator = AddAdult(dbContext, "Creator");
            var outsider = AddAdult(dbContext, "Outside member");
            var group = new FamilyGroup(Guid.NewGuid(), "Creator family", 100);
            var unrelatedGroup = new FamilyGroup(Guid.NewGuid(), "Other family", 100);
            dbContext.AddRange(group, unrelatedGroup);
            dbContext.AddRange(
                new FamilyGroupMembership(Guid.NewGuid(), creator.AccountId, creator.ProfileId, group.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), outsider.AccountId, outsider.ProfileId, unrelatedGroup.Id, true));
            await dbContext.SaveChangesAsync();

            var request = new CreateFamilyEventRequest(
                "Family dinner", "Bring a dish.", new DateOnly(2026, 10, 3), new TimeOnly(18, 30),
                "Community room", [outsider.ProfileId], []);
            var created = await new EfFamilyEventRepository(dbContext)
                .CreateAsync(creator.AccountId, group.Id, request, CancellationToken.None);

            Assert.Null(created);
            Assert.Empty(dbContext.FamilyEvents);
        });
    }

    [Fact]
    public async Task OnlyCreatorAndSelectedEditorsCanUpdateEventDetails()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;

        await WithTestDatabaseAsync(async dbContext =>
        {
            var creator = AddAdult(dbContext, "Creator");
            var editor = AddAdult(dbContext, "Editor");
            var viewer = AddAdult(dbContext, "View only");
            var ended = AddAdult(dbContext, "Former member");
            var other = AddAdult(dbContext, "Other group member");
            var group = new FamilyGroup(Guid.NewGuid(), "Event family", 100);
            var otherGroup = new FamilyGroup(Guid.NewGuid(), "Other family", 100);
            var viewerMembership = new FamilyGroupMembership(
                Guid.NewGuid(), viewer.AccountId, viewer.ProfileId, group.Id, true);
            var endedMembership = new FamilyGroupMembership(
                Guid.NewGuid(), ended.AccountId, ended.ProfileId, group.Id, false);
            dbContext.AddRange(group, otherGroup);
            dbContext.AddRange(
                new FamilyGroupMembership(Guid.NewGuid(), creator.AccountId, creator.ProfileId, group.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), editor.AccountId, editor.ProfileId, group.Id, true),
                viewerMembership,
                endedMembership,
                new FamilyGroupMembership(Guid.NewGuid(), other.AccountId, other.ProfileId, otherGroup.Id, true));
            dbContext.FamilyAdministratorAssignments.Add(
                new FamilyAdministratorAssignment(Guid.NewGuid(), viewerMembership.Id));
            await dbContext.SaveChangesAsync();

            var repository = new EfFamilyEventRepository(dbContext);
            var created = await repository.CreateAsync(
                creator.AccountId,
                group.Id,
                new CreateFamilyEventRequest(
                    "Family dinner", "Bring a dish.", new DateOnly(2026, 10, 3),
                    new TimeOnly(18, 30), "Community room",
                    [editor.ProfileId, viewer.ProfileId], [editor.ProfileId]),
                CancellationToken.None);
            Assert.NotNull(created);

            var update = new UpdateFamilyEventRequest(
                "Updated dinner", "Bring dessert.", new DateOnly(2026, 10, 4),
                new TimeOnly(19, 0), "Garden room");
            var editorResult = await repository.UpdateAsync(
                editor.AccountId, group.Id, created.EventId, update, CancellationToken.None);
            Assert.NotNull(editorResult);
            Assert.Equal("Updated dinner", editorResult.Name);
            Assert.True(editorResult.CanEdit);

            var viewerResult = Assert.Single((await repository.GetVisibleAsync(
                viewer.AccountId, group.Id, CancellationToken.None))!);
            Assert.False(viewerResult.CanEdit);
            var creatorResult = await repository.UpdateAsync(
                creator.AccountId, group.Id, created.EventId, update, CancellationToken.None);
            Assert.NotNull(creatorResult);
            Assert.True(creatorResult.CanEdit);

            Assert.Null(await repository.UpdateAsync(
                viewer.AccountId, group.Id, created.EventId, update, CancellationToken.None));
            Assert.Null(await repository.UpdateAsync(
                ended.AccountId, group.Id, created.EventId, update, CancellationToken.None));
            Assert.Null(await repository.UpdateAsync(
                other.AccountId, group.Id, created.EventId, update, CancellationToken.None));
        });
    }

    [Fact]
    public async Task ItemReadReturnsTheListEntryOnlyToCreatorAndSelectedActiveMembers()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;

        await WithTestDatabaseAsync(async dbContext =>
        {
            var creator = AddAdult(dbContext, "Creator");
            var editor = AddAdult(dbContext, "Editor");
            var viewer = AddAdult(dbContext, "Viewer");
            var other = AddAdult(dbContext, "Not selected");
            var administrator = AddAdult(dbContext, "Administrator");
            var ended = AddAdult(dbContext, "Former member");
            var outsider = AddAdult(dbContext, "Other family member");
            var group = new FamilyGroup(Guid.NewGuid(), "Event family", 100);
            var otherGroup = new FamilyGroup(Guid.NewGuid(), "Other family", 100);
            var administratorMembership = Membership(administrator, group);
            dbContext.AddRange(group, otherGroup,
                Membership(creator, group), Membership(editor, group), Membership(viewer, group),
                Membership(other, group), administratorMembership,
                Membership(ended, group, isActive: false), Membership(outsider, otherGroup));
            dbContext.FamilyAdministratorAssignments.Add(
                new FamilyAdministratorAssignment(Guid.NewGuid(), administratorMembership.Id));
            await dbContext.SaveChangesAsync();

            var repository = new EfFamilyEventRepository(dbContext);
            var created = await repository.CreateAsync(
                creator.AccountId, group.Id,
                EventRequest("Family dinner", [editor.ProfileId, viewer.ProfileId], [editor.ProfileId]),
                CancellationToken.None);
            Assert.NotNull(created);

            // AC-01: creator and editor can edit; a selected viewer reads with CanEdit false.
            Assert.True((await repository.GetAsync(creator.AccountId, created.EventId, CancellationToken.None))!.CanEdit);
            Assert.True((await repository.GetAsync(editor.AccountId, created.EventId, CancellationToken.None))!.CanEdit);
            var viewerItem = await repository.GetAsync(viewer.AccountId, created.EventId, CancellationToken.None);
            Assert.NotNull(viewerItem);
            Assert.False(viewerItem.CanEdit);
            Assert.Equal(group.Id, viewerItem.FamilyGroupId);

            // AC-02: every unavailable case is the same null.
            Assert.Null(await repository.GetAsync(viewer.AccountId, Guid.NewGuid(), CancellationToken.None));
            Assert.Null(await repository.GetAsync(viewer.AccountId, Guid.Empty, CancellationToken.None));
            Assert.Null(await repository.GetAsync(other.AccountId, created.EventId, CancellationToken.None));
            Assert.Null(await repository.GetAsync(administrator.AccountId, created.EventId, CancellationToken.None));
            Assert.Null(await repository.GetAsync(ended.AccountId, created.EventId, CancellationToken.None));
            Assert.Null(await repository.GetAsync(outsider.AccountId, created.EventId, CancellationToken.None));

            // AC-05: the item read equals the list entry for every caller, or is null when the list excludes it.
            var callers = new[] { creator, editor, viewer, other, administrator, ended, outsider };
            Assert.Equal(3, await AssertItemReadsMatchListsAsync(
                repository, callers, [(created.EventId, group.Id)]));
        });
    }

    [Fact]
    public async Task ItemReadFollowsMembershipChangesAndCrossFamilyAudienceLikeTheList()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;

        await WithTestDatabaseAsync(async dbContext =>
        {
            var creator = AddAdult(dbContext, "Creator");
            var viewer = AddAdult(dbContext, "Viewer");
            var restored = AddAdult(dbContext, "Restored, never selected");
            var twoFamilies = AddAdult(dbContext, "Member of two families");
            var otherCreator = AddAdult(dbContext, "Other family creator");
            var group = new FamilyGroup(Guid.NewGuid(), "Event family", 100);
            var otherGroup = new FamilyGroup(Guid.NewGuid(), "Second family", 100);
            var creatorMembership = Membership(creator, group);
            var viewerMembership = Membership(viewer, group);
            var restoredMembership = Membership(restored, group);
            dbContext.AddRange(group, otherGroup, creatorMembership, viewerMembership, restoredMembership,
                Membership(twoFamilies, group), Membership(twoFamilies, otherGroup),
                Membership(otherCreator, otherGroup));
            await dbContext.SaveChangesAsync();

            var repository = new EfFamilyEventRepository(dbContext);
            var familyEvent = (await repository.CreateAsync(
                creator.AccountId, group.Id,
                EventRequest("Family dinner", [viewer.ProfileId], []), CancellationToken.None))!;
            var otherFamilyEvent = (await repository.CreateAsync(
                otherCreator.AccountId, otherGroup.Id,
                EventRequest("Second family picnic", [twoFamilies.ProfileId], []), CancellationToken.None))!;
            var callers = new[] { creator, viewer, restored, twoFamilies, otherCreator };
            (Guid, Guid)[] events = [(familyEvent.EventId, group.Id), (otherFamilyEvent.EventId, otherGroup.Id)];
            Assert.Equal(4, await AssertItemReadsMatchListsAsync(repository, callers, events));

            // AC-03: read by ID alone; the response names the second Family.
            var crossFamily = await repository.GetAsync(
                twoFamilies.AccountId, otherFamilyEvent.EventId, CancellationToken.None);
            Assert.Equal(otherGroup.Id, crossFamily?.FamilyGroupId);
            Assert.Null(await repository.GetAsync(
                twoFamilies.AccountId, familyEvent.EventId, CancellationToken.None));

            // AC-04: an ended selected member loses the read; restoring the membership restores it.
            viewerMembership.End();
            await dbContext.SaveChangesAsync();
            Assert.Null(await repository.GetAsync(viewer.AccountId, familyEvent.EventId, CancellationToken.None));
            Assert.Equal(3, await AssertItemReadsMatchListsAsync(repository, callers, events));
            await RestoreMembershipAsync(dbContext, viewerMembership.Id);
            Assert.NotNull(await repository.GetAsync(viewer.AccountId, familyEvent.EventId, CancellationToken.None));
            Assert.Equal(4, await AssertItemReadsMatchListsAsync(repository, callers, events));

            // AC-02: an ended-then-restored member who was never selected stays unavailable.
            restoredMembership.End();
            await dbContext.SaveChangesAsync();
            await RestoreMembershipAsync(dbContext, restoredMembership.Id);
            Assert.Null(await repository.GetAsync(restored.AccountId, familyEvent.EventId, CancellationToken.None));

            // AC-02: the creator loses the read after leaving; the selected viewer keeps it.
            creatorMembership.End();
            await dbContext.SaveChangesAsync();
            Assert.Null(await repository.GetAsync(creator.AccountId, familyEvent.EventId, CancellationToken.None));
            Assert.NotNull(await repository.GetAsync(viewer.AccountId, familyEvent.EventId, CancellationToken.None));
            Assert.Equal(3, await AssertItemReadsMatchListsAsync(repository, callers, events));
        });
    }

    // For every caller and Event, the item read must equal that Event's entry in the
    // caller's Family-scoped list, and must be null exactly when the list excludes it
    // (or the list itself is unavailable). Returns the number of readable pairs.
    private static async Task<int> AssertItemReadsMatchListsAsync(
        EfFamilyEventRepository repository,
        IEnumerable<(Guid AccountId, Guid ProfileId)> callers,
        IEnumerable<(Guid EventId, Guid FamilyGroupId)> events)
    {
        var readable = 0;
        foreach (var caller in callers)
        {
            foreach (var (eventId, familyGroupId) in events)
            {
                var list = await repository.GetVisibleAsync(
                    caller.AccountId, familyGroupId, CancellationToken.None);
                var expected = list?.SingleOrDefault(view => view.EventId == eventId);
                var actual = await repository.GetAsync(caller.AccountId, eventId, CancellationToken.None);
                Assert.Equal(expected, actual);
                if (actual is not null) readable++;
            }
        }

        return readable;
    }

    private static CreateFamilyEventRequest EventRequest(
        string name, IReadOnlyList<Guid> audienceProfileIds, IReadOnlyList<Guid> editorProfileIds) => new(
        name, "Bring a dish.", new DateOnly(2026, 10, 3), new TimeOnly(18, 30),
        "Community room", audienceProfileIds, editorProfileIds);

    private static FamilyGroupMembership Membership(
        (Guid AccountId, Guid ProfileId) adult, FamilyGroup group, bool isActive = true) =>
        new(Guid.NewGuid(), adult.AccountId, adult.ProfileId, group.Id, isActive);

    private static Task<int> RestoreMembershipAsync(FamilySafetyDbContext dbContext, Guid membershipId) =>
        dbContext.FamilyGroupMemberships
            .Where(membership => membership.Id == membershipId)
            .ExecuteUpdateAsync(updates => updates.SetProperty(membership => membership.IsActive, true));

    private static (Guid AccountId, Guid ProfileId) AddAdult(FamilySafetyDbContext dbContext, string name)
    {
        var accountId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        dbContext.AddRange(
            new Account(accountId, $"{accountId:N}@example.invalid"),
            new AdultProfile(profileId, accountId, name));
        return (accountId, profileId);
    }

    private static FamilySafetyDbContext CreateContext(string connectionString) => new(
        new DbContextOptionsBuilder<FamilySafetyDbContext>().UseSqlServer(connectionString).Options);

    private static async Task WithTestDatabaseAsync(Func<FamilySafetyDbContext, Task> test)
    {
        var connectionString = CreateConnectionString();
        await using (var dbContext = CreateContext(connectionString))
        {
            var cleanupRequired = false;
            try
            {
                await dbContext.Database.EnsureDeletedAsync();
                cleanupRequired = true;
                await dbContext.Database.MigrateAsync();
                await test(dbContext);
            }
            finally
            {
                if (cleanupRequired)
                {
                    await using var cleanupContext = CreateContext(connectionString);
                    await cleanupContext.Database.EnsureDeletedAsync();
                }
            }
        }
    }
}
