using System.Net;
using System.Net.Http.Json;
using FamilySafety.Application.Households;
using FamilySafety.Application.Profiles;
using FamilySafety.Domain.Profiles;
using FamilySafety.Domain.Households;
using FamilySafety.Domain.Events;
using FamilySafety.Domain.Plans;
using FamilySafety.Domain.Trips;
using FamilySafety.Domain.CheckIns;
using FamilySafety.Domain.Incidents;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit.Abstractions;

namespace FamilySafety.Foundation.Tests;

public sealed class HouseholdTests(ITestOutputHelper output)
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private static string Url(Guid family) => $"/api/family-groups/{family}/households";
    private static EfHouseholdRepository Repo(World w) => new(w.Db, TimeProvider.System);
    private static async Task<HouseholdView> Current(World w, Guid id) =>
        (await Repo(w).ListAsync(w.Admin.Account, w.Group, Ct))!.Households.Single(h => h.HouseholdId == id);

    [Fact]
    public async Task ApiRechecksMembershipAndAdminAuthorityAndExposesOnlyOrganization()
    {
        await WithDatabase(async w =>
        {
            using var factory = new Factory(w.Connection);
            using var client = factory.CreateClient();
            var url = Url(w.Group);
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(url)).StatusCode);
            factory.Actor.Current = new(w.Admin.Account);
            var response = await client.PostAsJsonAsync(url, new { name = " Home ", ownerProfileId = w.Outsider.Profile });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Headers.CacheControl!.NoStore);
            var household = (await response.Content.ReadFromJsonAsync<HouseholdView>())!;
            Assert.Equal("Home", household.Name);
            var member = await client.GetFromJsonAsync<HouseholdStructureView>(url);
            Assert.True(member!.CanManage);
            factory.Actor.Current = new(w.Member.Account);
            var read = await client.GetAsync(url);
            Assert.True(read.Headers.CacheControl!.NoStore);
            var view = (await read.Content.ReadFromJsonAsync<HouseholdStructureView>())!;
            Assert.False(view.CanManage);
            Assert.Single(view.Households);
            Assert.Equal(3, view.UnassignedMembers.Count);
            var json = await read.Content.ReadAsStringAsync();
            Assert.DoesNotContain("contact", json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("@example", json, StringComparison.OrdinalIgnoreCase);
            foreach (var account in new[] { w.Member.Account, w.Outsider.Account })
            {
                factory.Actor.Current = new(account);
                Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync(url, new { name = "Denied" })).StatusCode);
                Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync(url + "/" + household.HouseholdId, new { name = "Denied", version = household.Version })).StatusCode);
                Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync(url + "/" + household.HouseholdId + "/members", new AssignHouseholdMemberRequest(household.Version, w.Member.Profile, null, null))).StatusCode);
                Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync(url + "/" + household.HouseholdId + "/members/" + w.Member.Profile + "?version=" + household.Version)).StatusCode);
                var denied = await client.DeleteAsync(url + "/" + household.HouseholdId + "?version=" + household.Version);
                var missing = await client.DeleteAsync(url + "/" + Guid.NewGuid() + "?version=" + household.Version);
                Assert.Equal(HttpStatusCode.NotFound, denied.StatusCode);
                Assert.Equal(denied.StatusCode, missing.StatusCode);
                Assert.Equal(await denied.Content.ReadAsStringAsync(), await missing.Content.ReadAsStringAsync());
            }
            // Supplied presentation context and known household identifiers grant nothing.
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(url + "?selectedFamilyId=" + w.Group)).StatusCode);
            factory.Actor.Current = new(w.Admin.Account);
            Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync(Url(w.OtherGroup) + "/" + household.HouseholdId, new { name = "Cross Family", version = household.Version })).StatusCode);
            household = (await (await client.PutAsJsonAsync(url + "/" + household.HouseholdId, new { name = "Renamed", version = household.Version })).Content.ReadFromJsonAsync<HouseholdView>())!;
            Assert.Equal("Renamed", household.Name);
            Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync(url + "/" + household.HouseholdId + "?version=" + Guid.NewGuid())).StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync(url + "/" + household.HouseholdId + "?version=" + household.Version)).StatusCode);
        });
    }

    [Fact]
    public async Task NamesNormalizePerFamilyAndRejectInvalidInputWithoutLosingEmptyHouseholds()
    {
        await WithDatabase(async w =>
        {
            var repo = Repo(w);
            var home = (await repo.CreateAsync(w.Admin.Account, w.Group, "Home", Ct))!;
            await using (var db = Context(w.Connection))
                await Assert.ThrowsAsync<HouseholdDuplicateException>(() => new EfHouseholdRepository(db, TimeProvider.System).CreateAsync(w.Admin.Account, w.Group, " ｈｏｍｅ ", Ct));
            Assert.NotNull(await repo.CreateAsync(w.Admin.Account, w.OtherGroup, "home", Ct));
            foreach (var name in new[] { " ", "Line\nbreak", new string('a', 101), "\ud800" })
                await Assert.ThrowsAsync<HouseholdValidationException>(() => repo.CreateAsync(w.Admin.Account, w.Group, name, Ct));
            var second = (await repo.CreateAsync(w.Admin.Account, w.Group, "Other", Ct))!;
            await using (var db = Context(w.Connection))
                await Assert.ThrowsAsync<HouseholdDuplicateException>(() => new EfHouseholdRepository(db, TimeProvider.System).RenameAsync(w.Admin.Account, w.Group, second.HouseholdId, new(second.Version, "HOME"), Ct));
            Assert.Empty((await Current(w, home.HouseholdId)).Members);
            Assert.Equal(2, (await repo.ListAsync(w.Member.Account, w.Group, Ct))!.Households.Count);
        });
    }

    [Fact]
    public async Task ActiveAdultsAssignMoveAndUnassignWithBothVersionsAndNoCrossFamilyOrInactiveAssignment()
    {
        await WithDatabase(async w =>
        {
            var repo = Repo(w);
            var a = (await repo.CreateAsync(w.Admin.Account, w.Group, "A", Ct))!;
            var b = (await repo.CreateAsync(w.Admin.Account, w.Group, "B", Ct))!;
            Assert.True(await repo.AssignAsync(w.Admin.Account, w.Group, a.HouseholdId, new(a.Version, w.Member.Profile, null, null), Ct));
            await Assert.ThrowsAsync<HouseholdConflictException>(() => repo.AssignAsync(w.Admin.Account, w.Group, b.HouseholdId, new(b.Version, w.Member.Profile, null, null), Ct));
            var oldA = a;
            a = await Current(w, a.HouseholdId);
            Assert.True(await repo.AssignAsync(w.Admin.Account, w.Group, a.HouseholdId, new(a.Version, w.Second.Profile, null, null), Ct));
            await Assert.ThrowsAsync<HouseholdConflictException>(() => repo.AssignAsync(w.Admin.Account, w.Group, b.HouseholdId, new(b.Version, w.Member.Profile, a.HouseholdId, a.Version), Ct));
            a = await Current(w, a.HouseholdId);
            Assert.True(await repo.AssignAsync(w.Admin.Account, w.Group, b.HouseholdId, new(b.Version, w.Member.Profile, a.HouseholdId, a.Version), Ct));
            Assert.Single(await w.Db.HouseholdAssignments.Where(x => x.ProfileId == w.Member.Profile).ToListAsync());
            Assert.Single((await Current(w, a.HouseholdId)).Members);
            b = await Current(w, b.HouseholdId);
            foreach (var profile in new[] { w.Outsider.Profile, w.Inactive.Profile, Guid.NewGuid() })
                Assert.False(await repo.AssignAsync(w.Admin.Account, w.Group, b.HouseholdId, new(b.Version, profile, null, null), Ct));
            await Assert.ThrowsAsync<HouseholdConflictException>(() => repo.UnassignAsync(w.Admin.Account, w.Group, a.HouseholdId, w.Second.Profile, oldA.Version, Ct));
            Assert.True(await repo.UnassignAsync(w.Admin.Account, w.Group, b.HouseholdId, w.Member.Profile, b.Version, Ct));
            Assert.Empty((await Current(w, b.HouseholdId)).Members);
            Assert.Contains((await repo.ListAsync(w.Member.Account, w.Group, Ct))!.UnassignedMembers, p => p.ProfileId == w.Member.Profile);
        });
    }

    [Fact]
    public async Task MembershipLossImmediatelyClearsAssignmentsAndRejoinAndReplacementInheritNothing()
    {
        await WithDatabase(async w =>
        {
            var repo = Repo(w);
            var home = (await repo.CreateAsync(w.Admin.Account, w.Group, "Home", Ct))!;
            Assert.True(await repo.AssignAsync(w.Admin.Account, w.Group, home.HouseholdId, new(home.Version, w.Member.Profile, null, null), Ct));
            home = await Current(w, home.HouseholdId);
            var member = await w.Db.FamilyGroupMemberships.SingleAsync(m => m.AdultProfileId == w.Member.Profile);
            member.End(); await w.Db.SaveChangesAsync();
            Assert.False(await w.Db.HouseholdAssignments.AnyAsync(a => a.ProfileId == w.Member.Profile));
            Assert.Null(await repo.ListAsync(w.Member.Account, w.Group, Ct));
            Assert.Empty((await Current(w, home.HouseholdId)).Members);
            await Assert.ThrowsAsync<HouseholdConflictException>(() => repo.DeleteAsync(w.Admin.Account, w.Group, home.HouseholdId, home.Version, Ct));
            await w.Db.FamilyGroupMemberships.Where(m => m.Id == member.Id).ExecuteUpdateAsync(u => u.SetProperty(m => m.IsActive, true));
            Assert.Contains((await repo.ListAsync(w.Member.Account, w.Group, Ct))!.UnassignedMembers, p => p.ProfileId == w.Member.Profile);
            home = await Current(w, home.HouseholdId);
            Assert.True(await repo.AssignAsync(w.Admin.Account, w.Group, home.HouseholdId, new(home.Version, w.Member.Profile, null, null), Ct));
            // Exact profile/membership provenance: replacing the member with another existing adult removes assignment.
            await w.Db.FamilyGroupMemberships.Where(m => m.Id == member.Id).ExecuteUpdateAsync(u =>
                u.SetProperty(m => m.AdultProfileId, w.Outsider.Profile).SetProperty(m => m.AccountId, w.Outsider.Account));
            Assert.False(await w.Db.HouseholdAssignments.AnyAsync(a => a.ProfileId == w.Member.Profile || a.ProfileId == w.Outsider.Profile));
            Assert.Contains((await repo.ListAsync(w.Outsider.Account, w.Group, Ct))!.UnassignedMembers, p => p.ProfileId == w.Outsider.Profile);
        });
    }

    [Fact]
    public async Task HouseholdIsFamilyOwnedAndSurvivesCreatingAdminRoleAndMembershipLoss()
    {
        await WithDatabase(async w =>
        {
            var repo = Repo(w);
            var home = (await repo.CreateAsync(w.Admin.Account, w.Group, "Family owned", Ct))!;
            var adminMembership = await w.Db.FamilyGroupMemberships.SingleAsync(m => m.AccountId == w.Admin.Account && m.FamilyGroupId == w.Group);
            var nextMembership = await w.Db.FamilyGroupMemberships.SingleAsync(m => m.AccountId == w.Second.Account);
            await w.Db.FamilyAdministratorAssignments.Where(a => a.MembershipId == adminMembership.Id).ExecuteDeleteAsync();
            Assert.Null(await repo.RenameAsync(w.Admin.Account, w.Group, home.HouseholdId, new(home.Version, "Denied"), Ct));
            Assert.False((await repo.ListAsync(w.Admin.Account, w.Group, Ct))!.CanManage);
            w.Db.Add(new FamilyAdministratorAssignment(Guid.NewGuid(), nextMembership.Id)); await w.Db.SaveChangesAsync();
            await w.Db.FamilyGroupMemberships.Where(m => m.Id == adminMembership.Id).ExecuteUpdateAsync(u => u.SetProperty(m => m.IsActive, false));
            Assert.True(await w.Db.FamilyHouseholds.AnyAsync(h => h.Id == home.HouseholdId));
            home = (await repo.RenameAsync(w.Second.Account, w.Group, home.HouseholdId, new(home.Version, "Still manageable"), Ct))!;
            Assert.True(await repo.DeleteAsync(w.Second.Account, w.Group, home.HouseholdId, home.Version, Ct));
        });
    }

    [Fact]
    public async Task GroupingAndPermanentDeletionLeaveMembershipAndProtectedDataAndAccessUnchanged()
    {
        await WithDatabase(async w =>
        {
            var date = new DateOnly(2026, 10, 5);
            var evt = new FamilyEvent(Guid.NewGuid(), w.Group, w.Admin.Profile, "Private event", "Private", date, new(12, 0), "Private");
            var plan = new FamilyPlan(Guid.NewGuid(), w.Group, w.Admin.Profile, "Private plan", date, new(12, 0), null, null, null, null, PlanAudienceType.SelectedPeople);
            var trip = new FamilyTrip(Guid.NewGuid(), w.Group, w.Admin.Profile, TripAudienceType.SelectedPeople, "Private trip", "Private", date, date, null, null, null, null, null, null);
            w.Db.AddRange(evt, plan, trip); await w.Db.SaveChangesAsync();
            var concern = (await new EfConcernRepository(w.Db, TimeProvider.System).CreateAsync(w.Admin.Account, w.Group, new(w.Member.Profile, "Private report", []), Ct))!;
            var incident = (await new EfIncidentRepository(w.Db, TimeProvider.System, new EfConcernAccessQuery(w.Db, TimeProvider.System))
                .CreateAsync(w.Admin.Account, w.Group, new(w.Member.Profile, "Private incident", IncidentInformationCategory.Reported, "Direct conversation", false, []), Ct))!;
            var checks = new EfCheckInRepository(w.Db, TimeProvider.System);
            var sessions = new EfSharingSessionRepository(w.Db, TimeProvider.System);
            var privateCheck = (await checks.CreateAsync(w.Admin.Account, new(w.Group, CheckInStatus.ImOkay, "Private", []), Ct))!;
            var sharedCheck = (await checks.CreateAsync(w.Admin.Account, new(w.Group, CheckInStatus.Arrived, "Shared", [w.Member.Profile]), Ct))!;
            var session = (await sessions.CreateAsync(w.Admin.Account, new(w.Group, [w.Member.Profile], null), Ct))!;
            var privateSession = (await sessions.CreateAsync(w.Admin.Account, new(w.Group, [w.Second.Profile], null), Ct))!;
            var beforeMemberships = await w.Db.FamilyGroupMemberships.AsNoTracking().OrderBy(m => m.Id).Select(m => new { m.Id, m.IsActive, m.AccessVersion }).ToListAsync();
            var beforeGrants = await w.Db.CheckInRecipients.AsNoTracking().OrderBy(r => r.CheckInId).Select(r => new { r.CheckInId, r.ProfileId, r.IsInvalidated }).ToListAsync();
            using var factory = new Factory(w.Connection); using var client = factory.CreateClient(); factory.Actor.Current = new(w.Member.Account);
            async Task AssertAccess()
            {
                foreach (var path in new[] { $"/api/events/{evt.Id}", $"/api/plans/{plan.Id}", $"/api/trips/{trip.Id}", $"/api/concerns/{concern.ConcernId}", $"/api/incidents/{incident.IncidentId}" })
                    Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(path)).StatusCode);
                Assert.Null(await checks.GetAsync(w.Member.Account, privateCheck.CheckInId, Ct));
                Assert.Null(await sessions.GetAsync(w.Member.Account, privateSession.SharingSessionId, Ct));
                Assert.NotNull(await checks.GetAsync(w.Member.Account, sharedCheck.CheckInId, Ct));
                Assert.NotNull(await sessions.GetAsync(w.Member.Account, session.SharingSessionId, Ct));
            }
            await AssertAccess();
            var repo = Repo(w); var home = (await repo.CreateAsync(w.Admin.Account, w.Group, "Home", Ct))!;
            Assert.True(await repo.AssignAsync(w.Admin.Account, w.Group, home.HouseholdId, new(home.Version, w.Member.Profile, null, null), Ct));
            await AssertAccess();
            home = await Current(w, home.HouseholdId);
            Assert.True(await repo.DeleteAsync(w.Admin.Account, w.Group, home.HouseholdId, home.Version, Ct));
            Assert.False(await w.Db.HouseholdAssignments.AnyAsync());
            Assert.Contains((await repo.ListAsync(w.Member.Account, w.Group, Ct))!.UnassignedMembers, p => p.ProfileId == w.Member.Profile);
            await AssertAccess();
            Assert.Equal(beforeMemberships, await w.Db.FamilyGroupMemberships.AsNoTracking().OrderBy(m => m.Id).Select(m => new { m.Id, m.IsActive, m.AccessVersion }).ToListAsync());
            Assert.Equal(beforeGrants, await w.Db.CheckInRecipients.AsNoTracking().OrderBy(r => r.CheckInId).Select(r => new { r.CheckInId, r.ProfileId, r.IsInvalidated }).ToListAsync());
            Assert.Equal("Private event", (await w.Db.FamilyEvents.AsNoTracking().SingleAsync()).Name);
            Assert.Single(await w.Db.FamilyPlans.ToListAsync()); Assert.Single(await w.Db.FamilyTrips.ToListAsync());
        });
    }

    [Fact]
    public async Task MigrationConstraintsAndConcurrentChangesPreserveOneAssignmentAndAcceptedVersions()
    {
        await WithDatabase(async w =>
        {
            await w.Db.GetService<IMigrator>().MigrateAsync("20261005215851_SafetyCircleRecipientPresets");
            await w.Db.Database.MigrateAsync(); await w.Db.Database.MigrateAsync();
            Assert.Equal(5, await w.Db.Accounts.CountAsync());
            var repo = Repo(w);
            var a = (await repo.CreateAsync(w.Admin.Account, w.Group, "A", Ct))!;
            var b = (await repo.CreateAsync(w.Admin.Account, w.Group, "B", Ct))!;
            async Task<bool> Assign(HouseholdView target)
            {
                await using var db = Context(w.Connection);
                try { return await new EfHouseholdRepository(db, TimeProvider.System).AssignAsync(w.Admin.Account, w.Group, target.HouseholdId, new(target.Version, w.Member.Profile, null, null), Ct); }
                catch (HouseholdConflictException) { return false; }
            }
            Assert.Single(await Task.WhenAll(Assign(a), Assign(b)), accepted => accepted);
            Assert.Single(await w.Db.HouseholdAssignments.ToListAsync());
            var assigned = (await repo.ListAsync(w.Admin.Account, w.Group, Ct))!.Households.Single(h => h.Members.Count > 0);
            async Task<bool> Rename(string name)
            {
                await using var db = Context(w.Connection);
                try { await new EfHouseholdRepository(db, TimeProvider.System).RenameAsync(w.Admin.Account, w.Group, assigned.HouseholdId, new(assigned.Version, name), Ct); return true; }
                catch (HouseholdConflictException) { return false; }
            }
            Assert.Single(await Task.WhenAll(Rename("One"), Rename("Two")), accepted => accepted);
            await Assert.ThrowsAsync<HouseholdConflictException>(() => repo.DeleteAsync(w.Admin.Account, w.Group, assigned.HouseholdId, assigned.Version, Ct));
            var inactiveMembership = await w.Db.FamilyGroupMemberships.AsNoTracking().SingleAsync(m => m.AdultProfileId == w.Inactive.Profile);
            await using (var db = Context(w.Connection))
            {
                db.Add(new HouseholdAssignment(w.Group, a.HouseholdId, w.Inactive.Profile, inactiveMembership.Id, inactiveMembership.AccessVersion));
                await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            }
            var memberMembership = await w.Db.FamilyGroupMemberships.AsNoTracking().SingleAsync(m => m.AdultProfileId == w.Member.Profile);
            await using (var db = Context(w.Connection))
            {
                db.Add(new HouseholdAssignment(w.Group, assigned.HouseholdId == a.HouseholdId ? b.HouseholdId : a.HouseholdId, w.Member.Profile, memberMembership.Id, memberMembership.AccessVersion));
                await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            }
            await using (var db = Context(w.Connection))
            {
                db.Add(new HouseholdAssignment(w.OtherGroup, a.HouseholdId, w.Member.Profile, memberMembership.Id, memberMembership.AccessVersion));
                await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            }
        });
    }

    private sealed record Adult(Guid Account, Guid Profile);
    private sealed record World(FamilySafetyDbContext Db, string Connection, Guid Group, Guid OtherGroup, Adult Admin, Adult Member, Adult Second, Adult Outsider, Adult Inactive);
    private static FamilySafetyDbContext Context(string connection) => new(new DbContextOptionsBuilder<FamilySafetyDbContext>().UseSqlServer(connection).Options);
    private async Task WithDatabase(Func<World, Task> action)
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        var connection = $"Server=localhost;Database=FamilySafety_WI095010_{Guid.NewGuid():N};Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True";
        await using var db = Context(connection);
        output.WriteLine("SQL gate enabled: disposable WI095010 database, migrations, API privacy/admin checks, assignment/lifecycle/concurrency assertions execute.");
        try
        {
            await db.Database.MigrateAsync();
            Adult Create(string name)
            {
                var adult = new Adult(Guid.NewGuid(), Guid.NewGuid());
                db.AddRange(new Account(adult.Account, $"{adult.Account:N}@example.invalid"), new AdultProfile(adult.Profile, adult.Account, name));
                return adult;
            }
            var admin = Create("Administrator"); var member = Create("Member"); var second = Create("Second adult"); var outsider = Create("Other Family adult"); var inactive = Create("Inactive");
            var group = new FamilyGroup(Guid.NewGuid(), "Family", 100); var other = new FamilyGroup(Guid.NewGuid(), "Other Family", 100); db.AddRange(group, other);
            foreach (var adult in new[] { admin, member, second, inactive })
            {
                var membership = new FamilyGroupMembership(Guid.NewGuid(), adult.Account, adult.Profile, group.Id, adult != inactive);
                db.Add(membership); if (adult == admin) db.Add(new FamilyAdministratorAssignment(Guid.NewGuid(), membership.Id));
            }
            foreach (var adult in new[] { admin, outsider })
            {
                var membership = new FamilyGroupMembership(Guid.NewGuid(), adult.Account, adult.Profile, other.Id, true);
                db.Add(membership); if (adult == admin) db.Add(new FamilyAdministratorAssignment(Guid.NewGuid(), membership.Id));
            }
            await db.SaveChangesAsync(); db.ChangeTracker.Clear();
            await action(new(db, connection, group.Id, other.Id, admin, member, second, outsider, inactive));
        }
        finally { await using var cleanup = Context(connection); await cleanup.Database.EnsureDeletedAsync(); }
    }

    public sealed class ActorAccessor : ICurrentAccountAccessor { public CurrentAccount? Current { get; set; } }
    private sealed class Factory(string connection) : WebApplicationFactory<Program>
    {
        public ActorAccessor Actor { get; } = new();
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing"); builder.UseSetting("ConnectionStrings:FamilySafety", connection);
            builder.UseSetting("DevelopmentIdentity:Enabled", "false"); builder.UseSetting("DevelopmentFixtures:Enabled", "false");
            builder.UseSetting("DevelopmentInvitationOutbox:Enabled", "false");
            builder.ConfigureTestServices(services => { services.RemoveAll<ICurrentAccountAccessor>(); services.AddSingleton<ICurrentAccountAccessor>(Actor); });
        }
    }
}
