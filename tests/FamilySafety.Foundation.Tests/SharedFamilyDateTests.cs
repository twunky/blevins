using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FamilySafety.Application.FamilyDates;
using FamilySafety.Application.Profiles;
using FamilySafety.Domain.FamilyDates;
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

public sealed class SharedFamilyDateTests(ITestOutputHelper output)
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    [Fact]
    public void RequiredContentAndFirstUnavailabilityAreBounded()
    {
        var now = DateTimeOffset.UtcNow;
        SharedFamilyDate Make(string title, string description, DateOnly date) =>
            new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0, title, description, date, now);
        Assert.Throws<ArgumentException>(() => Make(" ", "Detail", new(2026, 10, 4)));
        Assert.Throws<ArgumentException>(() => Make("Title", " ", new(2026, 10, 4)));
        Assert.Throws<ArgumentException>(() => Make("Title", "Detail", default));
        var row = Make(" Title ", " Description ", new(2028, 2, 29));
        Assert.Equal("Title", row.Title); Assert.Equal(new DateOnly(2028, 2, 29), row.Date);
        row.MakeUnavailable(now); row.MakeUnavailable(now.AddDays(5));
        Assert.Equal(now, row.UnavailableAtUtc); Assert.Equal(now.AddDays(30), row.RetainUntilUtc);
        Assert.Throws<InvalidOperationException>(() => row.Edit("Changed", "Content", new(2026, 10, 5), now));
    }

    [Fact]
    public async Task SqlApiLifecycleUsesCurrentIdentityDateOnlyAndCreatorParticipantAuthority()
    {
        await WithDatabase(async w =>
        {
            using var factory = new Factory(w.Connection, w.Clock);
            using var client = factory.CreateClient();
            var path = $"/api/family-groups/{w.Group}/dates";
            factory.Actor.Current = new(w.Creator.Account);
            var response = await client.PostAsJsonAsync(path, new {
                title = " Reunion ", description = " A specific family date ", date = "2028-02-29",
                associatedProfileIds = new[] { w.Participant.Profile }, accountId = w.Admin.Account, creatorProfileId = w.Admin.Profile,
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var created = (await response.Content.ReadFromJsonAsync<SharedFamilyDateView>())!;
            Assert.Equal(w.Creator.Profile, created.CreatorProfileId); Assert.Equal(new DateOnly(2028, 2, 29), created.Date);
            Assert.Contains("\"date\":\"2028-02-29\"", await response.Content.ReadAsStringAsync());
            Assert.Equal(new DateOnly(2028, 2, 29), (await w.Db.SharedFamilyDates.AsNoTracking().SingleAsync()).Date);

            factory.Actor.Current = new(w.Participant.Account);
            var view = (await client.GetFromJsonAsync<SharedFamilyDatesView>(path))!.Dates.Single();
            Assert.True(view.CanEdit); Assert.False(view.CanManageAssociations); Assert.False(view.CanDelete);
            Assert.Equal(w.Participant.Profile, view.AssociatedPeople.Single().ProfileId);
            Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync($"{path}/{view.DateId}",
                new EditSharedFamilyDateRequest(view.Version, "Forged", "Denied", view.Date, []))).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"{path}/{view.DateId}?version={view.Version}")).StatusCode);
            var editedResponse = await client.PutAsJsonAsync($"{path}/{view.DateId}",
                new EditSharedFamilyDateRequest(view.Version, "Edited", "Participant content", new(2028, 3, 1), null));
            Assert.Equal(HttpStatusCode.OK, editedResponse.StatusCode);
            var edited = (await editedResponse.Content.ReadFromJsonAsync<SharedFamilyDateView>())!;

            foreach (var actor in new[] { w.Admin, w.Unassociated, w.OtherFamily })
            {
                factory.Actor.Current = new(actor.Account);
                var list = await client.GetAsync(path);
                if (actor == w.OtherFamily) Assert.Equal(HttpStatusCode.NotFound, list.StatusCode);
                else Assert.Empty((await list.Content.ReadFromJsonAsync<SharedFamilyDatesView>())!.Dates);
                Assert.DoesNotContain("Participant content", await list.Content.ReadAsStringAsync());
                var denied = await client.PutAsJsonAsync($"{path}/{edited.DateId}?selectedFamily={w.Group}",
                    new EditSharedFamilyDateRequest(Guid.NewGuid(), "Denied", "Known ID", edited.Date, null));
                Assert.Equal(HttpStatusCode.NotFound, denied.StatusCode); Assert.Equal("", await denied.Content.ReadAsStringAsync());
                Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"{path}/{edited.DateId}?version={edited.Version}")).StatusCode);
            }
            factory.Actor.Current = new(w.Creator.Account);
            Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync($"{path}/{created.DateId}?version={created.Version}")).StatusCode);
            var changed = await client.PutAsJsonAsync($"{path}/{edited.DateId}",
                new EditSharedFamilyDateRequest(edited.Version, edited.Title, edited.Description, edited.Date, [w.Admin.Profile]));
            Assert.Equal(HttpStatusCode.OK, changed.StatusCode);
            var reassociated = (await changed.Content.ReadFromJsonAsync<SharedFamilyDateView>())!;
            Assert.Empty((await w.Repository.ListAsync(w.Participant.Account, w.Group, Ct))!.Dates);
            Assert.Single((await w.Repository.ListAsync(w.Admin.Account, w.Group, Ct))!.Dates);
            Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"{path}/{edited.DateId}?version={reassociated.Version}")).StatusCode);
            Assert.Empty((await client.GetFromJsonAsync<SharedFamilyDatesView>(path))!.Dates);
            var retained = await w.Db.SharedFamilyDates.AsNoTracking().SingleAsync();
            Assert.Equal(w.Clock.Now, retained.UnavailableAtUtc); Assert.Equal(w.Clock.Now.AddDays(30), retained.RetainUntilUtc);
            Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsync($"{path}/{edited.DateId}/restore", null)).StatusCode);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.GetAsync($"{path}/{edited.DateId}")).StatusCode); // No single-item GET contract.
            factory.Actor.Current = null;
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(path)).StatusCode);
        });
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task MembershipLossImmediatelyHidesWholeEntryRemovesAssociationAndNeverRevives(bool creatorLoss)
    {
        await WithDatabase(async w =>
        {
            var row = (await w.Repository.CreateAsync(w.Creator.Account, w.Group, Request(w.Participant.Profile, w.Admin.Profile), Ct))!;
            var profile = creatorLoss ? w.Creator.Profile : w.Participant.Profile;
            var lossAt = w.Clock.Now.AddHours(1);
            // Direct SQL mutation exercises the database boundary, not a client/repository filter.
            await w.Db.FamilyGroupMemberships.Where(m => m.AdultProfileId == profile)
                .ExecuteUpdateAsync(u => u.SetProperty(m => m.IsActive, false).SetProperty(m => m.EndedAt, lossAt));
            Assert.Equal(1, await w.Db.FamilyGroupMemberships.Where(m => m.AdultProfileId == profile).Select(m => m.AccessVersion).SingleAsync());
            var stored = await w.Db.SharedFamilyDates.AsNoTracking().SingleAsync();
            Assert.Equal(lossAt, stored.UnavailableAtUtc); Assert.Equal(lossAt.AddDays(30), stored.RetainUntilUtc);
            if (!creatorLoss) Assert.False(await w.Db.SharedFamilyDateAssociations.AnyAsync(a => a.ProfileId == profile));
            foreach (var actor in new[] { w.Creator, w.Participant, w.Admin })
            {
                var view = await w.Repository.ListAsync(actor.Account, w.Group, Ct);
                Assert.True(view is null || view.Dates.Count == 0);
                Assert.Null(await w.Repository.EditAsync(actor.Account, w.Group, row.DateId,
                    new(row.Version, "Denied", "Loss", row.Date, null), Ct));
            }
            await w.Db.FamilyGroupMemberships.Where(m => m.AdultProfileId == profile).ExecuteUpdateAsync(u => u.SetProperty(m => m.IsActive, true));
            Assert.Empty((await w.Repository.ListAsync(w.Creator.Account, w.Group, Ct))!.Dates);
            // Simulate stale backup/operational restoration: reset row state and restore old grants.
            await w.Db.SharedFamilyDates.ExecuteUpdateAsync(u => u.SetProperty(d => d.UnavailableAtUtc, (DateTimeOffset?)null)
                .SetProperty(d => d.RetainUntilUtc, (DateTimeOffset?)null));
            if (!creatorLoss)
            {
                w.Db.ChangeTracker.Clear();
                var m = await w.Db.FamilyGroupMemberships.AsNoTracking().SingleAsync(m => m.AdultProfileId == profile);
                w.Db.SharedFamilyDateAssociations.Add(new(row.DateId, profile, m.Id, m.AccessVersion - 1));
                await w.Db.SaveChangesAsync();
            }
            Assert.Empty((await w.Repository.ListAsync(w.Admin.Account, w.Group, Ct))!.Dates);
            Assert.Empty((await w.Repository.ListAsync(w.Creator.Account, w.Group, Ct))!.Dates);
            Assert.False(await w.Repository.DeleteAsync(w.Creator.Account, w.Group, row.DateId, row.Version, Ct));
        });
    }

    [Fact]
    public async Task RetentionExpiresAtThirtyDaysAndMembershipLossWithoutReadsStillStartsClock()
    {
        await WithDatabase(async w =>
        {
            var row = (await w.Repository.CreateAsync(w.Creator.Account, w.Group, Request(w.Participant.Profile), Ct))!;
            Assert.True(await w.Repository.DeleteAsync(w.Creator.Account, w.Group, row.DateId, row.Version, Ct));
            w.Clock.Now = w.Clock.Now.AddDays(30).AddTicks(-1);
            await w.Repository.ReconcileAsync(Ct); Assert.Single(await w.Db.SharedFamilyDates.AsNoTracking().ToListAsync());
            w.Clock.Now = w.Clock.Now.AddTicks(1);
            await w.Repository.ReconcileAsync(Ct); Assert.Empty(await w.Db.SharedFamilyDates.ToListAsync());
            Assert.Empty(await w.Db.SharedFamilyDateAssociations.ToListAsync());
            w.Db.ChangeTracker.Clear();
            var second = (await w.Repository.CreateAsync(w.Creator.Account, w.Group, Request(w.Participant.Profile), Ct))!;
            await w.Db.FamilyGroupMemberships.Where(m => m.AdultProfileId == w.Participant.Profile)
                .ExecuteUpdateAsync(u => u.SetProperty(m => m.IsActive, false).SetProperty(m => m.EndedAt, w.Clock.Now));
            w.Clock.Now = w.Clock.Now.AddDays(31);
            await w.Repository.ReconcileAsync(Ct);
            Assert.False(await w.Db.SharedFamilyDates.AnyAsync(d => d.Id == second.DateId));
        });
    }

    [Fact]
    public async Task ConcurrentEditsHaveOneWinnerAndStaleDeleteCannotOverwriteIt()
    {
        await WithDatabase(async w =>
        {
            var row = (await w.Repository.CreateAsync(w.Creator.Account, w.Group, Request(w.Participant.Profile), Ct))!;
            async Task<bool> Edit(string title)
            {
                await using var db = Context(w.Connection);
                try { await new EfSharedFamilyDateRepository(db, w.Clock).EditAsync(w.Participant.Account, w.Group, row.DateId,
                    new(row.Version, title, "Edited", row.Date, null), Ct); return true; }
                catch (SharedFamilyDateConflictException) { return false; }
            }
            Assert.Single(await Task.WhenAll(Edit("One"), Edit("Two")), success => success);
            await Assert.ThrowsAsync<SharedFamilyDateConflictException>(() => w.Repository.DeleteAsync(w.Creator.Account, w.Group, row.DateId, row.Version, Ct));
        });
    }

    [Fact]
    public async Task MigrationRoundTripPreservesExistingDataAndValidationRejectsIneligibleAssociations()
    {
        await WithDatabase(async w =>
        {
            await w.Db.GetService<IMigrator>().MigrateAsync("20261005013414_EventUpdates");
            await w.Db.Database.MigrateAsync(); await w.Db.Database.MigrateAsync();
            Assert.Equal(6, await w.Db.Accounts.CountAsync());
            Assert.Null(await w.Repository.CreateAsync(w.OtherFamily.Account, w.Group, Request(), Ct));
            Assert.Null(await w.Repository.CreateAsync(w.Creator.Account, w.Group, Request(w.OtherFamily.Profile), Ct));
            await Assert.ThrowsAsync<SharedFamilyDateValidationException>(() => w.Repository.CreateAsync(w.Creator.Account, w.Group,
                new("Title", " ", new(2026, 10, 4), []), Ct));
            var row = await w.Repository.CreateAsync(w.Admin.Account, w.Group, Request(), Ct);
            Assert.NotNull(row); Assert.True(row.CanDelete); // Any member creates; administrator is only its own creator.
        });
    }

    private static CreateSharedFamilyDateRequest Request(params Guid[] people) => new("Important date", "Family details", new(2026, 10, 4), people);
    public sealed class Clock : TimeProvider
    { public DateTimeOffset Now { get; set; } = new(2026, 10, 4, 12, 0, 0, TimeSpan.Zero); public override DateTimeOffset GetUtcNow() => Now; }
    private sealed record Adult(Guid Account, Guid Profile);
    private sealed record World(FamilySafetyDbContext Db, string Connection, Guid Group, Clock Clock,
        Adult Creator, Adult Participant, Adult Admin, Adult Unassociated, Adult OtherFamily)
    { public EfSharedFamilyDateRepository Repository => new(Db, Clock); }
    private static FamilySafetyDbContext Context(string connection) => new(new DbContextOptionsBuilder<FamilySafetyDbContext>().UseSqlServer(connection).Options);
    private async Task WithDatabase(Func<World, Task> action)
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        var connection = $"Server=localhost;Database=FamilySafety_WI095004_{Guid.NewGuid():N};Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True";
        await using var db = Context(connection);
        output.WriteLine("SQL gate enabled; disposable WI095004 database, migrations, trigger/API/privacy/retention assertions execute.");
        try
        {
            await db.Database.MigrateAsync();
            Adult Create(string name)
            {
                var a = new Adult(Guid.NewGuid(), Guid.NewGuid());
                db.AddRange(new Account(a.Account, $"{a.Account:N}@example.invalid"), new AdultProfile(a.Profile, a.Account, name)); return a;
            }
            var creator = Create("Creator"); var participant = Create("Participant"); var admin = Create("Admin");
            var unassociated = Create("Unassociated"); var other = Create("Other family"); Create("Nonmember");
            var family = new FamilyGroup(Guid.NewGuid(), "Dates family", 100); var otherGroup = new FamilyGroup(Guid.NewGuid(), "Other", 100);
            db.AddRange(family, otherGroup);
            foreach (var adult in new[] { creator, participant, admin, unassociated, other })
            {
                var m = new FamilyGroupMembership(Guid.NewGuid(), adult.Account, adult.Profile, adult == other ? otherGroup.Id : family.Id, true);
                db.Add(m); if (adult == admin) db.Add(new FamilyAdministratorAssignment(Guid.NewGuid(), m.Id));
            }
            await db.SaveChangesAsync(); db.ChangeTracker.Clear();
            await action(new(db, connection, family.Id, new(), creator, participant, admin, unassociated, other));
        }
        finally { await using var cleanup = Context(connection); await cleanup.Database.EnsureDeletedAsync(); }
    }
    public sealed class ActorAccessor : ICurrentAccountAccessor { public CurrentAccount? Current { get; set; } }
    public sealed class Factory(string connection, Clock clock) : WebApplicationFactory<Program>
    {
        public ActorAccessor Actor { get; } = new();
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing"); builder.UseSetting("ConnectionStrings:FamilySafety", connection);
            builder.UseSetting("DevelopmentIdentity:Enabled", "false"); builder.UseSetting("DevelopmentFixtures:Enabled", "false");
            builder.UseSetting("DevelopmentInvitationOutbox:Enabled", "false");
            builder.ConfigureTestServices(services => {
                services.RemoveAll<ICurrentAccountAccessor>(); services.AddSingleton<ICurrentAccountAccessor>(Actor);
                services.RemoveAll<TimeProvider>(); services.AddSingleton<TimeProvider>(clock);
            });
        }
    }
}
