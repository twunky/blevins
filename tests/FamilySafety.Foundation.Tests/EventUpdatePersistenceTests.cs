using System.Data.Common;
using System.Text.Json;
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

public sealed class EventUpdatePersistenceTests(ITestOutputHelper output)
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private static readonly string[] ExpectedPriorTexts = ["Original private revision", "First revised"];

    [Fact]
    public async Task MigrationPreservesExistingEventParticipationAndNeedsAndCanBeReapplied()
    {
        await WithDatabase(async w =>
        {
            var invite = await w.Invitations.IssueAsync(w.A.Account, w.EventId, new(w.B.Profile), Ct);
            var need = await new EfEventNeedRepository(w.Db, TimeProvider.System).CreateAsync(w.A.Account, w.EventId, new("Food", null, null), Ct);
            var before = await new EfFamilyEventRepository(w.Db).GetAsync(w.B.Account, w.EventId, Ct);
            await w.Db.Database.MigrateAsync();
            await w.Db.Database.MigrateAsync();
            Assert.Empty((await w.Updates.GetAsync(w.A.Account, w.EventId, Ct))!.Updates);
            Assert.Equal(before, await new EfFamilyEventRepository(w.Db).GetAsync(w.B.Account, w.EventId, Ct));
            Assert.Equal(invite, (await w.Invitations.GetOwnAsync(w.B.Account, w.EventId, Ct))!.Invitation);
            Assert.Equal(need!.NeedId, (await new EfEventNeedRepository(w.Db, TimeProvider.System).GetAsync(w.A.Account, w.EventId, Ct))!.Needs.Single().NeedId);
            await w.Db.GetService<IMigrator>().MigrateAsync("20261005003108_EventNeeds");
            await w.Db.Database.MigrateAsync();
            Assert.NotNull(await w.Updates.CreateAsync(w.A.Account, w.EventId, new("After reapplication"), Ct));
        }, previousMigration: true);
    }

    [Fact]
    public async Task VisibleProjectionOrdersCreationAndNeverDisclosesHiddenTextOrPriorRevisionsEvenToOrganizer()
    {
        await WithDatabase(async w =>
        {
            var clock = new Clock(); var repository = new EfEventUpdateRepository(w.Db, clock);
            var first = (await repository.CreateAsync(w.A.Account, w.EventId, new("Original private revision"), Ct))!;
            clock.Now = clock.Now.AddMinutes(1);
            var second = (await repository.CreateAsync(w.A.Account, w.EventId, new("Second visible"), Ct))!;
            clock.Now = clock.Now.AddMinutes(1);
            first = (await repository.EditAsync(w.A.Account, w.EventId, first.UpdateId, new(first.Version, "First revised"), Ct))!;
            clock.Now = clock.Now.AddMinutes(1);
            first = (await repository.EditAsync(w.A.Account, w.EventId, first.UpdateId, new(first.Version, "First current"), Ct))!;
            clock.Now = clock.Now.AddMinutes(1);
            var hidden = (await repository.CreateAsync(w.A.Account, w.EventId, new("Hidden secret"), Ct))!;
            Assert.True(await repository.HideAsync(w.A.Account, w.EventId, hidden.UpdateId, hidden.Version, Ct));
            foreach (var actor in new[] { w.A, w.B, w.Editor, w.Admin })
            {
                var view = (await repository.GetAsync(actor.Account, w.EventId, Ct))!;
                Assert.Equal(actor == w.A, view.CanManage);
                Assert.Equal(new[] { first.UpdateId, second.UpdateId }, view.Updates.Select(u => u.UpdateId));
                Assert.Equal("First current", view.Updates[0].Text); Assert.NotNull(view.Updates[0].UpdatedAtUtc);
                Assert.All(view.Updates, u => { Assert.Equal(w.A.Profile, u.AuthorProfileId); Assert.Equal("Organizer", u.AuthorName); });
                var json = JsonSerializer.Serialize(view);
                Assert.DoesNotContain("Original private revision", json); Assert.DoesNotContain("First revised", json);
                Assert.DoesNotContain("Hidden secret", json); Assert.DoesNotContain(hidden.UpdateId.ToString(), json);
                Assert.DoesNotContain("HiddenAt", json); Assert.DoesNotContain("Revision", json);
            }
            var retained = await w.Db.FamilyEventUpdates.AsNoTracking().SingleAsync(u => u.Id == hidden.UpdateId);
            Assert.Equal("Hidden secret", retained.Text); Assert.NotNull(retained.HiddenAtUtc);
            Assert.Equal(hidden.CreatedAtUtc, retained.CreatedAtUtc); Assert.Equal(w.A.Profile, retained.AuthorProfileId);
            var revisions = await w.Db.EventUpdateRevisions.AsNoTracking().OrderBy(r => r.RevisedAtUtc).ToListAsync();
            Assert.Equal(ExpectedPriorTexts, revisions.Select(r => r.Text));
            Assert.All(revisions, r => Assert.Equal(w.A.Profile, r.EditorProfileId));
            // Hide an edited Update: both its current record and all revisions remain.
            Assert.True(await repository.HideAsync(w.A.Account, w.EventId, first.UpdateId, first.Version, Ct));
            Assert.Single((await repository.GetAsync(w.B.Account, w.EventId, Ct))!.Updates);
            Assert.Equal(2, await w.Db.EventUpdateRevisions.CountAsync());
            Assert.Equal("First current", (await w.Db.FamilyEventUpdates.AsNoTracking().SingleAsync(u => u.Id == first.UpdateId)).Text);
        });
    }

    [Fact]
    public async Task WriteAuthorityIsOrganizerOnlyAndValidationIsPlainTextBounded()
    {
        await WithDatabase(async w =>
        {
            await w.Invitations.IssueAsync(w.A.Account, w.EventId, new(w.B.Profile), Ct);
            var need = (await new EfEventNeedRepository(w.Db, TimeProvider.System).CreateAsync(w.A.Account, w.EventId, new("Held", null, null), Ct))!;
            await new EfEventNeedRepository(w.Db, TimeProvider.System).ActAsync(w.B.Account, w.EventId, need.NeedId, need.Version, "claim", null, Ct);
            var update = (await w.Updates.CreateAsync(w.A.Account, w.EventId, new(" Plain text <b>message</b> "), Ct))!;
            Assert.Equal("Plain text <b>message</b>", update.Text);
            foreach (var actor in new[] { w.B, w.Editor, w.Admin, w.Outsider })
            {
                Assert.Null(await w.Updates.CreateAsync(actor.Account, w.EventId, new("Denied"), Ct));
                // Stale versions also remain nondisclosing for unauthorized actors.
                Assert.Null(await w.Updates.EditAsync(actor.Account, w.EventId, update.UpdateId, new(Guid.NewGuid(), "Denied"), Ct));
                Assert.False(await w.Updates.HideAsync(actor.Account, w.EventId, update.UpdateId, Guid.NewGuid(), Ct));
            }
            foreach (var invalid in new[] { " ", new string('a', 1001), null })
                await Assert.ThrowsAsync<FamilyEventValidationException>(() => w.Updates.CreateAsync(w.A.Account, w.EventId, new(invalid!), Ct));
            await Assert.ThrowsAsync<FamilyEventValidationException>(() => w.Updates.EditAsync(w.A.Account, w.EventId, update.UpdateId, new(update.Version, ""), Ct));
            Assert.Empty(await w.Db.EventUpdateRevisions.ToListAsync());
            Assert.NotNull(await w.Updates.CreateAsync(w.A.Account, w.EventId, new(new string('a', 1000)), Ct));
            var otherEvent = (await new EfFamilyEventRepository(w.Db).CreateAsync(w.A.Account, w.GroupId,
                new("Other", "Details", new(2026, 10, 4), new(18, 0), "Hall", [w.B.Profile], []), Ct))!;
            Assert.Null(await w.Updates.EditAsync(w.A.Account, otherEvent.EventId, update.UpdateId, new(update.Version, "Wrong Event"), Ct));
            Assert.False(await w.Updates.HideAsync(w.A.Account, otherEvent.EventId, update.UpdateId, update.Version, Ct));
            Assert.Null(await w.Updates.GetAsync(w.Outsider.Account, w.EventId, Ct));
            Assert.Null(await w.Updates.GetAsync(w.A.Account, Guid.NewGuid(), Ct));
        });
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task CurrentAccessLossDeniesReadsAndWritesWithoutChangingRetainedHistory(bool membershipLoss)
    {
        await WithDatabase(async w =>
        {
            var update = (await w.Updates.CreateAsync(w.A.Account, w.EventId, new("Before"), Ct))!;
            update = (await w.Updates.EditAsync(w.A.Account, w.EventId, update.UpdateId, new(update.Version, "Current"), Ct))!;
            await w.Invitations.IssueAsync(w.A.Account, w.EventId, new(w.B.Profile), Ct);
            if (membershipLoss)
                await w.Db.FamilyGroupMemberships.Where(m => m.AdultProfileId == w.B.Profile || m.AdultProfileId == w.A.Profile)
                    .ExecuteUpdateAsync(u => u.SetProperty(m => m.IsActive, false));
            else
                await w.Db.FamilyEventAudienceMembers.Where(a => a.EventId == w.EventId && a.ProfileId == w.B.Profile).ExecuteDeleteAsync();
            Assert.Null(await w.Updates.GetAsync(w.B.Account, w.EventId, Ct));
            Assert.Null(await w.Updates.EditAsync(w.B.Account, w.EventId, update.UpdateId, new(update.Version, "Known ID"), Ct));
            Assert.False(await w.Updates.HideAsync(w.B.Account, w.EventId, update.UpdateId, update.Version, Ct));
            if (membershipLoss)
            {
                Assert.Null(await w.Updates.GetAsync(w.A.Account, w.EventId, Ct));
                Assert.Null(await w.Updates.CreateAsync(w.A.Account, w.EventId, new("Lost organizer"), Ct));
                Assert.Null(await w.Updates.EditAsync(w.A.Account, w.EventId, update.UpdateId, new(update.Version, "Lost"), Ct));
                Assert.False(await w.Updates.HideAsync(w.A.Account, w.EventId, update.UpdateId, update.Version, Ct));
                Assert.Single((await w.Updates.GetAsync(w.Editor.Account, w.EventId, Ct))!.Updates);
            }
            Assert.Single(await w.Db.EventUpdateRevisions.ToListAsync());
            Assert.Equal("Current", (await w.Db.FamilyEventUpdates.AsNoTracking().SingleAsync()).Text);
        });
    }

    [Fact]
    public async Task ConcurrentEditsHaveOneWinnerAndStaleEditHideCannotOverwriteOrRestore()
    {
        await WithDatabase(async w =>
        {
            var update = (await w.Updates.CreateAsync(w.A.Account, w.EventId, new("Original"), Ct))!;
            async Task<bool> Edit(string text)
            {
                await using var db = Context(w.Connection);
                try { await new EfEventUpdateRepository(db, TimeProvider.System).EditAsync(w.A.Account, w.EventId, update.UpdateId, new(update.Version, text), Ct); return true; }
                catch (EventUpdateConflictException) { return false; }
            }
            Assert.Single(await Task.WhenAll(Edit("Winner A"), Edit("Winner B")), success => success);
            await Assert.ThrowsAsync<EventUpdateConflictException>(() => w.Updates.HideAsync(w.A.Account, w.EventId, update.UpdateId, update.Version, Ct));
            var current = (await w.Updates.GetAsync(w.A.Account, w.EventId, Ct))!.Updates.Single();
            Assert.Single(await w.Db.EventUpdateRevisions.ToListAsync());
            Assert.True(await w.Updates.HideAsync(w.A.Account, w.EventId, update.UpdateId, current.Version, Ct));
            await Assert.ThrowsAsync<EventUpdateConflictException>(() => w.Updates.EditAsync(w.A.Account, w.EventId, update.UpdateId, new(current.Version, "Stale after hide"), Ct));
            await Assert.ThrowsAsync<EventUpdateConflictException>(() => w.Updates.HideAsync(w.A.Account, w.EventId, update.UpdateId, current.Version, Ct));
            var hiddenVersion = (await w.Db.FamilyEventUpdates.AsNoTracking().SingleAsync()).Version;
            await Assert.ThrowsAsync<EventUpdateConflictException>(() => w.Updates.EditAsync(w.A.Account, w.EventId, update.UpdateId, new(hiddenVersion, "Restore attempt"), Ct));
            Assert.Empty((await w.Updates.GetAsync(w.B.Account, w.EventId, Ct))!.Updates);
            Assert.Single(await w.Db.EventUpdateRevisions.ToListAsync());
        });
    }

    [Fact]
    public async Task UpdateWaitsForFamilyFirstAlongsideEventEditWithoutDeadlock()
    {
        await WithDatabase(async w =>
        {
            var update = (await w.Updates.CreateAsync(w.A.Account, w.EventId, new("Before"), Ct))!;
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var held = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var attempted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            await using var first = Context(w.Connection, new FamilyOverlapObserver(held, attempted, true));
            await using var second = Context(w.Connection, new FamilyOverlapObserver(held, attempted, false));
            var eventEdit = new EfFamilyEventRepository(first).UpdateAsync(w.A.Account, w.GroupId, w.EventId,
                new("Changed", "Details", new(2026, 10, 4), new(18, 0), "Hall"), deadline.Token);
            Task<EventUpdateView?>? edit = null;
            try
            {
                await held.Task.WaitAsync(deadline.Token);
                edit = new EfEventUpdateRepository(second, TimeProvider.System).EditAsync(w.A.Account, w.EventId, update.UpdateId, new(update.Version, "After"), deadline.Token);
                await Task.WhenAll(eventEdit, edit); Assert.Equal("After", edit.Result!.Text);
                output.WriteLine("Event edit and Update edit overlapped at the Family lock without SQL deadlock.");
            }
            finally { attempted.TrySetResult(); await deadline.CancelAsync(); await eventEdit; if (edit is not null) await edit; }
        });
    }

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 10, 4, 12, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Now;
    }
    private sealed class FamilyOverlapObserver(TaskCompletionSource held, TaskCompletionSource attempted, bool holder) : DbCommandInterceptor
    {
        private static bool FamilyUpdate(DbCommand c) => c.CommandText.Contains("UPDATE", StringComparison.Ordinal) && c.CommandText.Contains("FamilyGroups", StringComparison.Ordinal);
        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData data, InterceptionResult<int> result, CancellationToken ct = default)
        { if (!holder && FamilyUpdate(command)) attempted.TrySetResult(); return ValueTask.FromResult(result); }
        public override async ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData data, int result, CancellationToken ct = default)
        { if (holder && FamilyUpdate(command)) { held.TrySetResult(); await attempted.Task.WaitAsync(ct); } return result; }
    }
    private sealed record Adult(Guid Account, Guid Profile);
    private sealed record World(FamilySafetyDbContext Db, string Connection, Guid GroupId, Guid EventId, Adult A, Adult B, Adult Editor, Adult Admin, Adult Outsider)
    {
        public EfEventUpdateRepository Updates => new(Db, TimeProvider.System);
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
        var connection = $"Server=localhost;Database=FamilySafety_WI095003_{Guid.NewGuid():N};Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True";
        await using var db = Context(connection);
        output.WriteLine("SQL gate enabled; disposable WI095003 database, migrations and SQL assertions execute.");
        try
        {
            await db.GetService<IMigrator>().MigrateAsync(previousMigration ? "20261005003108_EventNeeds" : null);
            Adult CreateAdult(string name)
            {
                var adult = new Adult(Guid.NewGuid(), Guid.NewGuid());
                db.AddRange(new Account(adult.Account, $"{adult.Account:N}@example.invalid"), new AdultProfile(adult.Profile, adult.Account, name)); return adult;
            }
            var a = CreateAdult("Organizer"); var b = CreateAdult("Viewer"); var editor = CreateAdult("Editor");
            var admin = CreateAdult("Administrator"); var outsider = CreateAdult("Outsider");
            var group = new FamilyGroup(Guid.NewGuid(), "Updates family", 100); db.Add(group);
            foreach (var adult in new[] { a, b, editor, admin, outsider })
            {
                var member = new FamilyGroupMembership(Guid.NewGuid(), adult.Account, adult.Profile, group.Id, true); db.Add(member);
                if (adult == admin || adult == outsider) db.Add(new FamilyAdministratorAssignment(Guid.NewGuid(), member.Id));
            }
            await db.SaveChangesAsync();
            var item = (await new EfFamilyEventRepository(db).CreateAsync(a.Account, group.Id,
                new("Gathering", "Details", new(2026, 10, 4), new(18, 0), "Hall", [b.Profile, editor.Profile, admin.Profile], [editor.Profile]), Ct))!;
            await test(new(db, connection, group.Id, item.EventId, a, b, editor, admin, outsider));
        }
        finally { await using var cleanup = Context(connection); await cleanup.Database.EnsureDeletedAsync(); }
    }
}
