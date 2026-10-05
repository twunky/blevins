using System.Net;
using System.Net.Http.Json;
using FamilySafety.Application.Profiles;
using FamilySafety.Application.TrustedContacts;
using FamilySafety.Application.Concerns;
using FamilySafety.Application.Incidents;
using FamilySafety.Domain.Profiles;
using FamilySafety.Domain.Events;
using FamilySafety.Domain.Plans;
using FamilySafety.Domain.Trips;
using FamilySafety.Domain.CheckIns;
using FamilySafety.Domain.Sharing;
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

using FamilySafety.Application.SafetyCircles;
public sealed class SafetyCircleTests(ITestOutputHelper output)
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private static string Url(Guid family) => $"/api/family-groups/{family}/safety-circles";
    private static EfSafetyCircleRepository Repo(World w) => new(w.Db, TimeProvider.System);

    [Fact]
    public async Task OwnerCrudIsPrivateNondisclosingAndUsesScopedNamesAndVersions()
    {
        await WithDatabase(async w => {
            using var factory = new Factory(w.Connection); using var client = factory.CreateClient();
            factory.Actor.Current = new(w.Owner.Account); var url = Url(w.Group);
            var create = await client.PostAsJsonAsync(url, new { name = " Close family ", ownerProfileId = w.Admin.Profile });
            Assert.Equal(HttpStatusCode.OK, create.StatusCode); Assert.True(create.Headers.CacheControl!.NoStore);
            var circle = (await create.Content.ReadFromJsonAsync<SafetyCircleView>())!;
            Assert.Equal("Close family", circle.Name); Assert.Empty(circle.Members);
            Assert.Equal(w.Owner.Profile, (await w.Db.SafetyCircles.AsNoTracking().SingleAsync()).OwnerProfileId);
            Assert.Single((await client.GetFromJsonAsync<SafetyCircleView[]>(url))!);
            Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync(url, new { name = "close FAMILY" })).StatusCode);
            foreach (var name in new[] { "", "Name\nText", new string('x', 101) })
                Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync(url, new { name })).StatusCode);
            foreach (var actor in new[] { w.Admin, w.Member, w.Outsider }) {
                factory.Actor.Current = new(actor.Account);
                var list = await client.GetAsync(url + $"?ownerProfileId={w.Owner.Profile}&selectedFamily={w.Group}");
                Assert.Equal(actor == w.Outsider ? HttpStatusCode.NotFound : HttpStatusCode.OK, list.StatusCode);
                if (actor != w.Outsider) Assert.Equal("[]", await list.Content.ReadAsStringAsync());
                foreach (var response in new[] {
                    await client.GetAsync(url + $"/{circle.SafetyCircleId}"),
                    await client.PutAsJsonAsync(url + $"/{circle.SafetyCircleId}", new { version = Guid.Empty, name = "" }),
                    await client.PostAsJsonAsync(url + $"/{circle.SafetyCircleId}/members", new { version = circle.Version, memberProfileId = actor.Profile }),
                    await client.DeleteAsync(url + $"/{circle.SafetyCircleId}/members/{w.Member.Profile}?version={circle.Version}"),
                    await client.DeleteAsync(url + $"/{circle.SafetyCircleId}?version={circle.Version}") }) {
                    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); Assert.Equal("", await response.Content.ReadAsStringAsync());
                    Assert.True(response.Headers.CacheControl!.NoStore);
                }
            }
            factory.Actor.Current = new(w.Admin.Account);
            Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync(url, new { name = "Close family" })).StatusCode);
            factory.Actor.Current = new(w.Owner.Account);
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(Url(Guid.NewGuid()) + $"/{circle.SafetyCircleId}?selectedFamily={w.Group}")).StatusCode);
            var changed = await client.PutAsJsonAsync(url + $"/{circle.SafetyCircleId}", new { version = circle.Version, name = "Renamed" });
            Assert.Equal(HttpStatusCode.OK, changed.StatusCode); var renamed = (await changed.Content.ReadFromJsonAsync<SafetyCircleView>())!;
            Assert.NotEqual(circle.Version, renamed.Version);
            Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync(url + $"/{circle.SafetyCircleId}?version={circle.Version}")).StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync(url + $"/{circle.SafetyCircleId}?version={renamed.Version}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(url + $"/{circle.SafetyCircleId}")).StatusCode);
            factory.Actor.Current = null; Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(url)).StatusCode);
        });
    }

    [Fact]
    public async Task EligibilityAndMemberLossRemoveOnlyExactMembershipWithoutRejoinRestoration()
    {
        await WithDatabase(async w => {
            var repo = Repo(w); var circle = (await repo.CreateAsync(w.Owner.Account, w.Group, "Members", Ct))!;
            var other = new FamilyGroup(Guid.NewGuid(), "Other", 100); w.Db.Add(other);
            w.Db.Add(new FamilyGroupMembership(Guid.NewGuid(), w.Outsider.Account, w.Outsider.Profile, other.Id, true));
            var contact = new FamilySafety.Domain.TrustedContacts.TrustedContact(Guid.NewGuid(), w.Owner.Profile,
                "Manual", null, null, "manual@example.invalid", "MANUAL@EXAMPLE.INVALID", null);
            w.Db.Add(contact); await w.Db.SaveChangesAsync();
            foreach (var id in new[] { w.Outsider.Profile, contact.Id, Guid.NewGuid() }) // Cross-Family, contact, no adult profile (child unsupported).
                Assert.Null(await repo.AddAsync(w.Owner.Account, w.Group, circle.SafetyCircleId, new(circle.Version, id), Ct));
            await w.Db.FamilyGroupMemberships.Where(m => m.AdultProfileId == w.Member.Profile).ExecuteUpdateAsync(u => u.SetProperty(m => m.IsActive, false));
            Assert.Null(await repo.AddAsync(w.Owner.Account, w.Group, circle.SafetyCircleId, new(circle.Version, w.Member.Profile), Ct));
            await w.Db.FamilyGroupMemberships.Where(m => m.AdultProfileId == w.Member.Profile).ExecuteUpdateAsync(u => u.SetProperty(m => m.IsActive, true));
            circle = (await repo.AddAsync(w.Owner.Account, w.Group, circle.SafetyCircleId, new(circle.Version, w.Member.Profile), Ct))!;
            Assert.Single(circle.Members); var priorVersion = circle.Version;
            await w.Db.FamilyGroupMemberships.Where(m => m.AdultProfileId == w.Member.Profile).ExecuteUpdateAsync(u => u.SetProperty(m => m.IsActive, false));
            Assert.False(await w.Db.SafetyCircleMembers.AnyAsync()); Assert.Single(await w.Db.SafetyCircles.AsNoTracking().ToListAsync());
            await w.Db.FamilyGroupMemberships.Where(m => m.AdultProfileId == w.Member.Profile).ExecuteUpdateAsync(u => u.SetProperty(m => m.IsActive, true));
            circle = (await repo.ReadAsync(w.Owner.Account, w.Group, circle.SafetyCircleId, Ct))!;
            Assert.Empty(circle.Members); Assert.NotEqual(priorVersion, circle.Version);
            circle = (await repo.AddAsync(w.Owner.Account, w.Group, circle.SafetyCircleId, new(circle.Version, w.Member.Profile), Ct))!;
            Assert.Single(circle.Members);
            circle = (await repo.RemoveAsync(w.Owner.Account, w.Group, circle.SafetyCircleId, w.Member.Profile, circle.Version, Ct))!;
            Assert.Empty(circle.Members);
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PresetsHaveZeroAccessAndLaterChangesOrDeletionNeverModifyTargetGrants(bool ownerLoss)
    {
        await WithDatabase(async w => {
            var repo = Repo(w); var circle = (await repo.CreateAsync(w.Owner.Account, w.Group, "Preset", Ct))!;
            circle = (await repo.AddAsync(w.Owner.Account, w.Group, circle.SafetyCircleId, new(circle.Version, w.Member.Profile), Ct))!;
            var checkIns = new EfCheckInRepository(w.Db, TimeProvider.System);
            var sessions = new EfSharingSessionRepository(w.Db, TimeProvider.System);
            // Preset alone grants nothing: private existing records have no explicit recipients.
            var privateCheckIn = (await checkIns.CreateAsync(w.Owner.Account, new(w.Group, CheckInStatus.ImOkay, "Private", []), Ct))!;
            Assert.Null(await checkIns.GetAsync(w.Member.Account, privateCheckIn.CheckInId, Ct));
            var privateSession = (await sessions.CreateAsync(w.Owner.Account, new(w.Group, [w.Admin.Profile], null), Ct))!;
            Assert.Null(await sessions.GetAsync(w.Member.Account, privateSession.SharingSessionId, Ct));
            var date = new DateOnly(2026, 10, 5);
            var evt = new FamilyEvent(Guid.NewGuid(), w.Group, w.Owner.Profile, "Private event", "Private", date, new(12, 0), "Private");
            var plan = new FamilyPlan(Guid.NewGuid(), w.Group, w.Owner.Profile, "Private plan", date, new(12, 0), null, null, null, null, PlanAudienceType.SelectedPeople);
            var trip = new FamilyTrip(Guid.NewGuid(), w.Group, w.Owner.Profile, TripAudienceType.SelectedPeople, "Private trip", "Private", date, date, null, null, null, null, null, null);
            w.Db.AddRange(evt, plan, trip); await w.Db.SaveChangesAsync();
            var concern = (await new EfConcernRepository(w.Db, TimeProvider.System).CreateAsync(w.Owner.Account, w.Group,
                new(w.Member.Profile, "Private report", []), Ct))!;
            var incident = (await new EfIncidentRepository(w.Db, TimeProvider.System, new EfConcernAccessQuery(w.Db, TimeProvider.System))
                .CreateAsync(w.Owner.Account, w.Group, new(w.Member.Profile, "Private incident", IncidentInformationCategory.Reported,
                    "Direct conversation", false, []), Ct))!;
            using (var factory = new Factory(w.Connection)) using (var client = factory.CreateClient()) {
                factory.Actor.Current = new(w.Member.Account);
                foreach (var target in new[] { $"/api/events/{evt.Id}", $"/api/plans/{plan.Id}", $"/api/trips/{trip.Id}",
                    $"/api/concerns/{concern.ConcernId}", $"/api/incidents/{incident.IncidentId}" })
                    Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(target)).StatusCode);
            }
            // Final selection may adjust the preset by adding a manually chosen eligible profile.
            var selected = circle.Members.Select(m => m.ProfileId).Append(w.Admin.Profile).ToArray();
            var check = (await checkIns.CreateAsync(w.Owner.Account, new(w.Group, CheckInStatus.Arrived, "Shared", selected), Ct))!;
            var session = (await sessions.CreateAsync(w.Owner.Account, new(w.Group, selected, null), Ct))!;
            var savedCheck = await w.Db.CheckInRecipients.AsNoTracking().Where(r => r.CheckInId == check.CheckInId).ToListAsync();
            var savedSession = await w.Db.SharingSessionRecipients.AsNoTracking().Where(r => r.SharingSessionId == session.SharingSessionId).ToListAsync();
            circle = (await repo.RemoveAsync(w.Owner.Account, w.Group, circle.SafetyCircleId, w.Member.Profile, circle.Version, Ct))!;
            circle = (await repo.AddAsync(w.Owner.Account, w.Group, circle.SafetyCircleId, new(circle.Version, w.Admin.Profile), Ct))!;
            Assert.NotNull(await checkIns.GetAsync(w.Member.Account, check.CheckInId, Ct)); Assert.NotNull(await sessions.GetAsync(w.Member.Account, session.SharingSessionId, Ct));
            if (ownerLoss) {
                // Separate circle owner loses eligibility; target items authored by another eligible adult keep valid grants.
                var otherCircle = (await repo.CreateAsync(w.Admin.Account, w.Group, "Other owner's preset", Ct))!;
                otherCircle = (await repo.AddAsync(w.Admin.Account, w.Group, otherCircle.SafetyCircleId, new(otherCircle.Version, w.Member.Profile), Ct))!;
                var otherCheck = (await checkIns.CreateAsync(w.Owner.Account, new(w.Group, CheckInStatus.Arrived, null, otherCircle.Members.Select(m => m.ProfileId).ToArray()), Ct))!;
                var otherSession = (await sessions.CreateAsync(w.Owner.Account, new(w.Group, otherCircle.Members.Select(m => m.ProfileId).ToArray(), null), Ct))!;
                await w.Db.FamilyGroupMemberships.Where(m => m.AdultProfileId == w.Admin.Profile).ExecuteUpdateAsync(u => u.SetProperty(m => m.IsActive, false));
                Assert.False(await w.Db.SafetyCircles.AnyAsync(c => c.Id == otherCircle.SafetyCircleId));
                Assert.False(await w.Db.SafetyCircleMembers.AnyAsync(m => m.SafetyCircleId == otherCircle.SafetyCircleId));
                Assert.Null(await repo.ReadAsync(w.Admin.Account, w.Group, otherCircle.SafetyCircleId, Ct));
                Assert.NotNull(await checkIns.GetAsync(w.Member.Account, otherCheck.CheckInId, Ct));
                Assert.NotNull(await sessions.GetAsync(w.Member.Account, otherSession.SharingSessionId, Ct));
                await w.Db.FamilyGroupMemberships.Where(m => m.AdultProfileId == w.Admin.Profile).ExecuteUpdateAsync(u => u.SetProperty(m => m.IsActive, true));
                Assert.Empty((await repo.ListAsync(w.Admin.Account, w.Group, Ct))!);
                Assert.Null(await repo.ReadAsync(w.Owner.Account, w.Group, otherCircle.SafetyCircleId, Ct));
                Assert.NotNull(await repo.CreateAsync(w.Admin.Account, w.Group, "Other owner's preset", Ct));
                // Target grants to the departed administrator follow their own permanent invalidation.
                Assert.Null(await checkIns.GetAsync(w.Admin.Account, check.CheckInId, Ct)); Assert.Null(await sessions.GetAsync(w.Admin.Account, session.SharingSessionId, Ct));
            }
            else {
                Assert.True(await repo.DeleteAsync(w.Owner.Account, w.Group, circle.SafetyCircleId, circle.Version, Ct));
                Assert.False(await w.Db.SafetyCircleMembers.AnyAsync(m => m.SafetyCircleId == circle.SafetyCircleId));
                Assert.NotNull(await checkIns.GetAsync(w.Member.Account, check.CheckInId, Ct)); Assert.NotNull(await sessions.GetAsync(w.Member.Account, session.SharingSessionId, Ct));
                Assert.Equal(savedCheck.Select(r => r.ProfileId), (await w.Db.CheckInRecipients.AsNoTracking().Where(r => r.CheckInId == check.CheckInId).ToListAsync()).Select(r => r.ProfileId));
                Assert.Equal(savedSession.Select(r => r.ProfileId), (await w.Db.SharingSessionRecipients.AsNoTracking().Where(r => r.SharingSessionId == session.SharingSessionId).ToListAsync()).Select(r => r.ProfileId));
            }
            // Own-author loss may independently end product access; circle deletion never suppresses those existing rules.
            await w.Db.FamilyGroupMemberships.Where(m => m.AdultProfileId == w.Owner.Profile).ExecuteUpdateAsync(u => u.SetProperty(m => m.IsActive, false));
            Assert.False(await w.Db.SafetyCircles.AnyAsync(c => c.OwnerProfileId == w.Owner.Profile));
            Assert.Null(await checkIns.GetAsync(w.Member.Account, check.CheckInId, Ct)); Assert.Null(await sessions.GetAsync(w.Member.Account, session.SharingSessionId, Ct));
            Assert.NotNull(await checkIns.GetAsync(w.Owner.Account, check.CheckInId, Ct));
        });
    }

    [Fact]
    public async Task MigrationRoundTripAndConcurrentMutationsPreservePrivacyAndAcceptedState()
    {
        await WithDatabase(async w => {
            await w.Db.GetService<IMigrator>().MigrateAsync("20261005040757_ManualTrustedContacts");
            await w.Db.Database.MigrateAsync(); await w.Db.Database.MigrateAsync(); Assert.Equal(5, await w.Db.Accounts.CountAsync());
            var row = (await Repo(w).CreateAsync(w.Owner.Account, w.Group, "Concurrent", Ct))!;
            async Task<bool> Rename(string name) {
                await using var db = Context(w.Connection);
                try { await new EfSafetyCircleRepository(db, TimeProvider.System).RenameAsync(w.Owner.Account, w.Group, row.SafetyCircleId, new(row.Version, name), Ct); return true; }
                catch (SafetyCircleConflictException) { return false; }
            }
            Assert.Single(await Task.WhenAll(Rename("One"), Rename("Two")), x => x);
            await Assert.ThrowsAsync<SafetyCircleConflictException>(() => Repo(w).DeleteAsync(w.Owner.Account, w.Group, row.SafetyCircleId, row.Version, Ct));
        });
    }

    private sealed record Adult(Guid Account, Guid Profile);
    private sealed record World(FamilySafetyDbContext Db, string Connection, Guid Group, Adult Owner, Adult Admin, Adult Member, Adult Outsider, Guid NoProfile);
    private static FamilySafetyDbContext Context(string connection) => new(new DbContextOptionsBuilder<FamilySafetyDbContext>().UseSqlServer(connection).Options);
    private async Task WithDatabase(Func<World, Task> action)
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        var connection = $"Server=localhost;Database=FamilySafety_WI095008_{Guid.NewGuid():N};Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True";
        await using var db = Context(connection);
        output.WriteLine("SQL gate enabled; disposable WI095008 database; migrations, circle privacy/provenance/target grants and permanent-delete assertions execute.");
        try
        {
            await db.Database.MigrateAsync();
            Adult Create(string name)
            {
                var adult = new Adult(Guid.NewGuid(), Guid.NewGuid());
                db.AddRange(new Account(adult.Account, $"{adult.Account:N}@example.invalid"), new AdultProfile(adult.Profile, adult.Account, name));
                return adult;
            }
            var owner = Create("Owner"); var admin = Create("Administrator"); var member = Create("Member"); var outsider = Create("No Family");
            var noProfile = Guid.NewGuid(); db.Add(new Account(noProfile, "no-profile@example.invalid"));
            var group = new FamilyGroup(Guid.NewGuid(), "Family", 100); db.Add(group);
            foreach (var adult in new[] { owner, admin, member })
            {
                var membership = new FamilyGroupMembership(Guid.NewGuid(), adult.Account, adult.Profile, group.Id, true);
                db.Add(membership); if (adult == admin) db.Add(new FamilyAdministratorAssignment(Guid.NewGuid(), membership.Id));
            }
            await db.SaveChangesAsync(); db.ChangeTracker.Clear();
            await action(new(db, connection, group.Id, owner, admin, member, outsider, noProfile));
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
