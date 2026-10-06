using FamilySafety.Application.Events;
using FamilySafety.Domain.Events;
using FamilySafety.Domain.Profiles;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Data.Common;
using System.Collections.Concurrent;
using Microsoft.Data.SqlClient;
using Xunit.Abstractions;

namespace FamilySafety.Foundation.Tests;

public sealed class EventInvitationPersistenceTests(ITestOutputHelper output)
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    [Fact]
    public async Task InvitationMigrationPreservesExistingEventAndAudienceAndIsIdempotent()
    {
        await WithDatabase(async w =>
        {
            var events = new EfFamilyEventRepository(w.Db);
            var before = await events.GetAsync(w.B.Account, w.EventId, Ct);
            var audienceBefore = await w.Db.FamilyEventAudienceMembers.AsNoTracking()
                .OrderBy(a => a.ProfileId).Select(a => new { a.EventId, a.ProfileId, a.CanEdit }).ToListAsync();
            Assert.DoesNotContain("20261004220426_EventInvitationsAndRsvp", await w.Db.Database.GetAppliedMigrationsAsync());
            await w.Db.Database.MigrateAsync();
            await w.Db.Database.MigrateAsync();
            Assert.Contains("20261004220426_EventInvitationsAndRsvp", await w.Db.Database.GetAppliedMigrationsAsync());
            Assert.Equal(before, await events.GetAsync(w.B.Account, w.EventId, Ct));
            Assert.Equal(audienceBefore, await w.Db.FamilyEventAudienceMembers.AsNoTracking()
                .OrderBy(a => a.ProfileId).Select(a => new { a.EventId, a.ProfileId, a.CanEdit }).ToListAsync());
            Assert.Empty(w.Db.FamilyEventInvitations);
            Assert.NotNull(await w.Repo.IssueAsync(w.A.Account, w.EventId, new(w.B.Profile), Ct));
        }, previousMigration: true);
    }

    [Fact]
    public async Task OrganizerIssueAndNamedReadRespectAudienceMembershipAndRoles()
    {
        await WithDatabase(async world =>
        {
            Assert.Empty(world.Db.FamilyEventInvitations);
            var repo = world.Repo;
            var organizer = (await repo.GetOrganizerAsync(world.A.Account, world.EventId, Ct))!;
            Assert.Equal(2, organizer.EligibleViewers.Count);
            Assert.DoesNotContain(organizer.EligibleViewers, choice => choice.ProfileId == world.A.Profile);
            foreach (var target in new[] { world.A.Profile, world.Nonviewer.Profile, world.Inactive.Profile, Guid.NewGuid() })
                Assert.Null(await repo.IssueAsync(world.A.Account, world.EventId, new(target), Ct));
            foreach (var actor in new[] { world.B.Account, world.Editor.Account, world.Nonviewer.Account, world.Inactive.Account })
            {
                Assert.Null(await repo.GetOrganizerAsync(actor, world.EventId, Ct));
                Assert.Null(await repo.IssueAsync(actor, world.EventId, new(world.B.Profile), Ct));
            }
            var issued = (await repo.IssueAsync(world.A.Account, world.EventId, new(world.B.Profile), Ct))!;
            Assert.Null(issued.RsvpUpdatedAtUtc);
            Assert.Equal(EventRsvpStatus.Pending, issued.RsvpStatus);
            await Assert.ThrowsAsync<EventInvitationConflictException>(() => repo.IssueAsync(world.A.Account, world.EventId, new(world.B.Profile), Ct));
            var named = Assert.Single((await repo.GetOrganizerAsync(world.A.Account, world.EventId, Ct))!.Invitations);
            Assert.Equal("B", named.DisplayName);
            Assert.Equal(issued, named.Invitation);
            Assert.Null((await repo.GetOwnAsync(world.Editor.Account, world.EventId, Ct))!.Invitation);
            Assert.Null(await repo.GetOwnAsync(world.Nonviewer.Account, world.EventId, Ct));
            Assert.Null(await repo.GetOwnAsync(world.B.Account, Guid.NewGuid(), Ct));
        });
    }

    [Fact]
    public async Task SelfResponseRevocationAndReinvitePreserveHistoryAndVersion()
    {
        await WithDatabase(async w =>
        {
            var row = (await w.Repo.IssueAsync(w.A.Account, w.EventId, new(w.B.Profile), Ct))!;
            Assert.Null(await w.Repo.RespondAsync(w.Editor.Account, w.EventId, new(row.InvitationId, row.Version, EventRsvpStatus.Going), Ct));
            Assert.Null(await w.Repo.RespondAsync(w.A.Account, w.EventId, new(row.InvitationId, row.Version, EventRsvpStatus.Going), Ct));
            Assert.Null(await w.Repo.RevokeAsync(w.Editor.Account, w.EventId, row.InvitationId, new(row.Version), Ct));
            var initial = row;
            foreach (var state in Enum.GetValues<EventRsvpStatus>())
            {
                row = (await w.Repo.RespondAsync(w.B.Account, w.EventId, new(row.InvitationId, row.Version, state), Ct))!;
                Assert.Equal(state, row.RsvpStatus);
                Assert.NotNull(row.RsvpUpdatedAtUtc);
            }
            await Assert.ThrowsAsync<FamilyEventValidationException>(() => w.Repo.RespondAsync(w.B.Account, w.EventId, new(row.InvitationId, row.Version, (EventRsvpStatus)99), Ct));
            await Assert.ThrowsAsync<EventInvitationConflictException>(() => w.Repo.RespondAsync(w.B.Account, w.EventId, new(initial.InvitationId, initial.Version, EventRsvpStatus.Going), Ct));
            await Assert.ThrowsAsync<EventInvitationConflictException>(() => w.Repo.RevokeAsync(w.A.Account, w.EventId, initial.InvitationId, new(initial.Version), Ct));
            var revoked = (await w.Repo.RevokeAsync(w.A.Account, w.EventId, row.InvitationId, new(row.Version), Ct))!;
            Assert.Equal(row.RsvpStatus, revoked.RsvpStatus);
            Assert.Equal(row.RsvpUpdatedAtUtc, revoked.RsvpUpdatedAtUtc);
            Assert.NotNull(revoked.RevokedAtUtc);
            Assert.Null(await w.Repo.RevokeAsync(w.A.Account, w.EventId, row.InvitationId, new(revoked.Version), Ct));
            Assert.Null(await w.Repo.RespondAsync(w.B.Account, w.EventId, new(row.InvitationId, row.Version, EventRsvpStatus.Going), Ct));
            Assert.Null((await w.Repo.GetOwnAsync(w.B.Account, w.EventId, Ct))!.Invitation);
            Assert.NotNull(await new EfFamilyEventRepository(w.Db).GetAsync(w.B.Account, w.EventId, Ct));
            var next = (await w.Repo.IssueAsync(w.A.Account, w.EventId, new(w.B.Profile), Ct))!;
            Assert.NotEqual(row.InvitationId, next.InvitationId);
            Assert.Null(next.RsvpUpdatedAtUtc);
            Assert.Equal(2, (await w.Repo.GetOrganizerAsync(w.A.Account, w.EventId, Ct))!.Invitations.Count);
            Assert.Null(await w.Repo.RespondAsync(w.B.Account, w.EventId, new(row.InvitationId, next.Version, EventRsvpStatus.Going), Ct));
        });
    }

    [Fact]
    public async Task EligibilityLossRestorationAndOrganizerLossAreIndependent()
    {
        await WithDatabase(async w =>
        {
            var row = (await w.Repo.IssueAsync(w.A.Account, w.EventId, new(w.B.Profile), Ct))!;
            await SetMembership(w.Db, w.B.Profile, false);
            Assert.Null(await w.Repo.GetOwnAsync(w.B.Account, w.EventId, Ct));
            Assert.Null(await w.Repo.GetPendingAsync(w.B.Account, w.GroupId, Ct));
            Assert.Null(await w.Repo.RespondAsync(w.B.Account, w.EventId, new(row.InvitationId, row.Version, EventRsvpStatus.Going), Ct));
            await SetMembership(w.Db, w.B.Profile, true);
            await w.Db.FamilyEventAudienceMembers.Where(a => a.EventId == w.EventId && a.ProfileId == w.B.Profile).ExecuteDeleteAsync();
            Assert.Null(await w.Repo.GetOwnAsync(w.B.Account, w.EventId, Ct));
            Assert.Empty((await w.Repo.GetPendingAsync(w.B.Account, w.GroupId, Ct))!);
            Assert.Null(await w.Repo.RespondAsync(w.B.Account, w.EventId, new(row.InvitationId, row.Version, EventRsvpStatus.Going), Ct));
            // Same account with a different profile cannot use invitation provenance.
            // Production has one profile per Account. Deliberately remap the test
            // membership to another existing identity to prove profile matching,
            // without introducing a product rejoin/identity feature.
            var replacementAccount = Guid.NewGuid();
            var replacement = new AdultProfile(Guid.NewGuid(), replacementAccount, "Replacement");
            w.Db.AddRange(new Account(replacementAccount, $"{replacementAccount:N}@example.invalid"), replacement);
            await w.Db.SaveChangesAsync();
            await w.Db.FamilyGroupMemberships.Where(m => m.AdultProfileId == w.B.Profile).ExecuteUpdateAsync(u => u.SetProperty(m => m.AdultProfileId, replacement.Id));
            w.Db.FamilyEventAudienceMembers.Add(new(w.EventId, replacement.Id, false));
            await w.Db.SaveChangesAsync();
            Assert.Null((await w.Repo.GetOwnAsync(w.B.Account, w.EventId, Ct))!.Invitation);
            Assert.Empty((await w.Repo.GetPendingAsync(w.B.Account, w.GroupId, Ct))!);
            Assert.Null(await w.Repo.RespondAsync(w.B.Account, w.EventId, new(row.InvitationId, row.Version, EventRsvpStatus.Going), Ct));
            await w.Db.FamilyGroupMemberships.Where(m => m.AdultProfileId == replacement.Id).ExecuteUpdateAsync(u => u.SetProperty(m => m.AdultProfileId, w.B.Profile));
            w.Db.ChangeTracker.Clear();
            w.Db.FamilyEventAudienceMembers.Add(new(w.EventId, w.B.Profile, false));
            await w.Db.SaveChangesAsync();
            Assert.Equal(row, (await w.Repo.GetOwnAsync(w.B.Account, w.EventId, Ct))!.Invitation);
            await SetMembership(w.Db, w.A.Profile, false);
            Assert.Null(await w.Repo.GetOrganizerAsync(w.A.Account, w.EventId, Ct));
            Assert.Null(await w.Repo.IssueAsync(w.A.Account, w.EventId, new(w.Editor.Profile), Ct));
            Assert.Null(await w.Repo.RevokeAsync(w.A.Account, w.EventId, row.InvitationId, new(row.Version), Ct));
            Assert.NotNull(await w.Repo.RespondAsync(w.B.Account, w.EventId, new(row.InvitationId, row.Version, EventRsvpStatus.Going), Ct));
        });
    }

    [Fact]
    public async Task PendingQueryIncludesPastDatesAndExcludesOtherStatesFamiliesAndLostVisibility()
    {
        await WithDatabase(async w =>
        {
            var row = (await w.Repo.IssueAsync(w.A.Account, w.EventId, new(w.B.Profile), Ct))!;
            var pending = Assert.Single((await w.Repo.GetPendingAsync(w.B.Account, w.GroupId, Ct))!);
            Assert.Equal(new DateOnly(2020, 1, 1), pending.Date);
            Assert.Equal(w.EventId, pending.EventId);
            Assert.Empty((await w.Repo.GetPendingAsync(w.Editor.Account, w.GroupId, Ct))!);
            Assert.Empty((await w.Repo.GetPendingAsync(w.A.Account, w.GroupId, Ct))!); // Organizer is not the invitee.
            Assert.Empty((await w.Repo.GetPendingAsync(w.Nonviewer.Account, w.GroupId, Ct))!); // Administrator gets no override.
            Assert.Null(await w.Repo.GetPendingAsync(w.B.Account, Guid.NewGuid(), Ct));
            var secondFamily = new FamilyGroup(Guid.NewGuid(), "Second invitation family", 100);
            w.Db.AddRange(secondFamily,
                new FamilyGroupMembership(Guid.NewGuid(), w.A.Account, w.A.Profile, secondFamily.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), w.B.Account, w.B.Profile, secondFamily.Id, true));
            await w.Db.SaveChangesAsync();
            var secondEvent = (await new EfFamilyEventRepository(w.Db).CreateAsync(w.A.Account, secondFamily.Id,
                new("Other Family gathering", "Details", new DateOnly(2019, 1, 1), new TimeOnly(12, 0), "Hall", [w.B.Profile], []), Ct))!;
            await w.Repo.IssueAsync(w.A.Account, secondEvent.EventId, new(w.B.Profile), Ct);
            Assert.Equal(w.EventId, Assert.Single((await w.Repo.GetPendingAsync(w.B.Account, w.GroupId, Ct))!).EventId);
            Assert.Equal(secondEvent.EventId, Assert.Single((await w.Repo.GetPendingAsync(w.B.Account, secondFamily.Id, Ct))!).EventId);
            var earlier = (await new EfFamilyEventRepository(w.Db).CreateAsync(w.A.Account, w.GroupId,
                new("Earlier gathering", "Details", new DateOnly(2018, 1, 1), new TimeOnly(12, 0), "Hall", [w.B.Profile], []), Ct))!;
            var earlierInvite = (await w.Repo.IssueAsync(w.A.Account, earlier.EventId, new(w.B.Profile), Ct))!;
            Assert.Equal(new[] { earlier.EventId, w.EventId }, (await w.Repo.GetPendingAsync(w.B.Account, w.GroupId, Ct))!.Select(i => i.EventId));
            await w.Repo.RevokeAsync(w.A.Account, earlier.EventId, earlierInvite.InvitationId, new(earlierInvite.Version), Ct);
            Assert.Equal(w.EventId, Assert.Single((await w.Repo.GetPendingAsync(w.B.Account, w.GroupId, Ct))!).EventId);
            foreach (var state in new[] { EventRsvpStatus.Going, EventRsvpStatus.Maybe, EventRsvpStatus.NotGoing })
            {
                row = (await w.Repo.RespondAsync(w.B.Account, w.EventId, new(row.InvitationId, row.Version, state), Ct))!;
                Assert.Empty((await w.Repo.GetPendingAsync(w.B.Account, w.GroupId, Ct))!);
            }
            row = (await w.Repo.RespondAsync(w.B.Account, w.EventId, new(row.InvitationId, row.Version, EventRsvpStatus.Pending), Ct))!;
            Assert.Single((await w.Repo.GetPendingAsync(w.B.Account, w.GroupId, Ct))!);
            await w.Db.FamilyEventAudienceMembers.Where(a => a.EventId == w.EventId && a.ProfileId == w.B.Profile).ExecuteDeleteAsync();
            Assert.Empty((await w.Repo.GetPendingAsync(w.B.Account, w.GroupId, Ct))!);
        });
    }

    [Fact]
    public async Task FilteredUniqueIndexAndEfConcurrencyAreEnforcedBySql()
    {
        await WithDatabase(async w =>
        {
            var row = (await w.Repo.IssueAsync(w.A.Account, w.EventId, new(w.B.Profile), Ct))!;
            w.Db.FamilyEventInvitations.Add(new(Guid.NewGuid(), w.EventId, w.B.Profile, DateTimeOffset.UtcNow));
            await Assert.ThrowsAsync<DbUpdateException>(() => w.Db.SaveChangesAsync());
            w.Db.ChangeTracker.Clear();
            await using var second = Context(w.Connection);
            var stale = await second.FamilyEventInvitations.SingleAsync(i => i.Id == row.InvitationId);
            await w.Repo.RespondAsync(w.B.Account, w.EventId, new(row.InvitationId, row.Version, EventRsvpStatus.Going), Ct);
            stale.Respond(EventRsvpStatus.Maybe, DateTimeOffset.UtcNow);
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
            Assert.Equal(EventRsvpStatus.Going, (await w.Repo.GetOwnAsync(w.B.Account, w.EventId, Ct))!.Invitation!.RsvpStatus);
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConcurrentIssueResponseAndRevokeSerializeWithoutLostUpdates(bool competingRevoke)
    {
        await WithDatabase(async w =>
        {
            async Task<bool> Issue()
            {
                await using var db = Context(w.Connection);
                try { return await new EfEventInvitationRepository(db, TimeProvider.System).IssueAsync(w.A.Account, w.EventId, new(w.B.Profile), Ct) is not null; }
                catch (EventInvitationConflictException) { return false; }
            }
            var issues = await Task.WhenAll(Issue(), Issue());
            Assert.Single(issues, result => result);
            var row = (await w.Repo.GetOwnAsync(w.B.Account, w.EventId, Ct))!.Invitation!;
            async Task<bool> Mutate(bool revoke)
            {
                await using var db = Context(w.Connection);
                var repo = new EfEventInvitationRepository(db, TimeProvider.System);
                try
                {
                    return revoke
                        ? await repo.RevokeAsync(w.A.Account, w.EventId, row.InvitationId, new(row.Version), Ct) is not null
                        : await repo.RespondAsync(w.B.Account, w.EventId, new(row.InvitationId, row.Version, EventRsvpStatus.Going), Ct) is not null;
                }
                catch (EventInvitationConflictException) { return false; }
            }
            var results = await Task.WhenAll(Mutate(false), Mutate(competingRevoke));
            Assert.Single(results, result => result);
            var current = Assert.Single((await w.Repo.GetOrganizerAsync(w.A.Account, w.EventId, Ct))!.Invitations).Invitation;
            Assert.True(current.RevokedAtUtc is not null || current.RsvpStatus == EventRsvpStatus.Going);
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CompetingEligibilityLossIsRecheckedAfterMutationAcquiresFamilyLock(bool loseAudience)
    {
        await WithDatabase(async w =>
        {
            var row = (await w.Repo.IssueAsync(w.A.Account, w.EventId, new(w.B.Profile), Ct))!;
            await using var competing = Context(w.Connection);
            await using var transaction = await competing.Database.BeginTransactionAsync();
            await competing.FamilyGroups.Where(g => g.Id == w.GroupId).ExecuteUpdateAsync(u => u.SetProperty(g => g.CapacityLimit, g => g.CapacityLimit));
            if (loseAudience)
                await competing.FamilyEventAudienceMembers.Where(a => a.EventId == w.EventId && a.ProfileId == w.B.Profile).ExecuteDeleteAsync();
            else
                await SetMembership(competing, w.B.Profile, false);
            var interceptor = new FamilyLockObserver();
            await using var writing = new FamilySafetyDbContext(new DbContextOptionsBuilder<FamilySafetyDbContext>()
                .UseSqlServer(w.Connection).AddInterceptors(interceptor).Options);
            var pending = new EfEventInvitationRepository(writing, TimeProvider.System).RespondAsync(w.B.Account, w.EventId,
                new(row.InvitationId, row.Version, EventRsvpStatus.Going), Ct);
            await interceptor.Reached.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.False(pending.IsCompleted);
            await transaction.CommitAsync();
            Assert.Null(await pending);
            var preserved = await w.Db.FamilyEventInvitations.AsNoTracking().SingleAsync(i => i.Id == row.InvitationId);
            Assert.Null(preserved.RevokedAtUtc);
            Assert.Null(preserved.RsvpUpdatedAtUtc);
            Assert.Equal(row.Version, preserved.Version);
        });
    }

    [Theory]
    [InlineData("Issue")]
    [InlineData("Revoke")]
    [InlineData("RSVP")]
    public async Task EventEditAndInvitationMutationDoNotDeadlockWhenTransactionsOverlap(string mutation)
    {
        await WithDatabase(async w =>
        {
            output.WriteLine($"Task database: {new SqlConnectionStringBuilder(w.Connection).InitialCatalog}; normal fixture EnsureDeletedAsync cleanup.");
            output.WriteLine($"Mutation: {mutation}. Planned iterations: 3; per-iteration deadline: 30 seconds; SQL command timeout: 20 seconds.");
            EventInvitationView? invitation = mutation == "RSVP"
                ? await w.Repo.IssueAsync(w.A.Account, w.EventId, new(w.B.Profile), Ct)
                : null;
            for (var iteration = 1; iteration <= 3; iteration++)
            {
                output.WriteLine($"Iteration {iteration} started.");
                if (mutation == "Issue")
                    invitation = null;
                else if (mutation == "Revoke")
                    invitation = await w.Repo.IssueAsync(w.A.Account, w.EventId, new(w.B.Profile), Ct);
                else
                    Assert.NotNull(invitation);
                using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var editorHeldFamily = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                var mutationAttemptingFamily = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                var trace = new ConcurrentQueue<string>();
                FamilySafetyDbContext ObservedContext(string role) => new(new DbContextOptionsBuilder<FamilySafetyDbContext>()
                    .UseSqlServer(w.Connection, sql => sql.CommandTimeout(20))
                    .AddInterceptors(new EditInvitationOverlapObserver(role, editorHeldFamily, mutationAttemptingFamily, trace)).Options);
                await using var editing = ObservedContext("Event edit");
                await using var mutating = ObservedContext($"Invitation {mutation}");
                async Task<Exception?> Capture(Func<Task> operation)
                {
                    try { await operation(); return null; }
                    catch (Exception error) { return error; }
                }
                var edit = Capture(async () => Assert.NotNull(await new EfFamilyEventRepository(editing).UpdateAsync(
                    w.A.Account, w.GroupId, w.EventId,
                    new($"Edited gathering {iteration}", "Details", new DateOnly(2020, 1, 1), new TimeOnly(18, 0), "Hall"), deadline.Token)));
                Task<Exception?>? mutationTask = null;
                try
                {
                    await editorHeldFamily.Task.WaitAsync(deadline.Token);
                    var repository = new EfEventInvitationRepository(mutating, TimeProvider.System);
                    mutationTask = mutation switch
                    {
                        "Issue" => Capture(async () => Assert.NotNull(await repository.IssueAsync(w.A.Account, w.EventId,
                            new(w.B.Profile), deadline.Token))),
                        "Revoke" => Capture(async () => Assert.NotNull(await repository.RevokeAsync(w.A.Account, w.EventId,
                            invitation!.InvitationId, new(invitation.Version), deadline.Token))),
                        "RSVP" => Capture(async () => Assert.NotNull(await repository.RespondAsync(w.B.Account, w.EventId,
                            new(invitation!.InvitationId, invitation.Version, EventRsvpStatus.Going), deadline.Token))),
                        _ => throw new ArgumentOutOfRangeException(nameof(mutation)),
                    };
                    var errors = await Task.WhenAll(edit, mutationTask);
                    foreach (var entry in trace) output.WriteLine(entry);
                    for (var path = 0; path < errors.Length; path++)
                    {
                        output.WriteLine($"{(path == 0 ? "Event edit" : $"Invitation {mutation}")} outcome: {(errors[path] is null ? "success" : "failure")}");
                        if (errors[path] is { } error)
                        {
                            output.WriteLine(error.ToString());
                            for (Exception? cause = error; cause is not null; cause = cause.InnerException)
                                if (cause is SqlException sqlError) output.WriteLine($"SQL error number: {sqlError.Number}");
                        }
                    }
                    Assert.All(errors, error => Assert.Null(error));
                    if (mutation == "Issue")
                    {
                        invitation = (await w.Repo.GetOrganizerAsync(w.A.Account, w.EventId, Ct))!.Invitations
                            .Single(row => row.Invitation.RevokedAtUtc is null).Invitation;
                        Assert.NotNull(await w.Repo.RevokeAsync(w.A.Account, w.EventId, invitation.InvitationId, new(invitation.Version), Ct));
                    }
                    if (mutation == "RSVP")
                        invitation = (await w.Repo.GetOwnAsync(w.B.Account, w.EventId, Ct))!.Invitation;
                    output.WriteLine($"Iteration {iteration} passed.");
                }
                finally
                {
                    await deadline.CancelAsync();
                    mutationAttemptingFamily.TrySetResult();
                    await edit;
                    if (mutationTask is not null) await mutationTask;
                }
            }
        });
    }

    // Pause only at an actual completed Family UPDATE. The invitation mutation's
    // Family UPDATE is announced before execution after any pre-lock target read.
    // Repository SQL, isolation, lock hints and mutation behavior are unchanged.
    private sealed class EditInvitationOverlapObserver(string role, TaskCompletionSource editorHeldFamily,
        TaskCompletionSource mutationAttemptingFamily, ConcurrentQueue<string> trace) : DbCommandInterceptor
    {
        private static bool FamilyUpdate(DbCommand command) => command.CommandText.Contains("UPDATE", StringComparison.Ordinal)
            && command.CommandText.Contains("FamilyGroups", StringComparison.Ordinal);
        private void Record(string phase, DbCommand command) => trace.Enqueue(
            $"{DateTimeOffset.UtcNow:O} {role} {phase}; isolation={command.Transaction?.IsolationLevel}; SQL: {command.CommandText}");

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Record("nonquery executing", command);
            if (role.StartsWith("Invitation ", StringComparison.Ordinal) && FamilyUpdate(command)) mutationAttemptingFamily.TrySetResult();
            return ValueTask.FromResult(result);
        }

        public override async ValueTask<int> NonQueryExecutedAsync(DbCommand command,
            CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            Record("nonquery completed", command);
            if (role == "Event edit" && FamilyUpdate(command))
            {
                editorHeldFamily.TrySetResult();
                await mutationAttemptingFamily.Task.WaitAsync(cancellationToken);
            }
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Record("reader executing", command);
            return ValueTask.FromResult(result);
        }

        public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command,
            CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
        {
            Record("reader opened", command);
            return ValueTask.FromResult(result);
        }
    }

    private sealed class FamilyLockObserver : DbCommandInterceptor
    {
        public TaskCompletionSource Reached { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (command.CommandText.Contains("UPDATE", StringComparison.Ordinal) && command.CommandText.Contains("FamilyGroups", StringComparison.Ordinal))
                Reached.TrySetResult();
            return ValueTask.FromResult(result);
        }
    }

    private static Task<int> SetMembership(FamilySafetyDbContext db, Guid profile, bool active) =>
        db.FamilyGroupMemberships.Where(m => m.AdultProfileId == profile).ExecuteUpdateAsync(u => u.SetProperty(m => m.IsActive, active));
    private static FamilySafetyDbContext Context(string connection) => new(new DbContextOptionsBuilder<FamilySafetyDbContext>().UseSqlServer(connection).Options);
    private sealed record Adult(Guid Account, Guid Profile);
    private sealed record World(FamilySafetyDbContext Db, string Connection, Guid GroupId, Guid EventId,
        Adult A, Adult B, Adult Editor, Adult Nonviewer, Adult Inactive)
    {
        public EfEventInvitationRepository Repo => new(Db, TimeProvider.System);
    }

    private static async Task WithDatabase(Func<World, Task> test, bool previousMigration = false)
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        var connection = $"Server=localhost;Database=FamilySafety_WI095001_{Guid.NewGuid():N};Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True";
        await using var db = Context(connection);
        try
        {
            await db.GetService<IMigrator>().MigrateAsync(previousMigration
                ? "20261002013148_SafetyIncidentDomainAndAccessFoundation" : null);
            Adult Adult(string name)
            {
                var adult = new Adult(Guid.NewGuid(), Guid.NewGuid());
                db.AddRange(new Account(adult.Account, $"{adult.Account:N}@example.invalid"), new AdultProfile(adult.Profile, adult.Account, name));
                return adult;
            }
            var a = Adult("A"); var b = Adult("B"); var editor = Adult("Editor"); var other = Adult("Administrator"); var inactive = Adult("Inactive");
            var group = new FamilyGroup(Guid.NewGuid(), "Invitation family", 100);
            db.Add(group);
            foreach (var adult in new[] { a, b, editor, other, inactive })
            {
                var member = new FamilyGroupMembership(Guid.NewGuid(), adult.Account, adult.Profile, group.Id, adult != inactive);
                db.Add(member);
                if (adult == editor || adult == other) db.Add(new FamilyAdministratorAssignment(Guid.NewGuid(), member.Id));
            }
            await db.SaveChangesAsync();
            var item = (await new EfFamilyEventRepository(db).CreateAsync(a.Account, group.Id,
                new("Past gathering", "Details", new DateOnly(2020, 1, 1), new TimeOnly(18, 0), "Hall", [b.Profile, editor.Profile], [editor.Profile]), Ct))!;
            await test(new(db, connection, group.Id, item.EventId, a, b, editor, other, inactive));
        }
        finally { await using var cleanup = Context(connection); await cleanup.Database.EnsureDeletedAsync(); }
    }
}
