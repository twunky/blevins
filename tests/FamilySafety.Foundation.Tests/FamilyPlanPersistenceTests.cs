using FamilySafety.Application.Plans;
using FamilySafety.Domain.Plans;
using FamilySafety.Domain.Profiles;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Foundation.Tests;

public sealed class FamilyPlanPersistenceTests
{
    [Fact]
    public async Task WholeGroupPlanIsVisibleToActiveGroupAndRetainedWhenCreatorLeaves()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;

        await WithTestDatabaseAsync(async dbContext =>
        {
            var creator = AddAdult(dbContext, "Creator");
            var member = AddAdult(dbContext, "Member");
            var outsider = AddAdult(dbContext, "Outsider");
            var group = new FamilyGroup(Guid.NewGuid(), "Plan family", 100);
            var unrelatedGroup = new FamilyGroup(Guid.NewGuid(), "Other family", 100);
            var creatorMembership = new FamilyGroupMembership(
                Guid.NewGuid(), creator.AccountId, creator.ProfileId, group.Id, true);
            var memberMembership = new FamilyGroupMembership(
                Guid.NewGuid(), member.AccountId, member.ProfileId, group.Id, true);
            dbContext.AddRange(group, unrelatedGroup, creatorMembership,
                memberMembership,
                new FamilyGroupMembership(Guid.NewGuid(), outsider.AccountId, outsider.ProfileId, unrelatedGroup.Id, true));
            await dbContext.SaveChangesAsync();

            var repository = new EfFamilyPlanRepository(dbContext);
            var notes = "Bring snacks.\nMeet by the north entrance.";
            var created = await repository.CreateAsync(
                creator.AccountId,
                group.Id,
                Request(PlanAudienceType.FamilyGroup, []) with { Notes = notes },
                CancellationToken.None);

            Assert.NotNull(created);
            Assert.Equal(notes, created.Notes);
            Assert.Equal(notes, Assert.Single((await repository.GetVisibleAsync(member.AccountId, group.Id, CancellationToken.None))!).Notes);
            Assert.Empty((await repository.GetVisibleAsync(outsider.AccountId, unrelatedGroup.Id, CancellationToken.None))!);

            var newcomer = AddAdult(dbContext, "New member");
            dbContext.FamilyGroupMemberships.Add(new FamilyGroupMembership(
                Guid.NewGuid(), newcomer.AccountId, newcomer.ProfileId, group.Id, true));
            await dbContext.SaveChangesAsync();
            Assert.Single((await repository.GetVisibleAsync(newcomer.AccountId, group.Id, CancellationToken.None))!);

            memberMembership.End();
            await dbContext.SaveChangesAsync();
            Assert.Null(await repository.GetVisibleAsync(member.AccountId, group.Id, CancellationToken.None));

            await dbContext.FamilyGroupMemberships
                .Where(membership => membership.Id == memberMembership.Id)
                .ExecuteUpdateAsync(updates => updates.SetProperty(membership => membership.IsActive, true));
            Assert.Single((await repository.GetVisibleAsync(member.AccountId, group.Id, CancellationToken.None))!);

            creatorMembership.End();
            await dbContext.SaveChangesAsync();

            Assert.Equal(1, await dbContext.FamilyPlans.CountAsync());
            Assert.Single((await repository.GetVisibleAsync(member.AccountId, group.Id, CancellationToken.None))!);
        });
    }

    [Fact]
    public async Task SelectedPeoplePlanIsVisibleOnlyToCreatorAndSelectedActiveMember()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;

        await WithTestDatabaseAsync(async dbContext =>
        {
            var creator = AddAdult(dbContext, "Creator");
            var selected = AddAdult(dbContext, "Selected");
            var other = AddAdult(dbContext, "Other");
            var ended = AddAdult(dbContext, "Former member");
            var group = new FamilyGroup(Guid.NewGuid(), "Plan family", 100);
            dbContext.AddRange(group,
                new FamilyGroupMembership(Guid.NewGuid(), creator.AccountId, creator.ProfileId, group.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), selected.AccountId, selected.ProfileId, group.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), other.AccountId, other.ProfileId, group.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), ended.AccountId, ended.ProfileId, group.Id, false));
            await dbContext.SaveChangesAsync();

            var repository = new EfFamilyPlanRepository(dbContext);
            var notes = "Meet at the playground.\nBring water.";
            var created = await repository.CreateAsync(
                creator.AccountId,
                group.Id,
                Request(PlanAudienceType.SelectedPeople, [selected.ProfileId]) with { Notes = notes },
                CancellationToken.None);

            Assert.NotNull(created);
            Assert.Equal(notes, created.Notes);
            Assert.Equal(notes, Assert.Single((await repository.GetVisibleAsync(creator.AccountId, group.Id, CancellationToken.None))!).Notes);
            Assert.Equal(notes, Assert.Single((await repository.GetVisibleAsync(selected.AccountId, group.Id, CancellationToken.None))!).Notes);
            Assert.Empty((await repository.GetVisibleAsync(other.AccountId, group.Id, CancellationToken.None))!);
            Assert.Null(await repository.GetVisibleAsync(ended.AccountId, group.Id, CancellationToken.None));
        });
    }

    [Fact]
    public async Task CreationRejectsCreatorInactiveGroupMemberAndOutOfGroupRecipients()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;

        await WithTestDatabaseAsync(async dbContext =>
        {
            var creator = AddAdult(dbContext, "Creator");
            var inactive = AddAdult(dbContext, "Inactive");
            var outsider = AddAdult(dbContext, "Outsider");
            var group = new FamilyGroup(Guid.NewGuid(), "Plan family", 100);
            var unrelatedGroup = new FamilyGroup(Guid.NewGuid(), "Other family", 100);
            dbContext.AddRange(group, unrelatedGroup,
                new FamilyGroupMembership(Guid.NewGuid(), creator.AccountId, creator.ProfileId, group.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), inactive.AccountId, inactive.ProfileId, group.Id, false),
                new FamilyGroupMembership(Guid.NewGuid(), outsider.AccountId, outsider.ProfileId, unrelatedGroup.Id, true));
            await dbContext.SaveChangesAsync();

            var repository = new EfFamilyPlanRepository(dbContext);
            Assert.Null(await repository.CreateAsync(
                creator.AccountId, group.Id,
                Request(PlanAudienceType.SelectedPeople, [creator.ProfileId]), CancellationToken.None));
            Assert.Null(await repository.CreateAsync(
                creator.AccountId, group.Id,
                Request(PlanAudienceType.SelectedPeople, [inactive.ProfileId]), CancellationToken.None));
            Assert.Null(await repository.CreateAsync(
                creator.AccountId, group.Id,
                Request(PlanAudienceType.SelectedPeople, [outsider.ProfileId]), CancellationToken.None));
            Assert.Empty(dbContext.FamilyPlans);
        });
    }

    [Fact]
    public async Task ItemReadReturnsTheListEntryOnlyUnderTheExistingPlanAudienceRule()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;

        await WithTestDatabaseAsync(async dbContext =>
        {
            var creator = AddAdult(dbContext, "Creator");
            var selected = AddAdult(dbContext, "Selected");
            var other = AddAdult(dbContext, "Not selected");
            var administrator = AddAdult(dbContext, "Administrator");
            var ended = AddAdult(dbContext, "Former member");
            var outsider = AddAdult(dbContext, "Other family member");
            var group = new FamilyGroup(Guid.NewGuid(), "Plan family", 100);
            var otherGroup = new FamilyGroup(Guid.NewGuid(), "Other family", 100);
            var administratorMembership = Membership(administrator, group);
            dbContext.AddRange(group, otherGroup,
                Membership(creator, group), Membership(selected, group), Membership(other, group),
                administratorMembership, Membership(ended, group, isActive: false),
                Membership(outsider, otherGroup));
            dbContext.FamilyAdministratorAssignments.Add(
                new FamilyAdministratorAssignment(Guid.NewGuid(), administratorMembership.Id));
            await dbContext.SaveChangesAsync();

            var repository = new EfFamilyPlanRepository(dbContext);
            var notes = "Meet at the playground.\nBring water.";
            var wholeGroup = (await repository.CreateAsync(
                creator.AccountId, group.Id,
                Request(PlanAudienceType.FamilyGroup, []) with { Notes = "Everyone welcome." },
                CancellationToken.None))!;
            var selectedPeople = (await repository.CreateAsync(
                creator.AccountId, group.Id,
                Request(PlanAudienceType.SelectedPeople, [selected.ProfileId]) with { Notes = notes },
                CancellationToken.None))!;

            // AC-06: creator and selected recipient read the selected-people Plan, with notes.
            Assert.Equal(notes, (await repository.GetAsync(creator.AccountId, selectedPeople.PlanId, CancellationToken.None))?.Notes);
            var selectedItem = await repository.GetAsync(selected.AccountId, selectedPeople.PlanId, CancellationToken.None);
            Assert.Equal(notes, selectedItem?.Notes);
            Assert.Equal(group.Id, selectedItem?.FamilyGroupId);
            // AC-06: any active member, the administrator included, reads the whole-group Plan.
            Assert.NotNull(await repository.GetAsync(other.AccountId, wholeGroup.PlanId, CancellationToken.None));
            Assert.NotNull(await repository.GetAsync(administrator.AccountId, wholeGroup.PlanId, CancellationToken.None));

            // AC-07: every unavailable case is the same null.
            Assert.Null(await repository.GetAsync(selected.AccountId, Guid.NewGuid(), CancellationToken.None));
            Assert.Null(await repository.GetAsync(selected.AccountId, Guid.Empty, CancellationToken.None));
            Assert.Null(await repository.GetAsync(other.AccountId, selectedPeople.PlanId, CancellationToken.None));
            Assert.Null(await repository.GetAsync(administrator.AccountId, selectedPeople.PlanId, CancellationToken.None));
            Assert.Null(await repository.GetAsync(ended.AccountId, wholeGroup.PlanId, CancellationToken.None));
            Assert.Null(await repository.GetAsync(outsider.AccountId, wholeGroup.PlanId, CancellationToken.None));
            Assert.Null(await repository.GetAsync(outsider.AccountId, selectedPeople.PlanId, CancellationToken.None));

            // AC-10: the item read equals the list entry for every caller, or is null when the list excludes it.
            var callers = new[] { creator, selected, other, administrator, ended, outsider };
            (Guid, Guid)[] plans = [(wholeGroup.PlanId, group.Id), (selectedPeople.PlanId, group.Id)];
            Assert.Equal(6, await AssertItemReadsMatchListsAsync(repository, callers, plans));
        });
    }

    [Fact]
    public async Task ItemReadFollowsMembershipChangesCreatorDepartureAndCrossFamilyAudienceLikeTheList()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;

        await WithTestDatabaseAsync(async dbContext =>
        {
            var creator = AddAdult(dbContext, "Creator");
            var selected = AddAdult(dbContext, "Selected");
            var member = AddAdult(dbContext, "Member");
            var restored = AddAdult(dbContext, "Restored, never selected");
            var twoFamilies = AddAdult(dbContext, "Member of two families");
            var otherCreator = AddAdult(dbContext, "Other family creator");
            var group = new FamilyGroup(Guid.NewGuid(), "Plan family", 100);
            var otherGroup = new FamilyGroup(Guid.NewGuid(), "Second family", 100);
            var creatorMembership = Membership(creator, group);
            var memberMembership = Membership(member, group);
            var restoredMembership = Membership(restored, group);
            dbContext.AddRange(group, otherGroup, creatorMembership, Membership(selected, group),
                memberMembership, restoredMembership,
                Membership(twoFamilies, group), Membership(twoFamilies, otherGroup),
                Membership(otherCreator, otherGroup));
            await dbContext.SaveChangesAsync();

            var repository = new EfFamilyPlanRepository(dbContext);
            var wholeGroup = (await repository.CreateAsync(
                creator.AccountId, group.Id, Request(PlanAudienceType.FamilyGroup, []),
                CancellationToken.None))!;
            var selectedPeople = (await repository.CreateAsync(
                creator.AccountId, group.Id, Request(PlanAudienceType.SelectedPeople, [selected.ProfileId]),
                CancellationToken.None))!;
            var otherFamilyPlan = (await repository.CreateAsync(
                otherCreator.AccountId, otherGroup.Id,
                Request(PlanAudienceType.SelectedPeople, [twoFamilies.ProfileId]), CancellationToken.None))!;
            var callers = new List<(Guid AccountId, Guid ProfileId)>
            {
                creator, selected, member, restored, twoFamilies, otherCreator,
            };
            (Guid, Guid)[] plans =
            [
                (wholeGroup.PlanId, group.Id),
                (selectedPeople.PlanId, group.Id),
                (otherFamilyPlan.PlanId, otherGroup.Id),
            ];
            // Whole group: 5 members of the Plan family; selected: creator and selected; other family: 2.
            Assert.Equal(9, await AssertItemReadsMatchListsAsync(repository, callers, plans));

            // AC-08: read by ID alone; the response names the second Family.
            var crossFamily = await repository.GetAsync(
                twoFamilies.AccountId, otherFamilyPlan.PlanId, CancellationToken.None);
            Assert.Equal(otherGroup.Id, crossFamily?.FamilyGroupId);
            Assert.Null(await repository.GetAsync(
                twoFamilies.AccountId, selectedPeople.PlanId, CancellationToken.None));

            // AC-06: a member who joins after creation reads the whole-group Plan only.
            var newcomer = AddAdult(dbContext, "New member");
            dbContext.FamilyGroupMemberships.Add(Membership(newcomer, group));
            await dbContext.SaveChangesAsync();
            callers.Add(newcomer);
            Assert.NotNull(await repository.GetAsync(newcomer.AccountId, wholeGroup.PlanId, CancellationToken.None));
            Assert.Null(await repository.GetAsync(newcomer.AccountId, selectedPeople.PlanId, CancellationToken.None));
            Assert.Equal(10, await AssertItemReadsMatchListsAsync(repository, callers, plans));

            // AC-09: an ended member loses the whole-group Plan; restoring the membership restores it.
            memberMembership.End();
            await dbContext.SaveChangesAsync();
            Assert.Null(await repository.GetAsync(member.AccountId, wholeGroup.PlanId, CancellationToken.None));
            Assert.Equal(9, await AssertItemReadsMatchListsAsync(repository, callers, plans));
            await RestoreMembershipAsync(dbContext, memberMembership.Id);
            Assert.NotNull(await repository.GetAsync(member.AccountId, wholeGroup.PlanId, CancellationToken.None));

            // AC-07: an ended-then-restored member who was never selected stays unavailable.
            restoredMembership.End();
            await dbContext.SaveChangesAsync();
            await RestoreMembershipAsync(dbContext, restoredMembership.Id);
            Assert.Null(await repository.GetAsync(restored.AccountId, selectedPeople.PlanId, CancellationToken.None));
            Assert.Equal(10, await AssertItemReadsMatchListsAsync(repository, callers, plans));

            // AC-09: after the creator leaves, current members keep the Plans and the creator loses both.
            creatorMembership.End();
            await dbContext.SaveChangesAsync();
            Assert.Null(await repository.GetAsync(creator.AccountId, wholeGroup.PlanId, CancellationToken.None));
            Assert.Null(await repository.GetAsync(creator.AccountId, selectedPeople.PlanId, CancellationToken.None));
            Assert.NotNull(await repository.GetAsync(member.AccountId, wholeGroup.PlanId, CancellationToken.None));
            Assert.NotNull(await repository.GetAsync(selected.AccountId, selectedPeople.PlanId, CancellationToken.None));
            Assert.Equal(8, await AssertItemReadsMatchListsAsync(repository, callers, plans));
        });
    }

    // For every caller and Plan, the item read must equal that Plan's entry in the
    // caller's Family-scoped list, and must be null exactly when the list excludes it
    // (or the list itself is unavailable). Returns the number of readable pairs.
    private static async Task<int> AssertItemReadsMatchListsAsync(
        EfFamilyPlanRepository repository,
        IEnumerable<(Guid AccountId, Guid ProfileId)> callers,
        IEnumerable<(Guid PlanId, Guid FamilyGroupId)> plans)
    {
        var readable = 0;
        foreach (var caller in callers)
        {
            foreach (var (planId, familyGroupId) in plans)
            {
                var list = await repository.GetVisibleAsync(
                    caller.AccountId, familyGroupId, CancellationToken.None);
                var expected = list?.SingleOrDefault(view => view.PlanId == planId);
                var actual = await repository.GetAsync(caller.AccountId, planId, CancellationToken.None);
                Assert.Equal(expected, actual);
                if (actual is not null) readable++;
            }
        }

        return readable;
    }

    private static FamilyGroupMembership Membership(
        (Guid AccountId, Guid ProfileId) adult, FamilyGroup group, bool isActive = true) =>
        new(Guid.NewGuid(), adult.AccountId, adult.ProfileId, group.Id, isActive);

    private static Task<int> RestoreMembershipAsync(FamilySafetyDbContext dbContext, Guid membershipId) =>
        dbContext.FamilyGroupMemberships
            .Where(membership => membership.Id == membershipId)
            .ExecuteUpdateAsync(updates => updates.SetProperty(membership => membership.IsActive, true));

    private static CreateFamilyPlanRequest Request(
        PlanAudienceType audienceType,
        IReadOnlyList<Guid> audienceProfileIds) => new(
        "Visit the lake",
        new DateOnly(2026, 10, 3),
        new TimeOnly(10, 0),
        null,
        null,
        null,
        null,
        audienceType,
        audienceProfileIds);

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
        var connectionString = $"Server=localhost;Database=FamilySafety_WI007001_PlanTests_{Guid.NewGuid():N};Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
        await using var dbContext = CreateContext(connectionString);
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
