using System.Data.Common;
using System.Text.Json;
using System.Text.Json.Serialization;
using FamilySafety.Application.Events;
using FamilySafety.Domain.Events;
using FamilySafety.Domain.Profiles;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit.Abstractions;

namespace FamilySafety.Foundation.Tests;

public sealed class EventNeedPersistenceTests(ITestOutputHelper output)
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private static readonly string[] PrivacyStages = ["assigned", "accepted", "completed", "released"];
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    { Converters = { new JsonStringEnumConverter() } };

    [Fact]
    public async Task MigrationPreservesParticipationAndSupportsRollbackAndReapplication()
    {
        await WithDatabase(async w =>
        {
            var invitation = await w.Invitations.IssueAsync(w.A.Account, w.EventId, new(w.B.Profile), Ct);
            var before = await new EfFamilyEventRepository(w.Db).GetAsync(w.B.Account, w.EventId, Ct);
            await w.Db.Database.MigrateAsync();
            await w.Db.Database.MigrateAsync();
            Assert.Empty((await w.Needs.GetAsync(w.A.Account, w.EventId, Ct))!.Needs);
            Assert.Equal(before, await new EfFamilyEventRepository(w.Db).GetAsync(w.B.Account, w.EventId, Ct));
            Assert.Equal(invitation, (await w.Invitations.GetOwnAsync(w.B.Account, w.EventId, Ct))!.Invitation);
            var migrator = w.Db.GetService<IMigrator>();
            await migrator.MigrateAsync("20261004220426_EventInvitationsAndRsvp");
            await migrator.MigrateAsync();
            Assert.NotNull(await w.Needs.CreateAsync(w.A.Account, w.EventId, new("Food", null, EventNeedCategory.Food), Ct));
        }, previousMigration: true);
    }

    [Fact]
    public async Task ProjectionsProtectCurrentPendingCompletedAndHistoricalIdentityForEveryActor()
    {
        await WithDatabase(async w =>
        {
            foreach (var adult in new[] { w.B, w.Editor, w.Viewer, w.Admin })
                await w.Invitations.IssueAsync(w.A.Account, w.EventId, new(adult.Profile), Ct);
            var need = (await w.Needs.CreateAsync(w.A.Account, w.EventId, new("Bring chairs", null, null), Ct))!;
            need = (await w.Needs.ActAsync(w.A.Account, w.EventId, need.NeedId, need.Version, "assign", w.B.Profile, Ct))!;
            foreach (var stage in PrivacyStages)
            {
                if (stage == "accepted") need = (await w.Needs.ActAsync(w.B.Account, w.EventId, need.NeedId, need.Version, "accept", null, Ct))!;
                if (stage == "completed") need = (await w.Needs.ActAsync(w.B.Account, w.EventId, need.NeedId, need.Version, "complete", null, Ct))!;
                if (stage == "released")
                {
                    var other = (await w.Needs.CreateAsync(w.A.Account, w.EventId, new("Another Need", null, null), Ct))!;
                    other = (await w.Needs.ActAsync(w.B.Account, w.EventId, other.NeedId, other.Version, "claim", null, Ct))!;
                    need = (await w.Needs.ActAsync(w.B.Account, w.EventId, other.NeedId, other.Version, "release", null, Ct))!;
                }
                var organizer = (await w.Needs.GetAsync(w.A.Account, w.EventId, Ct))!;
                var named = organizer.Needs.Single(n => n.NeedId == need.NeedId);
                Assert.Equal(stage == "released" ? null : w.B.Profile, named.ResponsibleProfileId);
                Assert.All(named.History, h => Assert.Equal(w.B.Profile, h.ProfileId));
                Assert.NotEmpty(named.History);
                var own = (await w.Needs.GetAsync(w.B.Account, w.EventId, Ct))!.Needs.Single(n => n.NeedId == need.NeedId);
                Assert.Equal(stage != "released", own.IsResponsible);
                Assert.All(own.History, h => Assert.Equal(w.B.Profile, h.ProfileId));
                foreach (var actor in new[] { w.Editor, w.Viewer, w.Admin })
                {
                    var view = (await w.Needs.GetAsync(actor.Account, w.EventId, Ct))!;
                    Assert.False(view.CanManage); Assert.True(view.CanParticipate); Assert.Empty(view.EligibleParticipants);
                    var anonymous = view.Needs.Single(n => n.NeedId == need.NeedId);
                    Assert.Equal(need.Status, anonymous.Status);
                    Assert.False(anonymous.IsResponsible); Assert.Null(anonymous.ResponsibleProfileId); Assert.Null(anonymous.ResponsibleName);
                    Assert.Empty(anonymous.History);
                    var json = JsonSerializer.Serialize(view, JsonOptions);
                    Assert.DoesNotContain(w.B.Profile.ToString(), json);
                    Assert.DoesNotContain("Responsible adult", json);
                }
                Assert.Null(await w.Needs.GetAsync(w.Outsider.Account, w.EventId, Ct));
            }
            Assert.Equal("Responsible adult", (await w.Needs.GetAsync(w.A.Account, w.EventId, Ct))!.Needs[0].ResponsibleName);
            // After someone else takes the released Need, the prior holder sees only
            // their own history, never the new person's current identity/history.
            need = (await w.Needs.ActAsync(w.Viewer.Account, w.EventId, need.NeedId, need.Version, "claim", null, Ct))!;
            var former = (await w.Needs.GetAsync(w.B.Account, w.EventId, Ct))!.Needs.Single(n => n.NeedId == need.NeedId);
            Assert.Null(former.ResponsibleProfileId); Assert.Null(former.ResponsibleName);
            Assert.All(former.History, h => Assert.Equal(w.B.Profile, h.ProfileId));
        });
    }

    [Fact]
    public async Task ManagementEligibilityValidationAndTransitionsAreBounded()
    {
        await WithDatabase(async w =>
        {
            foreach (var actor in new[] { w.B, w.Editor, w.Admin, w.Outsider })
                Assert.Null(await w.Needs.CreateAsync(actor.Account, w.EventId, new("Not authorized", null, null), Ct));
            await Assert.ThrowsAsync<FamilyEventValidationException>(() => w.Needs.CreateAsync(w.A.Account, w.EventId, new(" ", null, null), Ct));
            await Assert.ThrowsAsync<FamilyEventValidationException>(() => w.Needs.CreateAsync(w.A.Account, w.EventId, new("Valid", null, (EventNeedCategory)99), Ct));
            var need = (await w.Needs.CreateAsync(w.A.Account, w.EventId, new(" Need ", " detail ", EventNeedCategory.Help), Ct))!;
            Assert.Equal("Need", need.Title); Assert.Equal("detail", need.Detail);
            Assert.Null(await w.Needs.ActAsync(w.B.Account, w.EventId, need.NeedId, need.Version, "claim", null, Ct));
            Assert.Null(await w.Needs.ActAsync(w.A.Account, w.EventId, need.NeedId, need.Version, "assign", w.B.Profile, Ct));
            var invite = (await w.Invitations.IssueAsync(w.A.Account, w.EventId, new(w.B.Profile), Ct))!;
            foreach (var status in new[] { EventRsvpStatus.Pending, EventRsvpStatus.Going, EventRsvpStatus.Maybe })
            {
                invite = (await w.Invitations.RespondAsync(w.B.Account, w.EventId, new(invite.InvitationId, invite.Version, status), Ct))!;
                need = (await w.Needs.ActAsync(w.B.Account, w.EventId, need.NeedId, need.Version, "claim", null, Ct))!;
                Assert.Null(await w.Needs.ActAsync(w.Editor.Account, w.EventId, need.NeedId, need.Version, "complete", null, Ct));
                Assert.Null(await w.Needs.ActAsync(w.Admin.Account, w.EventId, need.NeedId, need.Version, "release", null, Ct));
                need = (await w.Needs.ActAsync(w.B.Account, w.EventId, need.NeedId, need.Version, "release", null, Ct))!;
            }
            invite = (await w.Invitations.RespondAsync(w.B.Account, w.EventId, new(invite.InvitationId, invite.Version, EventRsvpStatus.NotGoing), Ct))!;
            Assert.Null(await w.Needs.ActAsync(w.B.Account, w.EventId, need.NeedId, need.Version, "claim", null, Ct));
            Assert.Null(await w.Needs.ActAsync(w.A.Account, w.EventId, need.NeedId, need.Version, "assign", w.B.Profile, Ct));
            invite = (await w.Invitations.RespondAsync(w.B.Account, w.EventId, new(invite.InvitationId, invite.Version, EventRsvpStatus.Pending), Ct))!;
            need = (await w.Needs.ActAsync(w.A.Account, w.EventId, need.NeedId, need.Version, "assign", w.B.Profile, Ct))!;
            need = (await w.Needs.ActAsync(w.B.Account, w.EventId, need.NeedId, need.Version, "decline", null, Ct))!;
            Assert.Equal(EventNeedStatus.Open, need.Status);
            await Assert.ThrowsAsync<EventNeedConflictException>(() => w.Needs.ActAsync(w.A.Account, w.EventId, need.NeedId, need.Version, "complete", null, Ct));
            need = (await w.Needs.EditAsync(w.A.Account, w.EventId, need.NeedId, new(need.Version, "Edited", null, null), Ct))!;
            Assert.Null(await w.Needs.EditAsync(w.Editor.Account, w.EventId, need.NeedId, new(need.Version, "No", null, null), Ct));
            need = (await w.Needs.ActAsync(w.B.Account, w.EventId, need.NeedId, need.Version, "claim", null, Ct))!;
            need = (await w.Needs.ActAsync(w.A.Account, w.EventId, need.NeedId, need.Version, "complete", null, Ct))!;
            Assert.Equal(EventNeedStatus.Completed, need.Status);
            Assert.False(await w.Needs.RemoveAsync(w.Admin.Account, w.EventId, need.NeedId, need.Version, Ct));
            Assert.True(await w.Needs.RemoveAsync(w.A.Account, w.EventId, need.NeedId, need.Version, Ct));
            Assert.Empty(w.Db.EventNeedResponsibilityChanges.AsNoTracking());
        });
    }

    [Theory]
    [InlineData("RsvpNotGoing", false)] [InlineData("RsvpNotGoing", true)]
    [InlineData("InvitationRevoked", false)] [InlineData("InvitationRevoked", true)]
    [InlineData("EventVisibilityLost", false)] [InlineData("EventVisibilityLost", true)]
    [InlineData("MembershipLost", false)] [InlineData("MembershipLost", true)]
    public async Task EligibilityLossReleasesHeldNeedsAtomicallyButNeverCompletedOrRestoresResponsibility(string reason, bool assigned)
    {
        await WithDatabase(async w =>
        {
            var invite = (await w.Invitations.IssueAsync(w.A.Account, w.EventId, new(w.B.Profile), Ct))!;
            var need = (await w.Needs.CreateAsync(w.A.Account, w.EventId, new("Active", null, null), Ct))!;
            need = (await w.Needs.ActAsync(assigned ? w.A.Account : w.B.Account, w.EventId, need.NeedId, need.Version,
                assigned ? "assign" : "claim", assigned ? w.B.Profile : null, Ct))!;
            var completed = (await w.Needs.CreateAsync(w.A.Account, w.EventId, new("Completed", null, null), Ct))!;
            completed = (await w.Needs.ActAsync(w.B.Account, w.EventId, completed.NeedId, completed.Version, "claim", null, Ct))!;
            completed = (await w.Needs.ActAsync(w.B.Account, w.EventId, completed.NeedId, completed.Version, "complete", null, Ct))!;
            switch (reason)
            {
                case "RsvpNotGoing":
                    invite = (await w.Invitations.RespondAsync(w.B.Account, w.EventId, new(invite.InvitationId, invite.Version, EventRsvpStatus.NotGoing), Ct))!; break;
                case "InvitationRevoked":
                    await w.Invitations.RevokeAsync(w.A.Account, w.EventId, invite.InvitationId, new(invite.Version), Ct); break;
                case "EventVisibilityLost":
                    await w.Db.FamilyEventAudienceMembers.Where(a => a.EventId == w.EventId && a.ProfileId == w.B.Profile).ExecuteDeleteAsync(); break;
                case "MembershipLost":
                    await w.Db.FamilyGroupMemberships.Where(m => m.AdultProfileId == w.B.Profile).ExecuteUpdateAsync(u => u.SetProperty(m => m.IsActive, false)); break;
            }
            // Inspect SQL before any Need read: the change was not lazy repair.
            var stored = await w.Db.FamilyEventNeeds.AsNoTracking().SingleAsync(n => n.Id == need.NeedId);
            Assert.Equal(EventNeedStatus.Open, stored.Status); Assert.Null(stored.ResponsibleProfileId); Assert.NotEqual(need.Version, stored.Version);
            var release = await w.Db.EventNeedResponsibilityChanges.AsNoTracking().SingleAsync(h => h.NeedId == need.NeedId && h.Status == EventNeedStatus.Open);
            Assert.Equal(w.B.Profile, release.ProfileId); Assert.Equal(need.Status, release.PriorStatus);
            Assert.Equal(reason, release.Reason); Assert.Equal(stored.UpdatedAtUtc, release.OccurredAtUtc);
            Assert.Equal(EventNeedStatus.Completed, (await w.Db.FamilyEventNeeds.AsNoTracking().SingleAsync(n => n.Id == completed.NeedId)).Status);
            if (reason == "MembershipLost") await w.Db.FamilyGroupMemberships.Where(m => m.AdultProfileId == w.B.Profile).ExecuteUpdateAsync(u => u.SetProperty(m => m.IsActive, true));
            if (reason == "EventVisibilityLost") { w.Db.ChangeTracker.Clear(); w.Db.Add(new FamilyEventAudienceMember(w.EventId, w.B.Profile, false)); await w.Db.SaveChangesAsync(); }
            if (reason == "InvitationRevoked") await w.Invitations.IssueAsync(w.A.Account, w.EventId, new(w.B.Profile), Ct);
            if (reason == "RsvpNotGoing") await w.Invitations.RespondAsync(w.B.Account, w.EventId, new(invite.InvitationId, invite.Version, EventRsvpStatus.Maybe), Ct);
            var restored = (await w.Needs.GetAsync(w.B.Account, w.EventId, Ct))!;
            Assert.True(restored.CanParticipate); Assert.False(restored.Needs.Single(n => n.NeedId == need.NeedId).IsResponsible);
            var open = restored.Needs.Single(n => n.NeedId == need.NeedId);
            Assert.Equal(EventNeedStatus.Open, open.Status);
            Assert.NotNull(await w.Needs.ActAsync(w.B.Account, w.EventId, open.NeedId, open.Version, "claim", null, Ct));
        });
    }

    [Fact]
    public async Task SimultaneousClaimsHaveOneWinnerAndStaleWritesCannotOverwrite()
    {
        await WithDatabase(async w =>
        {
            foreach (var adult in new[] { w.B, w.Viewer }) await w.Invitations.IssueAsync(w.A.Account, w.EventId, new(adult.Profile), Ct);
            var need = (await w.Needs.CreateAsync(w.A.Account, w.EventId, new("Single owner", null, null), Ct))!;
            async Task<bool> Claim(Adult adult)
            {
                await using var db = Context(w.Connection);
                try { await new EfEventNeedRepository(db, TimeProvider.System).ActAsync(adult.Account, w.EventId, need.NeedId, need.Version, "claim", null, Ct); return true; }
                catch (EventNeedConflictException) { return false; }
            }
            var outcomes = await Task.WhenAll(Claim(w.B), Claim(w.Viewer));
            Assert.Single(outcomes, success => success);
            await Assert.ThrowsAsync<EventNeedConflictException>(() => w.Needs.EditAsync(w.A.Account, w.EventId, need.NeedId, new(need.Version, "Stale", null, null), Ct));
            Assert.Equal("Single owner", (await w.Db.FamilyEventNeeds.AsNoTracking().SingleAsync()).Title);
            Assert.Single(w.Db.EventNeedResponsibilityChanges.AsNoTracking());
        });
    }

    [Theory]
    [InlineData("EventEdit")] [InlineData("RsvpLoss")]
    public async Task NeedMutationWaitsForFamilyFirstAndDoesNotDeadlockOrRetainLostEligibility(string otherMutation)
    {
        await WithDatabase(async w =>
        {
            var invite = (await w.Invitations.IssueAsync(w.A.Account, w.EventId, new(w.B.Profile), Ct))!;
            var need = (await w.Needs.CreateAsync(w.A.Account, w.EventId, new("Concurrent", null, null), Ct))!;
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var held = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var attempted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            await using var first = Context(w.Connection, new FamilyOverlapObserver(held, attempted, holder: true));
            await using var second = Context(w.Connection, new FamilyOverlapObserver(held, attempted, holder: false));
            Task other = otherMutation == "EventEdit"
                ? new EfFamilyEventRepository(first).UpdateAsync(w.A.Account, w.GroupId, w.EventId, new("Edited", "Details", new(2026, 10, 4), new(18, 0), "Hall"), deadline.Token)
                : new EfEventInvitationRepository(first, TimeProvider.System).RespondAsync(w.B.Account, w.EventId, new(invite.InvitationId, invite.Version, EventRsvpStatus.NotGoing), deadline.Token);
            Task<EventNeedView?>? claim = null;
            try
            {
                await held.Task.WaitAsync(deadline.Token);
                claim = new EfEventNeedRepository(second, TimeProvider.System).ActAsync(w.B.Account, w.EventId, need.NeedId, need.Version, "claim", null, deadline.Token);
                await Task.WhenAll(other, claim);
                Assert.Equal(otherMutation == "EventEdit", claim.Result is not null);
                output.WriteLine($"{otherMutation}: overlap at Family lock completed without SQL deadlock.");
            }
            finally { attempted.TrySetResult(); await deadline.CancelAsync(); await other; if (claim is not null) await claim; }
        });
    }

    private sealed class FamilyOverlapObserver(TaskCompletionSource held, TaskCompletionSource attempted, bool holder) : DbCommandInterceptor
    {
        private static bool FamilyUpdate(DbCommand c) => c.CommandText.Contains("UPDATE", StringComparison.Ordinal) && c.CommandText.Contains("FamilyGroups", StringComparison.Ordinal);
        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData data,
            InterceptionResult<int> result, CancellationToken ct = default)
        { if (!holder && FamilyUpdate(command)) attempted.TrySetResult(); return ValueTask.FromResult(result); }
        public override async ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData data, int result, CancellationToken ct = default)
        { if (holder && FamilyUpdate(command)) { held.TrySetResult(); await attempted.Task.WaitAsync(ct); } return result; }
    }

    private sealed record Adult(Guid Account, Guid Profile);
    private sealed record World(FamilySafetyDbContext Db, string Connection, Guid GroupId, Guid EventId,
        Adult A, Adult B, Adult Editor, Adult Viewer, Adult Admin, Adult Outsider)
    {
        public EfEventNeedRepository Needs => new(Db, TimeProvider.System);
        public EfEventInvitationRepository Invitations => new(Db, TimeProvider.System);
    }
    private static FamilySafetyDbContext Context(string connection, DbCommandInterceptor? observer = null)
    {
        var options = new DbContextOptionsBuilder<FamilySafetyDbContext>().UseSqlServer(connection, sql => sql.CommandTimeout(20));
        if (observer is not null) options.AddInterceptors(observer);
        return new(options.Options);
    }
    private async Task WithDatabase(Func<World, Task> test, bool previousMigration = false)
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        var connection = $"Server=localhost;Database=FamilySafety_WI095002_{Guid.NewGuid():N};Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True";
        await using var db = Context(connection);
        output.WriteLine("SQL gate enabled; disposable WI095002 database, migration and SQL assertions execute.");
        try
        {
            await db.GetService<IMigrator>().MigrateAsync(previousMigration ? "20261004220426_EventInvitationsAndRsvp" : null);
            Adult CreateAdult(string name)
            {
                var adult = new Adult(Guid.NewGuid(), Guid.NewGuid());
                db.AddRange(new Account(adult.Account, $"{adult.Account:N}@example.invalid"), new AdultProfile(adult.Profile, adult.Account, name)); return adult;
            }
            var a = CreateAdult("Organizer"); var b = CreateAdult("Responsible adult"); var editor = CreateAdult("Editor");
            var viewer = CreateAdult("Viewer"); var admin = CreateAdult("Administrator"); var outsider = CreateAdult("Outsider");
            var group = new FamilyGroup(Guid.NewGuid(), "Needs family", 100); db.Add(group);
            foreach (var adult in new[] { a, b, editor, viewer, admin, outsider })
            {
                var member = new FamilyGroupMembership(Guid.NewGuid(), adult.Account, adult.Profile, group.Id, true); db.Add(member);
                if (adult == admin || adult == outsider) db.Add(new FamilyAdministratorAssignment(Guid.NewGuid(), member.Id));
            }
            await db.SaveChangesAsync();
            var item = (await new EfFamilyEventRepository(db).CreateAsync(a.Account, group.Id,
                new("Gathering", "Details", new(2026, 10, 4), new(18, 0), "Hall", [b.Profile, editor.Profile, viewer.Profile, admin.Profile], [editor.Profile]), Ct))!;
            await test(new(db, connection, group.Id, item.EventId, a, b, editor, viewer, admin, outsider));
        }
        finally { await using var cleanup = Context(connection); await cleanup.Database.EnsureDeletedAsync(); }
    }
}
