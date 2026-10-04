using System.Text.Json;
using FamilySafety.Application.Concerns;
using FamilySafety.Application.Incidents;
using FamilySafety.Domain.Concerns;
using FamilySafety.Domain.Incidents;
using FamilySafety.Domain.Trips;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using static FamilySafety.Foundation.Tests.IncidentSqlFixture;

namespace FamilySafety.Foundation.Tests;

/// <summary>
/// SQL-gated Incident lineage, retention, audit, concurrency, and migration tests.
/// They return early (and report as passed) unless RUN_FAMILYSAFETY_SQL_TESTS=1.
/// </summary>
public sealed class IncidentRetentionAuditTests
{
    private const string PreviousMigration = "20261001052512_BoundConcernDeniedAuditSummaries";

    [Fact]
    public async Task SqlCreateFromConcernStoresNonAuthorizingLineageWithoutConcernSideEffects()
    {
        if (!Enabled) return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var group = Group(db);
            var otherGroup = Group(db, "Other family");
            var a = Join(db, group, "Concern creator A");
            var s = Join(db, group, "Concern subject");
            var c = Join(db, group, "Concern participant C");
            var d = Join(db, group, "Incident subject D");
            var p = Join(db, group, "Incident participant P");
            var e = Join(db, group, "Departing creator E");
            Join(db, otherGroup, a);
            await db.SaveChangesAsync();
            var concerns = new EfConcernRepository(db, clock);
            var concern = await concerns.CreateAsync(a.AccountId, group.Id,
                new CreateConcernRequest(s.ProfileId, "Arrival not confirmed", [c.ProfileId]), default);
            await concerns.AddEntryAsync(c.AccountId, concern!.ConcernId, new AddConcernEntryRequest(
                ConcernEntryKind.Observation, ConcernInformationCategory.Reported, "Concern-only detail", "Call",
                false, null, null, null), default);
            var otherConcern = await concerns.CreateAsync(a.AccountId, otherGroup.Id,
                new CreateConcernRequest(a.ProfileId, "Other family concern", []), default);
            var departingConcern = await concerns.CreateAsync(e.AccountId, group.Id,
                new CreateConcernRequest(s.ProfileId, "E's concern", []), default);
            var before = await ConcernSnapshotAsync(db, concern.ConcernId);

            clock.Set(Start.AddHours(1));
            var incidents = Repo(db, clock);
            // Only the Concern creator with current access, in the same group, may start from it.
            Assert.Null(await incidents.CreateAsync(c.AccountId, group.Id, Create(d.ProfileId, [], concern.ConcernId), default));
            Assert.Null(await incidents.CreateAsync(a.AccountId, group.Id, Create(d.ProfileId, [], Guid.NewGuid()), default));
            Assert.Null(await incidents.CreateAsync(a.AccountId, group.Id,
                Create(d.ProfileId, [], otherConcern!.ConcernId), default));
            e.Membership.End(Start.AddMinutes(30));
            await db.SaveChangesAsync();
            await db.FamilyGroupMemberships.Where(m => m.Id == e.Membership.Id)
                .ExecuteUpdateAsync(update => update.SetProperty(m => m.IsActive, true));
            db.ChangeTracker.Clear();
            Assert.Null(await incidents.CreateAsync(e.AccountId, group.Id,
                Create(d.ProfileId, [], departingConcern!.ConcernId), default));
            Assert.NotNull(await incidents.CreateAsync(e.AccountId, group.Id, Create(d.ProfileId, []), default));

            var created = await incidents.CreateAsync(a.AccountId, group.Id, Create(d.ProfileId, [], concern.ConcernId), default);
            var id = created!.IncidentId;
            Assert.Equal((concern.ConcernId, d.ProfileId), (created.OriginatingConcernId!.Value, created.SubjectProfileId));
            Assert.Equal(a.ProfileId, Assert.Single(created.Participants).ProfileId);
            Assert.Equal(IncidentEntryKind.Summary, Assert.Single(created.Entries).Kind);
            Assert.Equal(concern.ConcernId,
                await db.Incidents.Where(i => i.Id == id).Select(i => i.OriginatingConcernId).SingleAsync());

            // The Concern's state, clock, audience, entries, and audit are unchanged.
            Assert.Equal(before, await ConcernSnapshotAsync(db, concern.ConcernId));

            // Lineage grants nothing; it is visible only to callers authorized for both records.
            Assert.Null(await incidents.GetAsync(c.AccountId, id, default));
            await incidents.ReplaceParticipantsAsync(a.AccountId, id,
                new ReplaceIncidentParticipantsRequest([c.ProfileId, p.ProfileId]), default);
            Assert.Equal(concern.ConcernId, (await incidents.GetAsync(c.AccountId, id, default))!.OriginatingConcernId);
            var withoutConcernAccess = await incidents.GetAsync(p.AccountId, id, default);
            Assert.Null(withoutConcernAccess!.OriginatingConcernId);
            var plain = await incidents.CreateAsync(a.AccountId, group.Id, Create(d.ProfileId, [p.ProfileId]), default);
            var plainForP = await incidents.GetAsync(p.AccountId, plain!.IncidentId, default);
            Assert.Contains("\"OriginatingConcernId\":null", JsonSerializer.Serialize(withoutConcernAccess), StringComparison.Ordinal);
            Assert.Contains("\"OriginatingConcernId\":null", JsonSerializer.Serialize(plainForP), StringComparison.Ordinal);
            Assert.Null(typeof(IncidentSummaryView).GetProperty("OriginatingConcernId"));
            Assert.Equal(before.Audit, (await ConcernSnapshotAsync(db, concern.ConcernId)).Audit);

            // Losing Concern access hides the lineage; the Incident access is unaffected.
            await concerns.ReplaceParticipantsAsync(a.AccountId, concern.ConcernId,
                new ReplaceConcernParticipantsRequest([]), default);
            var concernAudit = await db.ConcernAuditEvents.CountAsync();
            Assert.Null((await incidents.GetAsync(c.AccountId, id, default))!.OriginatingConcernId);
            Assert.Equal(concern.ConcernId, (await incidents.GetAsync(a.AccountId, id, default))!.OriginatingConcernId);
            Assert.Equal(concernAudit, await db.ConcernAuditEvents.CountAsync());

            // After Concern expiry the reference discloses nothing, and cleanup of either side never blocks.
            await concerns.ResolveAsync(a.AccountId, concern.ConcernId, new ResolveConcernRequest("Ended"), default);
            clock.Set(Start.AddHours(1).AddDays(180));
            var afterExpiry = await incidents.GetAsync(a.AccountId, id, default);
            Assert.Null(afterExpiry!.OriginatingConcernId);
            Assert.Null(await incidents.CreateAsync(a.AccountId, group.Id, Create(d.ProfileId, [], concern.ConcernId), default));
            await concerns.ReconcileAsync(default);
            Assert.False(await db.Concerns.AnyAsync(x => x.Id == concern.ConcernId));
            Assert.Null((await incidents.GetAsync(a.AccountId, id, default))!.OriginatingConcernId);
            Assert.Equal(concern.ConcernId,
                await db.Incidents.Where(i => i.Id == id).Select(i => i.OriginatingConcernId).SingleAsync());

            // The Concern stores no Incident reference, and nothing created an Incident automatically.
            var concernIncidentColumns = await db.Database.SqlQueryRaw<string>(
                "SELECT COLUMN_NAME AS [Value] FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME LIKE 'Concern%' AND COLUMN_NAME LIKE '%Incident%'")
                .ToListAsync();
            Assert.Empty(concernIncidentColumns);
            Assert.Equal(3, await db.Incidents.CountAsync());
            Assert.Null(await concerns.GetAsync(p.AccountId, otherConcern.ConcernId, default));
        });
    }

    [Fact]
    public async Task SqlExpiredIncidentIsUnavailableBeforeCleanupAndCleanupDeletesTheWholeAggregate()
    {
        if (!Enabled) return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var group = Group(db);
            var a = Join(db, group, "Coordinator A");
            var b = Join(db, group, "Participant B");
            var c = Join(db, group, "Participant C");
            var s = Join(db, group, "Subject S");
            await db.SaveChangesAsync();
            var concerns = new EfConcernRepository(db, clock);
            var concern = await concerns.CreateAsync(a.AccountId, group.Id,
                new CreateConcernRequest(s.ProfileId, "Concern kept", [b.ProfileId]), default);
            await concerns.AddEntryAsync(b.AccountId, concern!.ConcernId, new AddConcernEntryRequest(
                ConcernEntryKind.Unknown, ConcernInformationCategory.Unknown, "Unknown", null, true, null, null, null), default);
            var repository = Repo(db, clock);
            var id = (await repository.CreateAsync(a.AccountId, group.Id,
                Create(s.ProfileId, [b.ProfileId, c.ProfileId, s.ProfileId], concern.ConcernId), default))!.IncidentId;
            var observed = await repository.AddEntryAsync(b.AccountId, id, Observation("Observation"), default);
            var entryId = observed!.Entries.Single(x => x.Text == "Observation").EntryId;
            await repository.AddEntryAsync(b.AccountId, id, Correction(entryId), default);
            await repository.AddEntryAsync(a.AccountId, id, Challenge(entryId), default);
            await repository.AddEntryAsync(a.AccountId, id, new AddIncidentEntryRequest(IncidentEntryKind.ContactAttempt,
                IncidentInformationCategory.Reported, null, "Own attempt", false, EventTimeUnknown: true,
                TargetLabel: "Mobile", ContactMethod: IncidentContactMethod.TextMessage,
                ContactOutcome: IncidentContactOutcome.SentNoReplyYet), default);
            var t1 = Assert.Single((await repository.CreateTaskAsync(a.AccountId, id, TaskFor("One", b.ProfileId), default))!.Tasks).TaskId;
            await repository.CompleteTaskAsync(b.AccountId, id, t1, Complete(), default);
            var t2 = (await repository.CreateTaskAsync(a.AccountId, id, TaskFor("Two", a.ProfileId), default))!
                .Tasks.Single(t => t.Description == "Two").TaskId;
            await repository.CancelTaskAsync(a.AccountId, id, t2, Cancel(), default);
            await repository.CreateTaskAsync(a.AccountId, id, TaskFor("Three", b.ProfileId), default);
            await repository.ReplaceParticipantsAsync(a.AccountId, id,
                new ReplaceIncidentParticipantsRequest([b.ProfileId, s.ProfileId]), default);
            await repository.TransferCoordinatorAsync(a.AccountId, id, new TransferIncidentCoordinatorRequest(b.ProfileId), default);
            clock.Set(Start.AddDays(1));
            await repository.ResolveAsync(b.AccountId, id, Resolve(), default);

            // A later system event: the subject's grant is revoked at day 100.
            s.Membership.End(Start.AddDays(100));
            await db.SaveChangesAsync();
            clock.Set(Start.AddDays(100));
            Assert.NotNull(await repository.GetAsync(a.AccountId, id, default));

            var expiresAt = Start.AddDays(1).AddDays(365);
            clock.Set(expiresAt.AddTicks(-1));
            Assert.NotNull(await repository.GetAsync(a.AccountId, id, default));
            Assert.Single(await repository.GetVisibleAsync(b.AccountId, default));

            // Expiry applies at request time, before any cleanup.
            clock.Set(expiresAt);
            Assert.Null(await repository.GetAsync(a.AccountId, id, default));
            Assert.Null(await repository.GetAsync(b.AccountId, id, default));
            Assert.Empty(await repository.GetVisibleAsync(b.AccountId, default));
            Assert.Null(await repository.AddEntryAsync(b.AccountId, id, Observation("After expiry"), default));
            Assert.Null(await repository.CompleteTaskAsync(b.AccountId, id, t1, Complete(), default));
            Assert.True(await db.Incidents.AnyAsync(i => i.Id == id));

            await repository.ReconcileAsync(default);
            Assert.False(await db.Incidents.AnyAsync(i => i.Id == id));
            Assert.Equal(0, await db.IncidentParticipantGrants.CountAsync(g => g.IncidentId == id));
            Assert.Equal(0, await db.IncidentEntries.CountAsync(x => x.IncidentId == id));
            Assert.Equal(0, await db.IncidentTasks.CountAsync(t => t.IncidentId == id));
            Assert.Equal(0, await db.IncidentCoordinatorChanges.CountAsync(x => x.IncidentId == id));
            Assert.Null(await repository.GetAsync(a.AccountId, id, default));

            // Concern product data is untouched by Incident cleanup.
            Assert.True(await db.Concerns.AnyAsync(x => x.Id == concern.ConcernId));
            Assert.Equal(1, await db.ConcernEntries.CountAsync(x => x.ConcernId == concern.ConcernId));

            // Audit retention is independent of product deletion: 365 days per event.
            var remaining = await db.IncidentAuditEvents.AsNoTracking().Where(x => x.IncidentId == id).ToListAsync();
            var revoked = Assert.Single(remaining);
            Assert.Equal(("AccessRevoked", s.ProfileId, Start.AddDays(100)),
                (revoked.Action, revoked.AffectedProfileId!.Value, revoked.OccurredAt));
            clock.Set(Start.AddDays(100).AddDays(365));
            await repository.ReconcileAsync(default);
            Assert.False(await db.IncidentAuditEvents.AnyAsync(x => x.IncidentId == id));
        });
    }

    [Fact]
    public async Task SqlOverdueOrRestoredDatabaseNeverServesExpiredDataOrAcceptsFrozenWrites()
    {
        if (!Enabled) return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var group = Group(db);
            var a = Join(db, group, "Coordinator");
            var b = Join(db, group, "Participant");
            await db.SaveChangesAsync();
            var repository = Repo(db, clock);
            var stale = (await repository.CreateAsync(a.AccountId, group.Id, Create(b.ProfileId, [b.ProfileId]), default))!.IncidentId;
            clock.Set(Start.AddDays(340));
            var dormant = (await repository.CreateAsync(a.AccountId, group.Id, Create(b.ProfileId, [b.ProfileId]), default))!.IncidentId;
            clock.Set(Start.AddDays(399));
            var fresh = (await repository.CreateAsync(a.AccountId, group.Id, Create(b.ProfileId, [b.ProfileId]), default))!.IncidentId;

            // No request or maintenance ran since creation: the persisted rows still say Open.
            clock.Set(Start.AddDays(400));
            Assert.Equal(3, await db.Incidents.CountAsync(i => i.State == IncidentState.Open));
            Assert.Null(await repository.GetAsync(b.AccountId, stale, default));
            Assert.Null(await repository.AddEntryAsync(a.AccountId, stale, Observation("Stale"), default));
            Assert.DoesNotContain(await repository.GetVisibleAsync(b.AccountId, default), i => i.IncidentId == stale);
            Assert.Null(await repository.AddEntryAsync(b.AccountId, dormant, Observation("Frozen write"), default));
            Assert.Null(await repository.ResolveAsync(a.AccountId, dormant, Resolve(), default));
            var frozen = await repository.GetAsync(b.AccountId, dormant, default);
            Assert.Equal((IncidentState.Frozen, Start.AddDays(370)), (frozen!.State, frozen.FrozenAt!.Value));

            var freshAudit = await db.IncidentAuditEvents.CountAsync(x => x.IncidentId == fresh);
            await repository.ReconcileAsync(default);
            await repository.ReconcileAsync(default);
            Assert.False(await db.Incidents.AnyAsync(i => i.Id == stale));
            Assert.Equal(1, await db.IncidentAuditEvents.CountAsync(x => x.IncidentId == dormant && x.Action == "Frozen"));
            // A non-candidate Incident is untouched by maintenance.
            var untouched = await db.Incidents.AsNoTracking().SingleAsync(i => i.Id == fresh);
            Assert.Equal((IncidentState.Open, Start.AddDays(399)), (untouched.State, untouched.LastAuthorizedMutationAt));
            Assert.Equal(freshAudit, await db.IncidentAuditEvents.CountAsync(x => x.IncidentId == fresh));
            Assert.Equal(IncidentEntryKind.Summary,
                Assert.Single(await db.IncidentEntries.Where(x => x.IncidentId == fresh).ToListAsync()).Kind);
        });
    }

    [Fact]
    public async Task SqlSuccessfulActionsAreAuditedIndividuallyWithMetadataOnly()
    {
        if (!Enabled) return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var group = Group(db);
            var a = Join(db, group, "Coordinator");
            var b = Join(db, group, "Participant");
            var c = Join(db, group, "Member");
            await db.SaveChangesAsync();
            var repository = Repo(db, clock);
            var id = (await repository.CreateAsync(a.AccountId, group.Id, Create(c.ProfileId, [b.ProfileId]), default))!.IncidentId;
            await repository.ReplaceParticipantsAsync(a.AccountId, id, new ReplaceIncidentParticipantsRequest([b.ProfileId, c.ProfileId]), default);
            await repository.ReplaceParticipantsAsync(a.AccountId, id, new ReplaceIncidentParticipantsRequest([b.ProfileId]), default);
            var observed = await repository.AddEntryAsync(b.AccountId, id, Observation("Secret observation text"), default);
            var entry = observed!.Entries.Single(x => x.Text == "Secret observation text").EntryId;
            await repository.AddEntryAsync(b.AccountId, id, Unknown("Secret unknown"), default);
            await repository.AddEntryAsync(a.AccountId, id, SummaryEntry("Secret summary"), default);
            await repository.AddEntryAsync(b.AccountId, id, new AddIncidentEntryRequest(IncidentEntryKind.ContactAttempt,
                IncidentInformationCategory.Reported, "Secret notes", "Own call", false, EventAt: Start,
                TargetLabel: "Secret target", ContactMethod: IncidentContactMethod.Call,
                ContactOutcome: IncidentContactOutcome.LeftMessage), default);
            await repository.AddEntryAsync(b.AccountId, id, Correction(entry), default);
            await repository.AddEntryAsync(a.AccountId, id, Challenge(entry), default);
            var task = Assert.Single((await repository.CreateTaskAsync(a.AccountId, id,
                new CreateIncidentTaskRequest("Secret task description", b.ProfileId), default))!.Tasks).TaskId;
            await repository.CompleteTaskAsync(b.AccountId, id, task, Complete(), default);
            var other = (await repository.CreateTaskAsync(b.AccountId, id, TaskFor("Secret self task", b.ProfileId), default))!
                .Tasks.Single(t => t.Description == "Secret self task").TaskId;
            await repository.CancelTaskAsync(b.AccountId, id, other, new CancelIncidentTaskRequest("Secret reason"), default);
            await repository.TransferCoordinatorAsync(a.AccountId, id, new TransferIncidentCoordinatorRequest(b.ProfileId), default);
            await repository.ResolveAsync(b.AccountId, id, Resolve() with { Summary = "Secret resolution" }, default);

            var events = await db.IncidentAuditEvents.AsNoTracking().Where(x => x.IncidentId == id).ToListAsync();
            var expected = new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["IncidentCreated"] = 1, ["ParticipantAdded"] = 2, ["ParticipantRemoved"] = 1,
                ["ObservationCreated"] = 1, ["UnknownCreated"] = 1, ["SummaryCreated"] = 1,
                ["ContactAttemptRecorded"] = 1, ["CorrectionCreated"] = 1, ["ChallengeCreated"] = 1,
                ["TaskCreated"] = 2, ["TaskCompleted"] = 1, ["TaskCancelled"] = 1,
                ["CoordinatorTransferred"] = 1, ["Resolved"] = 1,
            };
            Assert.Equal(expected, events.GroupBy(x => x.Action).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal));
            Assert.All(events, x => Assert.True(x.Authorized && x.ActorAccountId is not null && x.DeniedAttemptCount == 0));
            Assert.All(events.Where(x => x.Action.EndsWith("Created", StringComparison.Ordinal)
                && x.Action is not "IncidentCreated" and not "TaskCreated"), x => Assert.NotNull(x.EntryId));
            Assert.All(events.Where(x => x.Action.StartsWith("Task", StringComparison.Ordinal)), x => Assert.NotNull(x.TaskId));

            // The audit table has no column that could carry Incident content.
            var characterColumns = await db.Database.SqlQueryRaw<string>(
                "SELECT COLUMN_NAME AS [Value] FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'IncidentAuditEvents' AND DATA_TYPE IN ('nvarchar', 'varchar', 'nchar', 'char', 'ntext', 'text')")
                .ToListAsync();
            Assert.Equal(["Action"], characterColumns);
            Assert.DoesNotContain(events, x => x.Action.Contains("Secret", StringComparison.Ordinal));
        });
    }

    [Fact]
    public async Task SqlDeniedAuditSummariesAreBoundedByActorCategoryAndUtcHourIncludingConcurrency()
    {
        if (!Enabled) return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var group = Group(db);
            var actor = Join(db, group, "Denied actor");
            var otherActor = Join(db, group, "Other denied actor");
            var coordinator = Join(db, group, "Coordinator");
            await db.SaveChangesAsync();
            var repository = Repo(db, clock);
            var hidden = (await repository.CreateAsync(coordinator.AccountId, group.Id,
                Create(actor.ProfileId, []), default))!.IncidentId;

            Assert.Null(await repository.GetAsync(actor.AccountId, Guid.NewGuid(), default));
            clock.Set(Start.AddMinutes(47));
            Assert.Null(await repository.GetAsync(actor.AccountId, hidden, default));
            var read = await db.IncidentAuditEvents.AsNoTracking()
                .SingleAsync(x => x.ActorAccountId == actor.AccountId && x.Action == IncidentAuditEvent.ReadDenied);
            Assert.Equal(((Guid?)null, false, (DateTimeOffset?)Start, (DateTimeOffset?)Start, (DateTimeOffset?)Start.AddMinutes(47), 2),
                (read.IncidentId, read.Authorized, read.UtcHourBucket, read.FirstAttemptAt, read.LastAttemptAt, read.DeniedAttemptCount));
            Assert.Equal(Start.AddDays(365), read.ExpiresAt);

            clock.Set(Start.AddMinutes(55));
            Assert.Null(await repository.CreateAsync(actor.AccountId, Guid.NewGuid(), Create(actor.ProfileId, []), default));
            Assert.Null(await repository.AddEntryAsync(actor.AccountId, hidden, Observation("Denied"), default));
            Assert.Null(await repository.ResolveAsync(actor.AccountId, Guid.NewGuid(), Resolve(), default));
            var write = await db.IncidentAuditEvents.AsNoTracking()
                .SingleAsync(x => x.ActorAccountId == actor.AccountId && x.Action == IncidentAuditEvent.WriteDenied);
            Assert.Equal(((Guid?)null, 3, (DateTimeOffset?)Start), (write.IncidentId, write.DeniedAttemptCount, write.UtcHourBucket));

            // The list never records a denial.
            var total = await db.IncidentAuditEvents.CountAsync();
            Assert.Empty(await repository.GetVisibleAsync(actor.AccountId, default));
            Assert.Equal(total, await db.IncidentAuditEvents.CountAsync());

            clock.Set(Start.AddHours(1));
            Assert.Null(await repository.GetAsync(actor.AccountId, Guid.NewGuid(), default));
            Assert.Null(await repository.GetAsync(otherActor.AccountId, Guid.NewGuid(), default));
            Assert.Equal(3, await db.IncidentAuditEvents.CountAsync(x => x.Action == IncidentAuditEvent.ReadDenied));

            clock.Set(Start.AddHours(2));
            var options = Options(db);
            await Task.WhenAll(Enumerable.Range(0, 16).Select(async i =>
            {
                await using var concurrentDb = new FamilySafetyDbContext(options);
                var concurrent = Repo(concurrentDb, clock);
                Assert.Null(i % 2 == 0
                    ? await concurrent.GetAsync(actor.AccountId, Guid.NewGuid(), default)
                    : await concurrent.GetAsync(actor.AccountId, hidden, default));
            }));
            await Task.WhenAll(Enumerable.Range(0, 8).Select(async _ =>
            {
                await using var concurrentDb = new FamilySafetyDbContext(options);
                Assert.Null(await Repo(concurrentDb, clock).AddEntryAsync(actor.AccountId, Guid.NewGuid(),
                    Observation("Denied"), default));
            }));
            var bucket = Start.AddHours(2);
            var concurrentRead = await db.IncidentAuditEvents.AsNoTracking().SingleAsync(x =>
                x.ActorAccountId == actor.AccountId && x.Action == IncidentAuditEvent.ReadDenied && x.UtcHourBucket == bucket);
            Assert.Equal(16, concurrentRead.DeniedAttemptCount);
            Assert.Null(concurrentRead.IncidentId);
            var concurrentWrite = await db.IncidentAuditEvents.AsNoTracking().SingleAsync(x =>
                x.ActorAccountId == actor.AccountId && x.Action == IncidentAuditEvent.WriteDenied && x.UtcHourBucket == bucket);
            Assert.Equal(8, concurrentWrite.DeniedAttemptCount);
            Assert.False(await db.IncidentAuditEvents.AnyAsync(x => x.DeniedAttemptCount > 0 && x.IncidentId != null));
            // Denied attempts never reach the Concern audit table.
            Assert.Equal(0, await db.ConcernAuditEvents.CountAsync());

            // Each summary expires 365 days after its first attempt (Start for reads, +55 minutes for writes).
            clock.Set(Start.AddDays(365).AddMinutes(55));
            await repository.ReconcileAsync(default);
            Assert.False(await db.IncidentAuditEvents.AnyAsync(x => x.ActorAccountId == actor.AccountId && x.UtcHourBucket == Start));
            Assert.True(await db.IncidentAuditEvents.AnyAsync(x => x.ActorAccountId == actor.AccountId
                && x.UtcHourBucket == Start.AddHours(1)));
        });
    }

    [Fact]
    public async Task SqlMembershipLossCommitSerializesAgainstIncidentWrite()
    {
        if (!Enabled) return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var group = Group(db);
            var coordinator = Join(db, group, "Coordinator");
            var participant = Join(db, group, "Participant");
            await db.SaveChangesAsync();
            var id = (await Repo(db, clock).CreateAsync(coordinator.AccountId, group.Id,
                Create(participant.ProfileId, [participant.ProfileId]), default))!.IncidentId;
            var options = Options(db);

            await using var endDb = new FamilySafetyDbContext(options);
            await using var endTransaction = await endDb.Database.BeginTransactionAsync();
            var toEnd = await endDb.FamilyGroupMemberships.SingleAsync(m => m.Id == participant.Membership.Id);
            toEnd.End(Start.AddMinutes(1));
            await endDb.SaveChangesAsync();

            clock.Set(Start.AddMinutes(2));
            var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var write = Task.Run(async () =>
            {
                await using var writeDb = new FamilySafetyDbContext(options);
                started.SetResult();
                return await Repo(writeDb, clock).AddEntryAsync(participant.AccountId, id,
                    Observation("Stale concurrent write"), default);
            });
            await started.Task;
            await Task.Delay(150);
            await endTransaction.CommitAsync();

            Assert.Null(await write.WaitAsync(TimeSpan.FromSeconds(15)));
            Assert.False(await db.IncidentEntries.AnyAsync(x => x.Text == "Stale concurrent write"));
            Assert.Equal(1, (await db.IncidentAuditEvents.AsNoTracking().SingleAsync(x =>
                x.ActorAccountId == participant.AccountId && x.Action == IncidentAuditEvent.WriteDenied)).DeniedAttemptCount);
        });
    }

    [Fact]
    public async Task SqlParticipantRemovalAndCoordinatorLossSerializeAgainstPendingCommands()
    {
        if (!Enabled) return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var group = Group(db);
            var coordinator = Join(db, group, "Coordinator");
            var participant = Join(db, group, "Participant");
            await db.SaveChangesAsync();
            var id = (await Repo(db, clock).CreateAsync(coordinator.AccountId, group.Id,
                Create(participant.ProfileId, [participant.ProfileId]), default))!.IncidentId;
            var options = Options(db);
            var grantId = await db.IncidentParticipantGrants.Where(g => g.IncidentId == id && g.ProfileId == participant.ProfileId)
                .Select(g => g.Id).SingleAsync();

            // A removal in flight (holding the Family Group lock) commits before the participant's write is evaluated.
            await using (var removalDb = new FamilySafetyDbContext(options))
            {
                await using var removal = await removalDb.Database.BeginTransactionAsync();
                await removalDb.FamilyGroups.Where(g => g.Id == group.Id)
                    .ExecuteUpdateAsync(update => update.SetProperty(g => g.CapacityLimit, g => g.CapacityLimit));
                await removalDb.IncidentParticipantGrants.Where(g => g.Id == grantId)
                    .ExecuteUpdateAsync(update => update.SetProperty(g => g.RemovedAt, Start.AddMinutes(1))
                        .SetProperty(g => g.RemovedByProfileId, coordinator.ProfileId));
                var write = Task.Run(async () =>
                {
                    await using var writeDb = new FamilySafetyDbContext(options);
                    return await Repo(writeDb, clock).AddEntryAsync(participant.AccountId, id, Observation("Racing write"), default);
                });
                await Task.Delay(200);
                Assert.False(write.IsCompleted);
                await removal.CommitAsync();
                Assert.Null(await write.WaitAsync(TimeSpan.FromSeconds(15)));
            }
            Assert.False(await db.IncidentEntries.AnyAsync(x => x.Text == "Racing write"));

            // Coordinator membership loss visible at commit denies the coordinator command and freezes at EndedAt.
            await using (var endDb = new FamilySafetyDbContext(options))
            {
                await using var ending = await endDb.Database.BeginTransactionAsync();
                var membership = await endDb.FamilyGroupMemberships.SingleAsync(m => m.Id == coordinator.Membership.Id);
                membership.End(Start.AddMinutes(3));
                await endDb.SaveChangesAsync();
                clock.Set(Start.AddMinutes(4));
                var resolve = Task.Run(async () =>
                {
                    await using var resolveDb = new FamilySafetyDbContext(options);
                    return await Repo(resolveDb, clock).ResolveAsync(coordinator.AccountId, id, Resolve(), default);
                });
                await Task.Delay(200);
                await ending.CommitAsync();
                Assert.Null(await resolve.WaitAsync(TimeSpan.FromSeconds(15)));
            }
            var stored = await db.Incidents.AsNoTracking().SingleAsync(i => i.Id == id);
            Assert.Equal((IncidentState.Frozen, (DateTimeOffset?)Start.AddMinutes(3), (IncidentFreezeReason?)IncidentFreezeReason.CoordinatorEligibilityLost),
                (stored.State, stored.FrozenAt, stored.FreezeReason));
            Assert.Null(stored.ResolvedAt);
        });
    }

    [Fact]
    public async Task SqlConcurrentTaskTransitionsAndHandoffsCommitExactlyOnce()
    {
        if (!Enabled) return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var group = Group(db);
            var a = Join(db, group, "Coordinator");
            var b = Join(db, group, "Participant B");
            var c = Join(db, group, "Participant C");
            await db.SaveChangesAsync();
            var repository = Repo(db, clock);
            var id = (await repository.CreateAsync(a.AccountId, group.Id,
                Create(b.ProfileId, [b.ProfileId, c.ProfileId]), default))!.IncidentId;
            var options = Options(db);

            async Task<IncidentView?> Run(Func<EfIncidentRepository, Task<IncidentView?>> action)
            {
                await using var context = new FamilySafetyDbContext(options);
                return await action(Repo(context, clock));
            }
            async Task<Guid> NewTaskAsync(string description) =>
                (await repository.CreateTaskAsync(a.AccountId, id, TaskFor(description, b.ProfileId), default))!
                    .Tasks.Single(t => t.Description == description).TaskId;

            // Complete vs cancel on one task: exactly one terminal transition.
            var raced = await NewTaskAsync("Raced");
            var outcomes = await Task.WhenAll(
                Run(r => r.CompleteTaskAsync(b.AccountId, id, raced, Complete(), default)),
                Run(r => r.CancelTaskAsync(a.AccountId, id, raced, Cancel(), default)));
            Assert.Single(outcomes, o => o is not null);
            var status = await db.IncidentTasks.AsNoTracking().Where(t => t.Id == raced).Select(t => t.Status).SingleAsync();
            Assert.Equal(outcomes[0] is not null ? IncidentTaskStatus.Done : IncidentTaskStatus.Cancelled, status);
            Assert.Equal(status == IncidentTaskStatus.Done ? 1 : 0,
                await db.IncidentEntries.CountAsync(x => x.RelatedTaskId == raced));

            // Double complete: one Done and one TaskResult.
            var twice = await NewTaskAsync("Twice");
            var completions = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ =>
                Run(r => r.CompleteTaskAsync(b.AccountId, id, twice, Complete(), default))));
            Assert.Single(completions, o => o is not null);
            Assert.Equal(1, await db.IncidentEntries.CountAsync(x => x.RelatedTaskId == twice));

            // Two handoffs by the same coordinator: exactly one coordinator change commits.
            var handoffs = await Task.WhenAll(
                Run(r => r.TransferCoordinatorAsync(a.AccountId, id, new TransferIncidentCoordinatorRequest(b.ProfileId), default)),
                Run(r => r.TransferCoordinatorAsync(a.AccountId, id, new TransferIncidentCoordinatorRequest(c.ProfileId), default)));
            Assert.Single(handoffs, o => o is not null);
            Assert.Equal(1, await db.IncidentCoordinatorChanges.CountAsync(x => x.IncidentId == id));
            var newCoordinator = handoffs.Single(o => o is not null)!.CoordinatorProfileId;
            var newCoordinatorAccount = newCoordinator == b.ProfileId ? b.AccountId : c.AccountId;

            // Handoff vs the old coordinator's command: never both.
            var nextTarget = newCoordinator == b.ProfileId ? c.ProfileId : b.ProfileId;
            var race = await Task.WhenAll(
                Run(r => r.TransferCoordinatorAsync(newCoordinatorAccount, id, new TransferIncidentCoordinatorRequest(nextTarget), default)),
                Run(r => r.ResolveAsync(newCoordinatorAccount, id, Resolve(), default)));
            Assert.Single(race, o => o is not null);
            var final = await repository.GetAsync(a.AccountId, id, default);
            Assert.Equal(race[1] is not null ? IncidentState.Resolved : IncidentState.Open, final!.State);
            Assert.Null(await repository.ReplaceParticipantsAsync(a.AccountId, id, new ReplaceIncidentParticipantsRequest([]), default));
        });
    }

    [Fact]
    public async Task SqlConcurrentReconciliationIsIdempotent()
    {
        if (!Enabled) return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var group = Group(db);
            var a = Join(db, group, "Coordinator");
            var b = Join(db, group, "Participant");
            await db.SaveChangesAsync();
            var repository = Repo(db, clock);
            var lost = (await repository.CreateAsync(a.AccountId, group.Id, Create(b.ProfileId, [b.ProfileId]), default))!.IncidentId;
            var expiring = (await repository.CreateAsync(b.AccountId, group.Id, Create(a.ProfileId, []), default))!.IncidentId;
            await repository.AddEntryAsync(b.AccountId, expiring, Observation("Will expire"), default);
            await repository.ResolveAsync(b.AccountId, expiring, Resolve(), default);
            a.Membership.End(Start.AddDays(2));
            await db.SaveChangesAsync();
            var options = Options(db);
            async Task SweepAsync()
            {
                await using var sweepDb = new FamilySafetyDbContext(options);
                await Repo(sweepDb, clock).ReconcileAsync(default);
            }

            clock.Set(Start.AddDays(3));
            await Task.WhenAll(SweepAsync(), SweepAsync(), SweepAsync());
            Assert.Equal(1, await db.IncidentAuditEvents.CountAsync(x => x.IncidentId == lost && x.Action == "Frozen"));
            Assert.Equal(1, await db.IncidentAuditEvents.CountAsync(x => x.IncidentId == lost && x.Action == "AccessRevoked"));
            Assert.Equal(IncidentState.Frozen, await db.Incidents.Where(i => i.Id == lost).Select(i => i.State).SingleAsync());

            clock.Set(Start.AddDays(366));
            await Task.WhenAll(SweepAsync(), SweepAsync(), SweepAsync());
            Assert.False(await db.Incidents.AnyAsync(i => i.Id == expiring));
            Assert.Equal(0, await db.IncidentEntries.CountAsync(x => x.IncidentId == expiring));
            Assert.Equal(0, await db.IncidentParticipantGrants.CountAsync(x => x.IncidentId == expiring));
            Assert.True(await db.Incidents.AnyAsync(i => i.Id == lost));
        });
    }

    [Fact]
    public async Task SqlMigrationAppliesAdditivelyOverExistingFamilyTripAndConcernData()
    {
        if (!Enabled) return;
        var connection = $"Server=localhost;Database=FamilySafety_WI008003_Migration_{Guid.NewGuid():N};Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
        var options = new DbContextOptionsBuilder<FamilySafetyDbContext>().UseSqlServer(connection).Options;
        var clock = new ManualClock(Start);
        await using var db = new FamilySafetyDbContext(options);
        var cleanupRequired = false;
        try
        {
            await db.Database.EnsureDeletedAsync();
            cleanupRequired = true;
            var migrator = db.GetService<IMigrator>();
            await migrator.MigrateAsync(PreviousMigration);
            Assert.Empty(await IncidentTablesAsync(db));

            var group = Group(db);
            var creator = Join(db, group, "Existing creator");
            var subject = Join(db, group, "Existing subject");
            db.Add(new FamilyTrip(Guid.NewGuid(), group.Id, creator.ProfileId, TripAudienceType.FamilyGroup, "Coast trip",
                "Seaside", new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 12), null, null, null, null, null, null));
            await db.SaveChangesAsync();
            var concerns = new EfConcernRepository(db, clock);
            var concern = await concerns.CreateAsync(creator.AccountId, group.Id,
                new CreateConcernRequest(subject.ProfileId, "Existing Concern", [subject.ProfileId]), default);
            await concerns.AddEntryAsync(subject.AccountId, concern!.ConcernId, new AddConcernEntryRequest(
                ConcernEntryKind.Observation, ConcernInformationCategory.Reported, "Existing entry", "Call", false,
                null, null, null), default);
            await concerns.GetAsync(Guid.NewGuid(), concern.ConcernId, default);

            var schemaBefore = await ExistingSchemaAsync(db);
            var dataBefore = await ExistingDataAsync(db, concern.ConcernId);

            await migrator.MigrateAsync("20261002013148_SafetyIncidentDomainAndAccessFoundation");
            Assert.Equal("20261002013148_SafetyIncidentDomainAndAccessFoundation",
                (await db.Database.GetAppliedMigrationsAsync()).Last());
            Assert.Equal(["IncidentAuditEvents", "IncidentCoordinatorChanges", "IncidentEntries",
                "IncidentParticipantGrants", "IncidentTasks", "Incidents"], await IncidentTablesAsync(db));
            Assert.Equal(schemaBefore, await ExistingSchemaAsync(db));
            Assert.Equal(dataBefore, await ExistingDataAsync(db, concern.ConcernId));

            // The upgraded database works end to end, including lineage to a pre-existing Concern.
            var incident = await Repo(db, clock).CreateAsync(creator.AccountId, group.Id,
                Create(subject.ProfileId, [subject.ProfileId], concern.ConcernId), default);
            Assert.Equal(concern.ConcernId, incident!.OriginatingConcernId);
            Assert.Equal(dataBefore, await ExistingDataAsync(db, concern.ConcernId));

            // Down removes only the Incident tables; existing data survives a round trip.
            await migrator.MigrateAsync(PreviousMigration);
            Assert.Empty(await IncidentTablesAsync(db));
            Assert.Equal(schemaBefore, await ExistingSchemaAsync(db));
            Assert.Equal(dataBefore, await ExistingDataAsync(db, concern.ConcernId));
            await migrator.MigrateAsync();
            Assert.Equal(6, (await IncidentTablesAsync(db)).Count);
        }
        finally
        {
            if (cleanupRequired)
            {
                await using var cleanup = new FamilySafetyDbContext(options);
                await cleanup.Database.EnsureDeletedAsync();
            }
        }
    }

    private static Task<List<string>> IncidentTablesAsync(FamilySafetyDbContext db) =>
        db.Database.SqlQueryRaw<string>(
            "SELECT TABLE_NAME AS [Value] FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE 'Incident%' ORDER BY TABLE_NAME COLLATE Latin1_General_BIN")
            .ToListAsync();

    private static async Task<List<string>> ExistingSchemaAsync(FamilySafetyDbContext db)
    {
        var columns = await db.Database.SqlQueryRaw<string>(@"
SELECT CONCAT(TABLE_NAME COLLATE DATABASE_DEFAULT, '.', COLUMN_NAME COLLATE DATABASE_DEFAULT, ':', DATA_TYPE COLLATE DATABASE_DEFAULT, ':', IS_NULLABLE COLLATE DATABASE_DEFAULT, ':', COALESCE(CAST(CHARACTER_MAXIMUM_LENGTH AS nvarchar(20)), ''), ':', COALESCE(COLUMN_DEFAULT COLLATE DATABASE_DEFAULT, '')) AS [Value]
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME NOT LIKE 'Incident%' AND TABLE_NAME <> '__EFMigrationsHistory'").ToListAsync();
        var indexes = await db.Database.SqlQueryRaw<string>(@"
SELECT CONCAT(t.name COLLATE DATABASE_DEFAULT, '.', i.name COLLATE DATABASE_DEFAULT, ':', i.is_unique, ':', COALESCE(i.filter_definition COLLATE DATABASE_DEFAULT, '')) AS [Value]
FROM sys.indexes AS i JOIN sys.tables AS t ON t.object_id = i.object_id
WHERE t.name NOT LIKE 'Incident%' AND t.name <> '__EFMigrationsHistory' AND i.name IS NOT NULL").ToListAsync();
        var foreignKeys = await db.Database.SqlQueryRaw<string>(@"
SELECT CONCAT(OBJECT_NAME(parent_object_id) COLLATE DATABASE_DEFAULT, '.', name COLLATE DATABASE_DEFAULT, ':', delete_referential_action_desc COLLATE DATABASE_DEFAULT) AS [Value]
FROM sys.foreign_keys WHERE OBJECT_NAME(parent_object_id) NOT LIKE 'Incident%'").ToListAsync();
        return [.. columns.Concat(indexes).Concat(foreignKeys).Order(StringComparer.Ordinal)];
    }

    private static async Task<string> ExistingDataAsync(FamilySafetyDbContext db, Guid concernId)
    {
        var concern = await db.Concerns.AsNoTracking().Where(c => c.Id == concernId)
            .Select(c => new { c.State, c.CreationReason, c.LastAuthorizedMutationAt, c.SubjectProfileId, c.RowVersion })
            .SingleAsync();
        return JsonSerializer.Serialize(new
        {
            concern,
            Groups = await db.FamilyGroups.CountAsync(),
            Memberships = await db.FamilyGroupMemberships.CountAsync(),
            Trips = await db.FamilyTrips.CountAsync(),
            ConcernGrants = await db.ConcernParticipantGrants.CountAsync(),
            ConcernEntries = await db.ConcernEntries.CountAsync(),
            ConcernAudit = await db.ConcernAuditEvents.CountAsync(),
        });
    }

    private static async Task<ConcernSnapshot> ConcernSnapshotAsync(FamilySafetyDbContext db, Guid concernId)
    {
        var row = await db.Concerns.AsNoTracking().Where(c => c.Id == concernId)
            .Select(c => new { c.State, c.LastAuthorizedMutationAt, c.FrozenAt, c.ResolvedAt }).SingleAsync();
        return new ConcernSnapshot(row.State, row.LastAuthorizedMutationAt, row.FrozenAt, row.ResolvedAt,
            await db.ConcernParticipantGrants.CountAsync(g => g.ConcernId == concernId && g.RemovedAt == null),
            await db.ConcernEntries.CountAsync(x => x.ConcernId == concernId),
            await db.ConcernAuditEvents.CountAsync());
    }

    private sealed record ConcernSnapshot(ConcernState State, DateTimeOffset LastAuthorizedMutationAt,
        DateTimeOffset? FrozenAt, DateTimeOffset? ResolvedAt, int Participants, int Entries, int Audit);
}
