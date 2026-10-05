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

public sealed class TrustedContactTests(ITestOutputHelper output)
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private const string Path = "/api/trusted-contacts";
    private static SaveTrustedContactRequest Input(string? phone = null, string? email = "friend@example.invalid", string name = "Friend") =>
        new(name, phone, email, "Neighbor");

    [Theory]
    [InlineData("", null, "a@example.invalid", null)]
    [InlineData("Name", null, null, null)]
    [InlineData("Name", "not a phone", null, null)]
    [InlineData("Name", "123", null, null)]
    [InlineData("Name", "+1234567890123456", null, null)]
    [InlineData("Name", null, "invalid", null)]
    [InlineData("Name", "+44 20 7946 0958", "invalid", null)]
    [InlineData("Name\nText", null, "a@example.invalid", null)]
    public void InvalidContentIsRejected(string name, string? phone, string? email, string? label) =>
        Assert.Throws<TrustedContactValidationException>(() => TrustedContactFields.Validate(name, phone, email, label));

    [Fact]
    public void NormalizationPreservesDisplayAndDoesNotGuessCountry()
    {
        var fields = TrustedContactFields.Validate(" Friend ", " +44 (20) 7946-0958 ", " FRIEND@Example.Invalid ", " Neighbor ");
        Assert.Equal("Friend", fields.DisplayName); Assert.Equal("Neighbor", fields.RelationshipLabel);
        Assert.Equal("+44 (20) 7946-0958", fields.PhoneNumber); Assert.Equal("+442079460958", fields.NormalizedPhone);
        Assert.Equal("FRIEND@Example.Invalid", fields.EmailAddress); Assert.Equal("FRIEND@EXAMPLE.INVALID", fields.NormalizedEmail);
        Assert.Equal(fields.NormalizedPhone, TrustedContactFields.Validate("Other", "0044 20 7946 0958", null, null).NormalizedPhone);
        Assert.Equal("2025550123", TrustedContactFields.Validate("Local", "(202) 555-0123", null, " ").NormalizedPhone);
        Assert.Null(TrustedContactFields.Validate("Only email", " ", "x@example.invalid", " ").RelationshipLabel);
        Assert.Throws<TrustedContactValidationException>(() => TrustedContactFields.Validate(new string('x', 101), null, "x@example.invalid", null));
        Assert.Throws<TrustedContactValidationException>(() => TrustedContactFields.Validate("Name", null, "x@example.invalid", new string('x', 101)));
    }

    [Fact]
    public async Task OwnerOnlyApiLifecycleIsNondisclosingAndRemovalIsPermanent()
    {
        await WithDatabase(async w =>
        {
            using var factory = new Factory(w.Connection); using var client = factory.CreateClient();
            factory.Actor.Current = new(w.Owner.Account);
            var response = await client.PostAsJsonAsync(Path, new {
                displayName = " Manual friend ", phoneNumber = "+1 (202) 555-0101", emailAddress = "manual@example.invalid",
                relationshipLabel = " Friend ", ownerProfileId = w.Admin.Profile, familyGroupId = w.Group,
                contactProfileId = w.Outsider.Profile,
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode); Assert.True(response.Headers.CacheControl!.NoStore);
            var row = (await response.Content.ReadFromJsonAsync<TrustedContactView>())!;
            Assert.Equal("Manual friend", row.DisplayName); Assert.Equal("Friend", row.RelationshipLabel);
            Assert.Equal(w.Owner.Profile, (await w.Db.TrustedContacts.AsNoTracking().SingleAsync()).OwnerProfileId);
            Assert.Single((await client.GetFromJsonAsync<TrustedContactView[]>(Path))!);
            Assert.Equal(row, await client.GetFromJsonAsync<TrustedContactView>($"{Path}/{row.TrustedContactId}"));
            var missing = await client.GetAsync($"{Path}/{Guid.NewGuid()}");
            foreach (var actor in new[] { w.Admin, w.Member, w.Outsider })
            {
                factory.Actor.Current = new(actor.Account);
                var list = await client.GetAsync($"{Path}?ownerProfileId={w.Owner.Profile}&selectedFamily={w.Group}");
                Assert.Equal(HttpStatusCode.OK, list.StatusCode); Assert.Equal("[]", await list.Content.ReadAsStringAsync());
                var denied = await client.GetAsync($"{Path}/{row.TrustedContactId}?selectedFamily={w.Group}");
                Assert.Equal(missing.StatusCode, denied.StatusCode); Assert.Equal("", await denied.Content.ReadAsStringAsync());
                Assert.True(denied.Headers.CacheControl!.NoStore);
                // Wrong owner is denied before invalid content, stale version or duplicate identity is evaluated.
                Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync($"{Path}/{row.TrustedContactId}",
                    new EditTrustedContactRequest(Guid.Empty, "", null, null, null))).StatusCode);
                Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"{Path}/{row.TrustedContactId}?version={row.Version}")).StatusCode);
            }
            factory.Actor.Current = new(w.Owner.Account);
            foreach (var invalid in new[] { Input(name: " "), Input(email: null), Input(phone: "invalid"), Input(email: "invalid") })
                Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync(Path, invalid)).StatusCode);
            var edit = await client.PutAsJsonAsync($"{Path}/{row.TrustedContactId}", new EditTrustedContactRequest(row.Version,
                "Edited", null, "changed@example.invalid", null));
            Assert.Equal(HttpStatusCode.OK, edit.StatusCode);
            var changed = (await edit.Content.ReadFromJsonAsync<TrustedContactView>())!;
            Assert.Null(changed.PhoneNumber); Assert.Null(changed.RelationshipLabel); Assert.NotEqual(row.Version, changed.Version);
            Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync($"{Path}/{row.TrustedContactId}?version={row.Version}")).StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"{Path}/{row.TrustedContactId}?version={changed.Version}")).StatusCode);
            Assert.False(await w.Db.TrustedContacts.AnyAsync());
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"{Path}/{row.TrustedContactId}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsync($"{Path}/{row.TrustedContactId}/restore", null)).StatusCode);
            factory.Actor.Current = new(w.NoProfile);
            Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync(Path, Input())).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(Path)).StatusCode);
            factory.Actor.Current = null;
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(Path)).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync(Path, Input())).StatusCode);
        });
    }

    [Fact]
    public async Task OwnerScopedDuplicatesApplyToEitherMethodAndEditsWithNoGlobalIdentity()
    {
        await WithDatabase(async w =>
        {
            using var factory = new Factory(w.Connection); using var client = factory.CreateClient();
            factory.Actor.Current = new(w.Owner.Account);
            async Task<TrustedContactView> Create(SaveTrustedContactRequest request)
            {
                var response = await client.PostAsJsonAsync(Path, request); Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                return (await response.Content.ReadFromJsonAsync<TrustedContactView>())!;
            }
            var first = await Create(Input("+44 (20) 7946-0958", "Mixed@Example.Invalid"));
            Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync(Path, Input("+1 202 555 0123", " mixed@example.invalid ", "Different name"))).StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync(Path, Input("0044 20 7946 0958", "different@example.invalid"))).StatusCode);
            var second = await Create(Input("202-555-0123", "second@example.invalid")); // Same name allowed.
            Assert.Equal(HttpStatusCode.Conflict, (await client.PutAsJsonAsync($"{Path}/{second.TrustedContactId}",
                new EditTrustedContactRequest(second.Version, "Changed", first.PhoneNumber, "new@example.invalid", null))).StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, (await client.PutAsJsonAsync($"{Path}/{second.TrustedContactId}",
                new EditTrustedContactRequest(second.Version, "Changed", null, "mixed@example.invalid", null))).StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync($"{Path}/{second.TrustedContactId}",
                new EditTrustedContactRequest(second.Version, "Name", null, null, null))).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"{Path}/{first.TrustedContactId}",
                new EditTrustedContactRequest(first.Version, "Renamed", "0044 20 7946 0958", "MIXED@example.invalid", null))).StatusCode);
            factory.Actor.Current = new(w.Outsider.Account); // Has no Family membership.
            await Create(Input(first.PhoneNumber, first.EmailAddress));
            Assert.Equal(3, await w.Db.TrustedContacts.CountAsync());
            Assert.False(await w.Db.FamilyGroupMemberships.AnyAsync(m => m.AccountId == w.Outsider.Account));
        });
    }

    [Fact]
    public async Task ConcurrentDuplicatesAndStaleMutationsCannotOverwriteAcceptedData()
    {
        await WithDatabase(async w =>
        {
            async Task<bool> Create()
            {
                await using var db = Context(w.Connection);
                try { await new EfTrustedContactRepository(db).CreateAsync(w.Owner.Account, Input("+12025550100"), Ct); return true; }
                catch (TrustedContactDuplicateException) { return false; }
            }
            Assert.Single(await Task.WhenAll(Create(), Create()), x => x);
            var row = (await new EfTrustedContactRepository(w.Db).ListAsync(w.Owner.Account, Ct))!.Single();
            async Task<bool> Edit(string name)
            {
                await using var db = Context(w.Connection);
                try { await new EfTrustedContactRepository(db).EditAsync(w.Owner.Account, row.TrustedContactId,
                    new(row.Version, name, null, "friend@example.invalid", null), Ct); return true; }
                catch (TrustedContactConflictException) { return false; }
            }
            Assert.Single(await Task.WhenAll(Edit("One"), Edit("Two")), x => x);
            await Assert.ThrowsAsync<TrustedContactConflictException>(() => new EfTrustedContactRepository(w.Db)
                .DeleteAsync(w.Owner.Account, row.TrustedContactId, row.Version, Ct));
            Assert.Single(await w.Db.TrustedContacts.AsNoTracking().ToListAsync());
        });
    }

    [Fact]
    public async Task FamilyLossAndJoinDoNotChangePersonalContactsAndMigrationPreservesExistingData()
    {
        await WithDatabase(async w =>
        {
            // Round-trip only this feature on a disposable database, preserving the seeded accounts/memberships.
            await w.Db.GetService<IMigrator>().MigrateAsync("20261005032233_TemporarySharingSessions");
            await w.Db.Database.MigrateAsync(); await w.Db.Database.MigrateAsync();
            Assert.Equal(5, await w.Db.Accounts.CountAsync());
            var repo = new EfTrustedContactRepository(w.Db);
            var row = (await repo.CreateAsync(w.Owner.Account, Input(), Ct))!;
            await w.Db.FamilyGroupMemberships.Where(m => m.AdultProfileId == w.Owner.Profile)
                .ExecuteUpdateAsync(u => u.SetProperty(m => m.IsActive, false).SetProperty(m => m.EndedAt, DateTimeOffset.UtcNow));
            Assert.Equal(row, (await repo.ListAsync(w.Owner.Account, Ct))!.Single());
            var other = new FamilyGroup(Guid.NewGuid(), "Other", 100);
            w.Db.Add(other); w.Db.Add(new FamilyGroupMembership(Guid.NewGuid(), w.Owner.Account, w.Owner.Profile, other.Id, true));
            await w.Db.SaveChangesAsync(); Assert.Single((await repo.ListAsync(w.Owner.Account, Ct))!);
            Assert.Empty((await repo.ListAsync(w.Admin.Account, Ct))!);
            var externalOwner = (await repo.CreateAsync(w.Outsider.Account, Input(), Ct))!;
            // FK cascade follows actual profile deletion; this is not a new account-deletion workflow.
            await w.Db.AdultProfiles.Where(p => p.Id == w.Outsider.Profile).ExecuteDeleteAsync();
            Assert.False(await w.Db.TrustedContacts.AnyAsync(c => c.Id == externalOwner.TrustedContactId));
            Assert.NotNull(await repo.ReadAsync(w.Owner.Account, row.TrustedContactId, Ct));
        });
    }

    [Fact]
    public async Task ManualDesignationGrantsZeroProtectedProductAccessOrMembership()
    {
        await WithDatabase(async w =>
        {
            var repo = new EfTrustedContactRepository(w.Db);
            // Represent a real app user's email purely as manually entered information.
            await repo.CreateAsync(w.Owner.Account, Input(email: $"{w.Outsider.Account:N}@example.invalid"), Ct);
            var date = new DateOnly(2026, 10, 5); var now = DateTimeOffset.UtcNow;
            var membership = await w.Db.FamilyGroupMemberships.AsNoTracking().SingleAsync(m => m.AdultProfileId == w.Owner.Profile);
            var evt = new FamilyEvent(Guid.NewGuid(), w.Group, w.Owner.Profile, "Private event", "Private", date, new(12, 0), "Private");
            var plan = new FamilyPlan(Guid.NewGuid(), w.Group, w.Owner.Profile, "Private plan", date, new(12, 0), null, null, null, null, PlanAudienceType.SelectedPeople);
            var trip = new FamilyTrip(Guid.NewGuid(), w.Group, w.Owner.Profile, TripAudienceType.SelectedPeople, "Private trip", "Private", date, date, null, null, null, null, null, null);
            var checkIn = new CheckIn(Guid.NewGuid(), w.Group, w.Owner.Profile, membership.Id, membership.AccessVersion, CheckInStatus.ImOkay, null, now);
            var session = new SharingSession(Guid.NewGuid(), w.Group, w.Owner.Profile, membership.Id, membership.AccessVersion, now, now.AddHours(1));
            var location = new SharingCurrentLocation(session.Id); location.Replace(40, -75, 10, now, now);
            w.Db.AddRange(evt, plan, trip, checkIn, session, location); await w.Db.SaveChangesAsync();
            var concern = (await new EfConcernRepository(w.Db, TimeProvider.System).CreateAsync(w.Owner.Account, w.Group,
                new(w.Member.Profile, "Private report", []), Ct))!;
            var incident = (await new EfIncidentRepository(w.Db, TimeProvider.System, new EfConcernAccessQuery(w.Db, TimeProvider.System))
                .CreateAsync(w.Owner.Account, w.Group, new(w.Member.Profile, "Private incident", IncidentInformationCategory.Reported,
                    "Direct conversation", false, []), Ct))!;
            using var factory = new Factory(w.Connection); using var client = factory.CreateClient();
            foreach (var path in new[] {
                $"/api/family-groups/{w.Group}/members", $"/api/family-members/{w.Owner.Profile}/profile",
                $"/api/events/{evt.Id}", $"/api/plans/{plan.Id}", $"/api/trips/{trip.Id}",
                $"/api/check-ins/{checkIn.Id}", $"/api/sharing-sessions/{session.Id}",
                $"/api/concerns/{concern.ConcernId}", $"/api/incidents/{incident.IncidentId}",
            })
            {
                factory.Actor.Current = new(w.Owner.Account);
                Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(path)).StatusCode);
                factory.Actor.Current = new(w.Outsider.Account);
                Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"{path}?selectedFamily={w.Group}")).StatusCode);
            }
            Assert.False(await w.Db.FamilyGroupMemberships.AnyAsync(m => m.AccountId == w.Outsider.Account));
            Assert.Equal("[]", await (await client.GetAsync("/api/family-groups/administered")).Content.ReadAsStringAsync());
        });
    }

    private sealed record Adult(Guid Account, Guid Profile);
    private sealed record World(FamilySafetyDbContext Db, string Connection, Guid Group, Adult Owner, Adult Admin, Adult Member, Adult Outsider, Guid NoProfile);
    private static FamilySafetyDbContext Context(string connection) => new(new DbContextOptionsBuilder<FamilySafetyDbContext>().UseSqlServer(connection).Options);
    private async Task WithDatabase(Func<World, Task> action)
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        var connection = $"Server=localhost;Database=FamilySafety_WI095007_{Guid.NewGuid():N};Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True";
        await using var db = Context(connection);
        output.WriteLine("SQL gate enabled; disposable WI095007 database; migrations, ownership/API/duplicates/permanent-delete assertions execute.");
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
