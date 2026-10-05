using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FamilySafety.Application.CheckIns;
using FamilySafety.Application.Profiles;
using FamilySafety.Domain.CheckIns;
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

public sealed class CheckInTests(ITestOutputHelper output)
{
    private static readonly string[] ResponseFields = ["authorDisplayName", "authorProfileId", "checkInId", "createdAtUtc", "familyGroupId", "note", "recipientProfileIds", "status"];
    private static readonly CancellationToken Ct = CancellationToken.None;

    [Fact]
    public void DomainAcceptsOnlyCanonicalStatusesAndOptionalPlainText()
    {
        CheckIn Make(CheckInStatus status, string? note) => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0, status, note, DateTimeOffset.UtcNow);
        Assert.Equal(5, Enum.GetValues<CheckInStatus>().Length);
        foreach (var status in Enum.GetValues<CheckInStatus>()) Assert.Equal(status, Make(status, null).Status);
        Assert.Throws<ArgumentException>(() => Make((CheckInStatus)99, null));
        Assert.Throws<ArgumentException>(() => Make(CheckInStatus.Arrived, new string('x', 1001)));
        Assert.Null(Make(CheckInStatus.ImOkay, " ").Note);
        Assert.Equal("<script>plain text</script>", Make(CheckInStatus.Delayed, " <script>plain text</script> ").Note);
    }

    [Fact]
    public async Task SqlApiExplicitIdentityAudienceNondisclosureAndNoLocation()
    {
        await WithDatabase(async w =>
        {
            using var factory = new Factory(w.Connection);
            using var client = factory.CreateClient();
            factory.Actor.Current = new(w.Author.Account);
            var created = await client.PostAsJsonAsync("/api/check-ins", new {
                familyGroupId = w.Family, status = "PlansChanged", note = " Private note ",
                recipientProfileIds = new[] { w.Recipient.Profile }, authorProfileId = w.Admin.Profile,
                accountId = w.Admin.Account, location = new { latitude = 42, longitude = -72 }, tripId = Guid.NewGuid(),
            });
            Assert.Equal(HttpStatusCode.OK, created.StatusCode);
            var json = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
            var id = json.RootElement.GetProperty("checkInId").GetGuid();
            Assert.Equal(w.Author.Profile, json.RootElement.GetProperty("authorProfileId").GetGuid());
            Assert.Equal("Private note", json.RootElement.GetProperty("note").GetString());
            Assert.Equal(new[] { w.Recipient.Profile }, json.RootElement.GetProperty("recipientProfileIds").EnumerateArray().Select(e => e.GetGuid()));
            Assert.Equal(ResponseFields,
                json.RootElement.EnumerateObject().Select(p => p.Name).Order());
            Assert.DoesNotContain(w.Db.Model.FindEntityType(typeof(CheckIn))!.GetProperties(), p => p.Name.Contains("Location") || p.Name.Contains("Latitude") || p.Name == "TripId");
            Assert.Equal(0, await w.Db.Concerns.CountAsync()); Assert.Equal(0, await w.Db.Incidents.CountAsync());
            Assert.Single(await client.GetFromJsonAsync<JsonElement[]>("/api/check-ins/mine") ?? []);
            factory.Actor.Current = new(w.Recipient.Account);
            var received = (await client.GetFromJsonAsync<JsonElement[]>("/api/check-ins/received"))!.Single();
            Assert.Equal(id, received.GetProperty("checkInId").GetGuid());
            Assert.Equal(JsonValueKind.Null, received.GetProperty("recipientProfileIds").ValueKind);
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/check-ins/{id}")).StatusCode);
            Assert.Empty((await client.GetFromJsonAsync<JsonElement[]>("/api/check-ins/mine"))!);
            foreach (var actor in new[] { w.Admin, w.Other, w.Inactive })
            {
                factory.Actor.Current = new(actor.Account);
                Assert.Empty((await client.GetFromJsonAsync<JsonElement[]>("/api/check-ins/received"))!);
                var denied = await client.GetAsync($"/api/check-ins/{id}?selectedFamily={w.Family}");
                var missing = await client.GetAsync($"/api/check-ins/{Guid.NewGuid()}");
                Assert.Equal(HttpStatusCode.NotFound, denied.StatusCode); Assert.Equal(missing.StatusCode, denied.StatusCode);
                Assert.Equal("", await denied.Content.ReadAsStringAsync());
                Assert.Equal(await missing.Content.ReadAsStringAsync(), await denied.Content.ReadAsStringAsync());
            }
            factory.Actor.Current = new(w.Author.Account);
            foreach (var bad in new[] { w.Other.Profile, w.Inactive.Profile, Guid.NewGuid() })
                Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync("/api/check-ins", Request(w, bad))).StatusCode);
            foreach (var status in new object[] { 99, "Safe", "Emergency" })
                Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/check-ins", new { familyGroupId = w.Family, status, recipientProfileIds = Array.Empty<Guid>() })).StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/check-ins", new { familyGroupId = w.Family, status = "Arrived" })).StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/check-ins", new { familyGroupId = w.Family, recipientProfileIds = Array.Empty<Guid>() })).StatusCode);
            Assert.Single(await w.Db.CheckIns.ToListAsync());
            Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.PutAsJsonAsync($"/api/check-ins/{id}", new { status = "Arrived" })).StatusCode);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.DeleteAsync($"/api/check-ins/{id}")).StatusCode);
            factory.Actor.Current = new(w.Other.Account);
            Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync("/api/check-ins", Request(w, w.Recipient.Profile))).StatusCode);
            factory.Actor.Current = null;
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/check-ins/mine")).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/check-ins", Request(w, w.Recipient.Profile))).StatusCode);
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MembershipLossPermanentlyEndsSharingWhileAuthorHistorySurvives(bool authorLeaves)
    {
        await WithDatabase(async w =>
        {
            var entry = (await w.Repository.CreateAsync(w.Author.Account, Request(w, w.Recipient.Profile), Ct))!;
            Assert.Single(await w.Repository.ListAsync(w.Recipient.Account, true, Ct));
            var actor = authorLeaves ? w.Author : w.Recipient;
            w.Db.ChangeTracker.Clear();
            var member = await w.Db.FamilyGroupMemberships.SingleAsync(m => m.AdultProfileId == actor.Profile);
            member.End(); await w.Db.SaveChangesAsync(); w.Db.ChangeTracker.Clear();
            Assert.Empty(await w.Repository.ListAsync(w.Recipient.Account, true, Ct));
            Assert.Null(await w.Repository.GetAsync(w.Recipient.Account, entry.CheckInId, Ct));
            Assert.Single(await w.Repository.ListAsync(w.Author.Account, false, Ct));
            Assert.True((await w.Db.CheckInRecipients.SingleAsync()).IsInvalidated);
            // Simulate reactivation of the same membership. Neither saved history nor rejoin restores a grant.
            await w.Db.FamilyGroupMemberships.Where(m => m.Id == member.Id).ExecuteUpdateAsync(u => u.SetProperty(m => m.IsActive, true));
            Assert.Empty(await w.Repository.ListAsync(w.Recipient.Account, true, Ct));
            Assert.NotNull(await w.Repository.GetAsync(w.Author.Account, entry.CheckInId, Ct));
            Assert.Single(await w.Db.CheckIns.ToListAsync()); Assert.Single(await w.Db.CheckInRecipients.ToListAsync());
            var newEntry = await w.Repository.CreateAsync(w.Author.Account, Request(w, w.Recipient.Profile), Ct);
            Assert.NotNull(newEntry); Assert.Single(await w.Repository.ListAsync(w.Recipient.Account, true, Ct));
        });
    }

    [Fact]
    public async Task ReplacementProfileAndOperationalMembershipToggleCannotInheritGrant()
    {
        await WithDatabase(async w =>
        {
            var entry = (await w.Repository.CreateAsync(w.Author.Account, Request(w, w.Recipient.Profile), Ct))!;
            var replacement = Guid.NewGuid();
            // Keep original historical profile but change which profile the account currently owns.
            var oldAccount = Guid.NewGuid(); w.Db.Accounts.Add(new Account(oldAccount, "old-profile@example.invalid"));
            await w.Db.SaveChangesAsync();
            await w.Db.AdultProfiles.Where(p => p.Id == w.Recipient.Profile).ExecuteUpdateAsync(u => u.SetProperty(p => p.AccountId, oldAccount));
            w.Db.AdultProfiles.Add(new AdultProfile(replacement, w.Recipient.Account, "Same display name")); await w.Db.SaveChangesAsync();
            await w.Db.FamilyGroupMemberships.Where(m => m.AccountId == w.Recipient.Account).ExecuteUpdateAsync(u => u.SetProperty(m => m.AdultProfileId, replacement));
            Assert.Null(await w.Repository.GetAsync(w.Recipient.Account, entry.CheckInId, Ct));
            Assert.Empty(await w.Repository.ListAsync(w.Recipient.Account, true, Ct));
            // Even a direct off/on toggle without changing AccessVersion irreversibly invalidates sharing.
            await w.Db.FamilyGroupMemberships.Where(m => m.AccountId == w.Author.Account).ExecuteUpdateAsync(u => u.SetProperty(m => m.IsActive, false));
            await w.Db.FamilyGroupMemberships.Where(m => m.AccountId == w.Author.Account).ExecuteUpdateAsync(u => u.SetProperty(m => m.IsActive, true));
            Assert.True((await w.Db.CheckInRecipients.AsNoTracking().SingleAsync()).IsInvalidated);
            Assert.Single(await w.Repository.ListAsync(w.Author.Account, false, Ct));
        });
    }

    [Fact]
    public async Task MigrationRoundTripPreservesExistingFamilyDataAndPrivateCheckInWorks()
    {
        await WithDatabase(async w =>
        {
            await w.Db.GetService<IMigrator>().MigrateAsync("20261005020352_SharedFamilyDates");
            await w.Db.Database.MigrateAsync(); await w.Db.Database.MigrateAsync();
            Assert.Equal(5, await w.Db.Accounts.CountAsync());
            var entry = await w.Repository.CreateAsync(w.Author.Account, Request(w), Ct);
            Assert.NotNull(entry); Assert.Empty(entry.RecipientProfileIds!);
            Assert.Empty(await w.Repository.ListAsync(w.Recipient.Account, true, Ct));
        });
    }

    private static CreateCheckInRequest Request(World w, params Guid[] recipients) => new(w.Family, CheckInStatus.Delayed, "Personal report", recipients);
    private sealed record Adult(Guid Account, Guid Profile);
    private sealed record World(FamilySafetyDbContext Db, string Connection, Guid Family, Adult Author, Adult Recipient, Adult Admin, Adult Other, Adult Inactive)
    { public EfCheckInRepository Repository => new(Db, TimeProvider.System); }
    private static FamilySafetyDbContext Context(string connection) => new(new DbContextOptionsBuilder<FamilySafetyDbContext>().UseSqlServer(connection).Options);
    private async Task WithDatabase(Func<World, Task> action)
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        var connection = $"Server=localhost;Database=FamilySafety_WI095005_{Guid.NewGuid():N};Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True";
        await using var db = Context(connection);
        output.WriteLine("SQL gate enabled: isolated WI095005 migrations/API/privacy/membership assertions execute.");
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
    public sealed class Factory(string connection) : WebApplicationFactory<Program>
    {
        public ActorAccessor Actor { get; } = new();
        protected override void ConfigureWebHost(IWebHostBuilder builder) {
            builder.UseEnvironment("Testing"); builder.UseSetting("ConnectionStrings:FamilySafety", connection);
            builder.UseSetting("DevelopmentIdentity:Enabled", "false"); builder.UseSetting("DevelopmentFixtures:Enabled", "false");
            builder.UseSetting("DevelopmentInvitationOutbox:Enabled", "false");
            builder.ConfigureTestServices(services => { services.RemoveAll<ICurrentAccountAccessor>(); services.AddSingleton<ICurrentAccountAccessor>(Actor); });
        }
    }
}
