using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FamilySafety.Application.Sharing;
using FamilySafety.Application.Profiles;
using FamilySafety.Domain.Sharing;
using FamilySafety.Domain.Profiles;
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

public sealed class SharingSessionTests(ITestOutputHelper output)
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    public sealed class TestClock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => Now;
    }
    private static CreateSharingSessionRequest Request(World w, params Guid[] recipients) => new(w.Family, recipients, null);
    private static UpdateSharingLocationRequest Point(World w, int version, double latitude = 42) => new(latitude, -72, 12, w.Clock.Now, version);

    [Fact]
    public void DomainRejectsUnboundedAndMissingSessionIdentity()
    {
        var now = DateTimeOffset.UtcNow;
        SharingSession Make(DateTimeOffset end) => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0, now, end);
        Assert.Throws<ArgumentException>(() => Make(now));
        Assert.Throws<ArgumentException>(() => Make(now.AddHours(25)));
        Assert.Equal(now.AddHours(24), Make(now.AddHours(24)).EndsAtUtc);
        Assert.Throws<ArgumentException>(() => new SharingSession(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0, now, now.AddHours(1)));
    }

    [Fact]
    public async Task SqlApiSelfInitiatedAudienceValidationAndNondisclosure()
    {
        await WithDatabase(async w => {
            using var factory = new Factory(w.Connection, w.Clock); using var client = factory.CreateClient();
            factory.Actor.Current = new(w.Author.Account);
            var created = await client.PostAsJsonAsync("/api/sharing-sessions", new {
                familyGroupId = w.Family, recipientProfileIds = new[] { w.Recipient.Profile },
                authorProfileId = w.Admin.Profile, accountId = w.Admin.Account,
            });
            Assert.Equal(HttpStatusCode.OK, created.StatusCode);
            Assert.Equal("no-store", created.Headers.CacheControl!.ToString());
            var view = (await created.Content.ReadFromJsonAsync<SharingSessionView>())!;
            Assert.Equal(w.Author.Profile, view.AuthorProfileId); Assert.Null(view.Location);
            Assert.Equal(w.Clock.Now.AddHours(1), view.EndsAtUtc); Assert.Single(view.Recipients!);
            var id = view.SharingSessionId;
            factory.Actor.Current = new(w.Recipient.Account);
            var shared = (await client.GetFromJsonAsync<SharingSessionView>($"/api/sharing-sessions/{id}"))!;
            Assert.Null(shared.Recipients); Assert.Null(shared.Location);
            Assert.Single((await client.GetFromJsonAsync<SharingSessionView[]>("/api/sharing-sessions/received"))!);
            Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync($"/api/sharing-sessions/{id}/location", Point(w, view.Version))).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync($"/api/sharing-sessions/{id}/stop", new StopSharingSessionRequest(view.Version))).StatusCode);
            foreach (var actor in new[] { w.Admin, w.Other, w.Inactive }) {
                factory.Actor.Current = new(actor.Account);
                var denied = await client.GetAsync($"/api/sharing-sessions/{id}?selectedFamily={w.Family}");
                var missing = await client.GetAsync($"/api/sharing-sessions/{Guid.NewGuid()}");
                Assert.Equal(HttpStatusCode.NotFound, denied.StatusCode); Assert.Equal(missing.StatusCode, denied.StatusCode);
                Assert.Equal("", await denied.Content.ReadAsStringAsync());
                Assert.Equal(await missing.Content.ReadAsStringAsync(), await denied.Content.ReadAsStringAsync());
                Assert.Empty((await client.GetFromJsonAsync<SharingSessionView[]>("/api/sharing-sessions/received"))!);
                Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync($"/api/sharing-sessions/{id}/location", Point(w, 1))).StatusCode);
            }
            factory.Actor.Current = new(w.Author.Account);
            foreach (var bad in new[] { w.Other.Profile, w.Inactive.Profile, Guid.NewGuid(), w.Author.Profile })
                Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync("/api/sharing-sessions", Request(w, bad))).StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/sharing-sessions", Request(w))).StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/sharing-sessions", new { familyGroupId = w.Family })).StatusCode);
            foreach (var end in new[] { w.Clock.Now, w.Clock.Now.AddHours(25) })
                Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/sharing-sessions", new CreateSharingSessionRequest(w.Family, [w.Recipient.Profile], end))).StatusCode);
            factory.Actor.Current = new(w.Inactive.Account);
            Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync("/api/sharing-sessions", Request(w, w.Recipient.Profile))).StatusCode);
            factory.Actor.Current = null;
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/sharing-sessions/mine")).StatusCode);
            Assert.Single(await w.Db.SharingSessions.ToListAsync()); Assert.Empty(await w.Db.SharingCurrentLocations.ToListAsync());
        });
    }

    [Fact]
    public async Task SqlLatestPointOnlyValidationMinuteCadenceAndStopConcurrency()
    {
        await WithDatabase(async w => {
            var row = (await w.Repository.CreateAsync(w.Author.Account, Request(w, w.Recipient.Profile), Ct))!;
            foreach (var bad in new[] {
                Point(w, row.Version, 91), Point(w, row.Version) with { Longitude = -181 },
                Point(w, row.Version) with { Accuracy = -1 }, Point(w, row.Version) with { Latitude = null },
                Point(w, row.Version) with { CapturedAtUtc = null }, Point(w, row.Version) with { CapturedAtUtc = w.Clock.Now.AddMinutes(2) },
                Point(w, row.Version) with { CapturedAtUtc = w.Clock.Now.AddMinutes(-6) },
            }) await Assert.ThrowsAsync<SharingSessionValidationException>(() => w.Repository.UpdateLocationAsync(w.Author.Account, row.SharingSessionId, bad, Ct));
            var first = (await w.Repository.UpdateLocationAsync(w.Author.Account, row.SharingSessionId, Point(w, row.Version), Ct))!;
            Assert.Equal(w.Clock.Now, first.Location!.CapturedAtUtc); Assert.Equal(w.Clock.Now, first.Location.ReceivedAtUtc);
            Assert.Null(await w.Repository.UpdateLocationAsync(w.Recipient.Account, row.SharingSessionId, Point(w, first.Version), Ct));
            await Assert.ThrowsAsync<SharingSessionConflictException>(() => w.Repository.UpdateLocationAsync(w.Author.Account, row.SharingSessionId, Point(w, first.Version), Ct));
            w.Clock.Now = w.Clock.Now.AddMinutes(1);
            await Assert.ThrowsAsync<SharingSessionConflictException>(() => w.Repository.UpdateLocationAsync(w.Author.Account, row.SharingSessionId, Point(w, row.Version), Ct));
            var latest = (await w.Repository.UpdateLocationAsync(w.Author.Account, row.SharingSessionId, Point(w, first.Version, 43), Ct))!;
            Assert.Equal(43, latest.Location!.Latitude);
            var recipient = (await w.Repository.GetAsync(w.Recipient.Account, row.SharingSessionId, Ct))!;
            Assert.Equal(latest.Location, recipient.Location); Assert.Null(recipient.Recipients);
            Assert.Single(await w.Db.SharingCurrentLocations.ToListAsync());
            await Assert.ThrowsAsync<SharingSessionConflictException>(() => w.Repository.StopAsync(w.Author.Account, row.SharingSessionId, first.Version, Ct));
            Assert.True(await w.Repository.StopAsync(w.Author.Account, row.SharingSessionId, latest.Version, Ct));
            Assert.Null(await w.Repository.GetAsync(w.Recipient.Account, row.SharingSessionId, Ct));
            Assert.Null(await w.Repository.GetAsync(w.Author.Account, row.SharingSessionId, Ct));
            Assert.Null(await w.Repository.UpdateLocationAsync(w.Author.Account, row.SharingSessionId, Point(w, latest.Version), Ct));
            Assert.Empty(await w.Db.SharingCurrentLocations.ToListAsync());
        });
    }

    [Fact]
    public async Task SqlExpirationDeniesBeforeCleanupAndRejectsFurtherUpdates()
    {
        await WithDatabase(async w => {
            var row = (await w.Repository.CreateAsync(w.Author.Account, new(w.Family, [w.Recipient.Profile], w.Clock.Now.AddHours(24)), Ct))!;
            await w.Repository.UpdateLocationAsync(w.Author.Account, row.SharingSessionId, Point(w, row.Version), Ct);
            w.Clock.Now = row.EndsAtUtc;
            Assert.Single(await w.Db.SharingCurrentLocations.ToListAsync()); // Cleanup has not run.
            Assert.Null(await w.Repository.GetAsync(w.Recipient.Account, row.SharingSessionId, Ct));
            Assert.Empty(await w.Repository.ListAsync(w.Recipient.Account, true, Ct));
            Assert.Null(await w.Repository.UpdateLocationAsync(w.Author.Account, row.SharingSessionId, Point(w, 2), Ct));
            await w.Repository.CleanupAsync(Ct); w.Db.ChangeTracker.Clear();
            Assert.Empty(await w.Db.SharingCurrentLocations.ToListAsync()); Assert.Single(await w.Db.SharingSessions.ToListAsync());
        });
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task SqlMembershipLossIsPermanentEvenOnOperationalRejoin(bool authorLeaves)
    {
        await WithDatabase(async w => {
            var row = (await w.Repository.CreateAsync(w.Author.Account, Request(w, w.Recipient.Profile, w.Admin.Profile), Ct))!;
            await w.Repository.UpdateLocationAsync(w.Author.Account, row.SharingSessionId, Point(w, 1), Ct);
            var actor = authorLeaves ? w.Author : w.Recipient;
            await w.Db.FamilyGroupMemberships.Where(m => m.AdultProfileId == actor.Profile).ExecuteUpdateAsync(u => u.SetProperty(m => m.IsActive, false));
            Assert.Null(await w.Repository.GetAsync(w.Recipient.Account, row.SharingSessionId, Ct));
            Assert.Equal(!authorLeaves, await w.Repository.GetAsync(w.Admin.Account, row.SharingSessionId, Ct) is not null);
            await w.Db.FamilyGroupMemberships.Where(m => m.AdultProfileId == actor.Profile).ExecuteUpdateAsync(u => u.SetProperty(m => m.IsActive, true));
            Assert.Null(await w.Repository.GetAsync(w.Recipient.Account, row.SharingSessionId, Ct));
            w.Db.ChangeTracker.Clear();
            Assert.True((await w.Db.SharingSessionRecipients.SingleAsync(r => r.ProfileId == w.Recipient.Profile)).IsInvalidated);
            if (authorLeaves) {
                Assert.NotNull((await w.Db.SharingSessions.SingleAsync()).StoppedAtUtc);
                Assert.Empty(await w.Db.SharingCurrentLocations.ToListAsync());
                Assert.Null(await w.Repository.UpdateLocationAsync(w.Author.Account, row.SharingSessionId, Point(w, 2), Ct));
            }
        });
    }

    [Fact]
    public async Task SqlReplacementProfileCannotInheritOriginalGrant()
    {
        await WithDatabase(async w => {
            var row = (await w.Repository.CreateAsync(w.Author.Account, Request(w, w.Recipient.Profile), Ct))!;
            var replacement = Guid.NewGuid(); var oldAccount = Guid.NewGuid();
            w.Db.Accounts.Add(new Account(oldAccount, "old-sharing-profile@example.invalid")); await w.Db.SaveChangesAsync();
            await w.Db.AdultProfiles.Where(p => p.Id == w.Recipient.Profile).ExecuteUpdateAsync(u => u.SetProperty(p => p.AccountId, oldAccount));
            w.Db.AdultProfiles.Add(new AdultProfile(replacement, w.Recipient.Account, "Same display name")); await w.Db.SaveChangesAsync();
            await w.Db.FamilyGroupMemberships.Where(m => m.AccountId == w.Recipient.Account).ExecuteUpdateAsync(u => u.SetProperty(m => m.AdultProfileId, replacement));
            Assert.Null(await w.Repository.GetAsync(w.Recipient.Account, row.SharingSessionId, Ct));
            Assert.Empty(await w.Repository.ListAsync(w.Recipient.Account, true, Ct));
        });
    }

    [Fact]
    public async Task SqlMigrationRoundTripPreservesFamilyAndCheckInSchema()
    {
        await WithDatabase(async w => {
            await w.Db.GetService<IMigrator>().MigrateAsync("20261005025236_StandaloneCheckIns");
            await w.Db.Database.MigrateAsync(); await w.Db.Database.MigrateAsync();
            Assert.Equal(5, await w.Db.Accounts.CountAsync());
            Assert.NotNull(await w.Repository.CreateAsync(w.Author.Account, Request(w, w.Recipient.Profile), Ct));
        });
    }

    [Fact]
    public async Task SqlConcurrentStopAndPointCannotReviveSharing()
    {
        await WithDatabase(async w => {
            var row = (await w.Repository.CreateAsync(w.Author.Account, Request(w, w.Recipient.Profile), Ct))!;
            await using var writerDb = Context(w.Connection);
            await using var stopperDb = Context(w.Connection);
            var writer = new EfSharingSessionRepository(writerDb, w.Clock);
            var stopper = new EfSharingSessionRepository(stopperDb, w.Clock);
            var update = writer.UpdateLocationAsync(w.Author.Account, row.SharingSessionId, Point(w, 1), Ct);
            var stop = AttemptStop();
            await Task.WhenAll(update, stop);
            // If replacement won, refresh the optimistic version and deliberately stop the still-active session.
            var active = await w.Repository.GetAsync(w.Author.Account, row.SharingSessionId, Ct);
            if (active is not null) Assert.True(await w.Repository.StopAsync(w.Author.Account, row.SharingSessionId, active.Version, Ct));
            Assert.Null(await w.Repository.GetAsync(w.Recipient.Account, row.SharingSessionId, Ct));
            Assert.Empty(await w.Db.SharingCurrentLocations.AsNoTracking().ToListAsync());
            Assert.Null(await writer.UpdateLocationAsync(w.Author.Account, row.SharingSessionId, Point(w, 1), Ct));
            async Task AttemptStop() {
                try { await stopper.StopAsync(w.Author.Account, row.SharingSessionId, 1, Ct); }
                catch (SharingSessionConflictException) { /* Authorized optimistic conflict, refreshed above. */ }
            }
        });
    }

    private sealed record Adult(Guid Account, Guid Profile);
    private sealed record World(FamilySafetyDbContext Db, string Connection, Guid Family, Adult Author, Adult Recipient, Adult Admin, Adult Other, Adult Inactive)
    { public TestClock Clock { get; } = new(); public EfSharingSessionRepository Repository => new(Db, Clock); }
    private static FamilySafetyDbContext Context(string connection) => new(new DbContextOptionsBuilder<FamilySafetyDbContext>().UseSqlServer(connection).Options);
    private async Task WithDatabase(Func<World, Task> action)
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        var connection = $"Server=localhost;Database=FamilySafety_WI095006_{Guid.NewGuid():N};Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True";
        await using var db = Context(connection);
        output.WriteLine("SQL gate enabled: isolated WI095006 migrations/API/privacy/lifecycle assertions execute.");
        try
        {
            await db.Database.MigrateAsync();
            Adult Create(string name) {
                var a = new Adult(Guid.NewGuid(), Guid.NewGuid());
                db.AddRange(new Account(a.Account, $"{a.Account:N}@example.invalid"), new AdultProfile(a.Profile, a.Account, name)); return a;
            }
            var author = Create("Author"); var recipient = Create("Recipient"); var admin = Create("Admin"); var other = Create("Other family"); var inactive = Create("Inactive");
            var family = new FamilyGroup(Guid.NewGuid(), "Sharing family", 100); var otherGroup = new FamilyGroup(Guid.NewGuid(), "Other family", 100); db.AddRange(family, otherGroup);
            foreach (var a in new[] { author, recipient, admin, other, inactive }) {
                var m = new FamilyGroupMembership(Guid.NewGuid(), a.Account, a.Profile, a == other ? otherGroup.Id : family.Id, a != inactive);
                db.Add(m); if (a == admin) db.Add(new FamilyAdministratorAssignment(Guid.NewGuid(), m.Id));
            }
            await db.SaveChangesAsync(); db.ChangeTracker.Clear();
            await action(new(db, connection, family.Id, author, recipient, admin, other, inactive));
        }
        finally { await using var cleanup = Context(connection); await cleanup.Database.EnsureDeletedAsync(); }
    }
    public sealed class ActorAccessor : ICurrentAccountAccessor { public CurrentAccount? Current { get; set; } }
    public sealed class Factory(string connection, TestClock clock) : WebApplicationFactory<Program>
    {
        public ActorAccessor Actor { get; } = new();
        protected override void ConfigureWebHost(IWebHostBuilder builder) {
            builder.UseEnvironment("Testing"); builder.UseSetting("ConnectionStrings:FamilySafety", connection);
            builder.UseSetting("DevelopmentIdentity:Enabled", "false"); builder.UseSetting("DevelopmentFixtures:Enabled", "false");
            builder.UseSetting("DevelopmentInvitationOutbox:Enabled", "false");
            builder.ConfigureTestServices(services => { services.RemoveAll<ICurrentAccountAccessor>(); services.AddSingleton<ICurrentAccountAccessor>(Actor); services.RemoveAll<TimeProvider>(); services.AddSingleton<TimeProvider>(clock); });
        }
    }
}
