using System.Collections.Concurrent;
using System.Data.Common;
using System.Text.Json;
using FamilySafety.Application.Incidents;
using FamilySafety.Domain.Incidents;
using FamilySafety.Domain.Profiles;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using static FamilySafety.Foundation.Tests.IncidentSqlFixture;

namespace FamilySafety.Foundation.Tests;

/// <summary>
/// SQL-gated Incident tests against a real SQL Server database with all migrations applied.
/// They return early (and report as passed) unless RUN_FAMILYSAFETY_SQL_TESTS=1.
/// </summary>
public sealed class IncidentPersistenceTests
{
    private static readonly string[] EntryAuditActions =
        ["ObservationCreated", "UnknownCreated", "SummaryCreated", "CorrectionCreated", "ChallengeCreated"];

    private static readonly string[] ProtectedSqlNames =
    [
        "ContactEmail", "ContactPhone", "Birthday", "FamilyTrip", "FamilyEvent", "FamilyPlan", "FamilyRelationship",
        "FamilyAdministratorAssignments", "MembershipInvitations",
    ];

    [Fact]
    public async Task SqlCreationEligibilityProvenanceAndNoImplicitAuthority()
    {
        if (!Enabled) return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var group = Group(db);
            var otherGroup = Group(db, "Other family");
            var creator = Join(db, group, "Creator");
            var subject = Join(db, group, "Subject");
            var participant = Join(db, group, "Participant");
            var administrator = Join(db, group, "Administrator");
            var inactive = Join(db, group, "Inactive", active: false);
            var otherGroupAdult = Join(db, otherGroup, "Other group adult");
            var nonmember = AddPerson(db, "Nonmember");
            var noProfileAccount = Guid.NewGuid();
            db.Add(new Account(noProfileAccount, $"{noProfileAccount:N}@example.invalid"));
            db.Add(new FamilyAdministratorAssignment(Guid.NewGuid(), administrator.Membership.Id));
            await db.SaveChangesAsync();
            var repository = Repo(db, clock);

            var created = await repository.CreateAsync(creator.AccountId, group.Id,
                Create(subject.ProfileId, [participant.ProfileId]), default);
            Assert.NotNull(created);
            Assert.Equal(IncidentState.Open, created.State);
            Assert.Equal((creator.ProfileId, "Creator"), (created.CoordinatorProfileId, created.CoordinatorName));
            Assert.Equal(creator.ProfileId, created.CreatedByProfileId);
            Assert.Equal((subject.ProfileId, "Subject"), (created.SubjectProfileId, created.SubjectName));
            Assert.Equal(new HashSet<Guid> { creator.ProfileId, participant.ProfileId },
                created.Participants.Select(p => p.ProfileId).ToHashSet());
            Assert.True(created.Participants.Single(p => p.ProfileId == creator.ProfileId).IsCoordinator);
            Assert.False(created.Participants.Single(p => p.ProfileId == participant.ProfileId).IsCoordinator);
            Assert.Null(created.OriginatingConcernId);
            var summary = Assert.Single(created.Entries);
            Assert.Equal(IncidentEntryKind.Summary, summary.Kind);
            Assert.Equal(IncidentInformationCategory.Reported, summary.Category);
            Assert.Equal(Reason, summary.Text);
            Assert.Equal("Phone call with a neighbour", summary.SourceDescription);
            Assert.Equal(creator.ProfileId, summary.ContributorProfileId);
            Assert.Equal(Start, summary.RecordedAt);

            var stored = await db.Incidents.AsNoTracking().SingleAsync(i => i.Id == created.IncidentId);
            var coordinatorGrant = await db.IncidentParticipantGrants.AsNoTracking()
                .SingleAsync(g => g.Id == stored.CoordinatorGrantId);
            Assert.Equal((creator.ProfileId, creator.Membership.Id, creator.Membership.AccessVersion),
                (coordinatorGrant.ProfileId, coordinatorGrant.MembershipId, coordinatorGrant.MembershipAccessVersion));
            var participantGrant = await db.IncidentParticipantGrants.AsNoTracking()
                .SingleAsync(g => g.IncidentId == created.IncidentId && g.ProfileId == participant.ProfileId);
            Assert.Equal((participant.Membership.Id, participant.Membership.AccessVersion),
                (participantGrant.MembershipId, participantGrant.MembershipAccessVersion));

            // Self-Incident is allowed; Administrator status neither adds nor removes eligibility.
            var self = await repository.CreateAsync(creator.AccountId, group.Id, Create(creator.ProfileId, []), default);
            Assert.Equal(creator.ProfileId, self!.SubjectProfileId);
            var byAdministrator = await repository.CreateAsync(administrator.AccountId, group.Id,
                Create(subject.ProfileId, []), default);
            Assert.NotNull(byAdministrator);

            var incidents = await db.Incidents.CountAsync();
            var grants = await db.IncidentParticipantGrants.CountAsync();
            var entries = await db.IncidentEntries.CountAsync();
            var subjectRequest = Create(subject.ProfileId, []);
            Assert.Null(await repository.CreateAsync(otherGroupAdult.AccountId, group.Id, subjectRequest, default));
            Assert.Null(await repository.CreateAsync(nonmember.AccountId, group.Id, subjectRequest, default));
            Assert.Null(await repository.CreateAsync(inactive.AccountId, group.Id, subjectRequest, default));
            Assert.Null(await repository.CreateAsync(noProfileAccount, group.Id, subjectRequest, default));
            Assert.Null(await repository.CreateAsync(creator.AccountId, Guid.NewGuid(), subjectRequest, default));
            Assert.Null(await repository.CreateAsync(creator.AccountId, group.Id, Create(inactive.ProfileId, []), default));
            Assert.Null(await repository.CreateAsync(creator.AccountId, group.Id, Create(otherGroupAdult.ProfileId, []), default));
            Assert.Null(await repository.CreateAsync(creator.AccountId, group.Id, Create(nonmember.ProfileId, []), default));
            Assert.Null(await repository.CreateAsync(creator.AccountId, group.Id, Create(Guid.NewGuid(), []), default));
            Assert.Null(await repository.CreateAsync(creator.AccountId, group.Id,
                Create(subject.ProfileId, [participant.ProfileId, inactive.ProfileId]), default));
            Assert.Null(await repository.CreateAsync(creator.AccountId, group.Id,
                Create(subject.ProfileId, [participant.ProfileId, otherGroupAdult.ProfileId]), default));
            Assert.Null(await repository.CreateAsync(creator.AccountId, group.Id,
                Create(subject.ProfileId, [creator.ProfileId]), default));
            // Any invalid request rejects the whole creation atomically: no partial records.
            Assert.Equal(incidents, await db.Incidents.CountAsync());
            Assert.Equal(grants, await db.IncidentParticipantGrants.CountAsync());
            Assert.Equal(entries, await db.IncidentEntries.CountAsync());

            // Subject status, membership alone, Administrator, and other groups grant no access.
            foreach (var denied in new[]
            {
                subject.AccountId, administrator.AccountId, otherGroupAdult.AccountId, nonmember.AccountId,
                noProfileAccount, inactive.AccountId,
            })
            {
                Assert.Null(await repository.GetAsync(denied, created.IncidentId, default));
                Assert.DoesNotContain(await repository.GetVisibleAsync(denied, default),
                    item => item.IncidentId == created.IncidentId);
            }
            var listed = Assert.Single(await repository.GetVisibleAsync(participant.AccountId, default));
            Assert.Equal((created.IncidentId, "Subject", "Creator", IncidentState.Open),
                (listed.IncidentId, listed.SubjectName, listed.CoordinatorName, listed.State));
            Assert.NotNull(await repository.GetAsync(participant.AccountId, created.IncidentId, default));
            Assert.Equal(2, (await repository.GetVisibleAsync(creator.AccountId, default)).Count);
            Assert.Equal(byAdministrator.IncidentId,
                Assert.Single(await repository.GetVisibleAsync(administrator.AccountId, default)).IncidentId);
            Assert.Equal(3, await db.IncidentAuditEvents.CountAsync(a => a.Action == "IncidentCreated" && a.Authorized));
            Assert.Equal(1, await db.IncidentAuditEvents.CountAsync(a => a.Action == "ParticipantAdded"));
        });
    }

    [Fact]
    public async Task SqlUnavailableResultIsUniformForReadsCommandsAndNestedIds()
    {
        if (!Enabled) return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var group = Group(db);
            var coordinator = Join(db, group, "Coordinator");
            var participant = Join(db, group, "Participant");
            var outsider = Join(db, group, "Outsider");
            await db.SaveChangesAsync();
            var repository = Repo(db, clock);
            var first = await repository.CreateAsync(coordinator.AccountId, group.Id,
                Create(outsider.ProfileId, [participant.ProfileId]), default);
            var second = await repository.CreateAsync(coordinator.AccountId, group.Id,
                Create(outsider.ProfileId, []), default);
            var withEntry = await repository.AddEntryAsync(coordinator.AccountId, second!.IncidentId,
                Observation("Second Incident detail"), default);
            var foreignEntry = withEntry!.Entries.Single(e => e.Text == "Second Incident detail").EntryId;
            var withTask = await repository.CreateTaskAsync(coordinator.AccountId, second.IncidentId,
                TaskFor("Second Incident task", coordinator.ProfileId), default);
            var foreignTask = Assert.Single(withTask!.Tasks).TaskId;
            var firstEntries = await db.IncidentEntries.CountAsync(e => e.IncidentId == first!.IncidentId);

            async Task AssertAllCommandsUnavailable(Guid actor, Guid incidentId, Guid taskId)
            {
                Assert.Null(await repository.GetAsync(actor, incidentId, default));
                Assert.Null(await repository.ReplaceParticipantsAsync(actor, incidentId,
                    new ReplaceIncidentParticipantsRequest([]), default));
                Assert.Null(await repository.TransferCoordinatorAsync(actor, incidentId,
                    new TransferIncidentCoordinatorRequest(participant.ProfileId), default));
                Assert.Null(await repository.AddEntryAsync(actor, incidentId, Observation("Denied"), default));
                Assert.Null(await repository.CreateTaskAsync(actor, incidentId, TaskFor("Denied", outsider.ProfileId), default));
                Assert.Null(await repository.CompleteTaskAsync(actor, incidentId, taskId, Complete(), default));
                Assert.Null(await repository.CancelTaskAsync(actor, incidentId, taskId, Cancel(), default));
                Assert.Null(await repository.ResolveAsync(actor, incidentId, Resolve(), default));
            }

            await AssertAllCommandsUnavailable(outsider.AccountId, first!.IncidentId, foreignTask);
            await AssertAllCommandsUnavailable(coordinator.AccountId, Guid.NewGuid(), Guid.NewGuid());
            await AssertAllCommandsUnavailable(Guid.NewGuid(), first.IncidentId, Guid.NewGuid());

            // Nested IDs from another Incident are unavailable even to an authorized participant.
            Assert.Null(await repository.CompleteTaskAsync(coordinator.AccountId, first.IncidentId, foreignTask, Complete(), default));
            Assert.Null(await repository.CancelTaskAsync(coordinator.AccountId, first.IncidentId, foreignTask, Cancel(), default));
            Assert.Null(await repository.AddEntryAsync(participant.AccountId, first.IncidentId,
                Challenge(foreignEntry), default));
            Assert.Null(await repository.AddEntryAsync(coordinator.AccountId, first.IncidentId,
                Correction(foreignEntry), default));
            Assert.Equal(firstEntries, await db.IncidentEntries.CountAsync(e => e.IncidentId == first.IncidentId));
            Assert.Equal(IncidentTaskStatus.Open,
                await db.IncidentTasks.Where(t => t.Id == foreignTask).Select(t => t.Status).SingleAsync());

            // Denials are summarized without any Incident ID, and nothing else is disclosed in audit.
            Assert.True(await db.IncidentAuditEvents.AnyAsync(a => a.Action == IncidentAuditEvent.ReadDenied));
            Assert.True(await db.IncidentAuditEvents.AnyAsync(a => a.Action == IncidentAuditEvent.WriteDenied));
            Assert.False(await db.IncidentAuditEvents.AnyAsync(a => !a.Authorized && a.DeniedAttemptCount > 0
                && a.IncidentId != null));
        });
    }

    [Fact]
    public async Task SqlParticipantManagementRemovalLeaveAndRejoin()
    {
        if (!Enabled) return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var group = Group(db);
            var coordinator = Join(db, group, "Coordinator");
            var subject = Join(db, group, "Subject");
            var b = Join(db, group, "Participant B");
            var c = Join(db, group, "Participant C");
            var d = Join(db, group, "Member D");
            await db.SaveChangesAsync();
            var repository = Repo(db, clock);
            var created = await repository.CreateAsync(coordinator.AccountId, group.Id,
                Create(subject.ProfileId, [b.ProfileId, c.ProfileId]), default);
            var id = created!.IncidentId;
            var oldGrantB = await GrantIdAsync(db, id, b.ProfileId);

            Assert.Null(await repository.ReplaceParticipantsAsync(b.AccountId, id,
                new ReplaceIncidentParticipantsRequest([b.ProfileId]), default));
            Assert.Null(await repository.ReplaceParticipantsAsync(coordinator.AccountId, id,
                new ReplaceIncidentParticipantsRequest([coordinator.ProfileId, b.ProfileId]), default));
            Assert.Null(await repository.ReplaceParticipantsAsync(coordinator.AccountId, id,
                new ReplaceIncidentParticipantsRequest([c.ProfileId, Guid.NewGuid()]), default));

            clock.Set(Start.AddMinutes(5));
            var replaced = await repository.ReplaceParticipantsAsync(coordinator.AccountId, id,
                new ReplaceIncidentParticipantsRequest([c.ProfileId, d.ProfileId]), default);
            Assert.Equal(new HashSet<Guid> { coordinator.ProfileId, c.ProfileId, d.ProfileId },
                replaced!.Participants.Select(p => p.ProfileId).ToHashSet());
            Assert.Null(await repository.GetAsync(b.AccountId, id, default));
            Assert.Null(await repository.AddEntryAsync(b.AccountId, id, Observation("Removed write"), default));
            Assert.NotNull(await repository.GetAsync(d.AccountId, id, default));
            Assert.Equal(Start.AddMinutes(5), replaced.LastAuthorizedMutationAt);

            // No request can remove the coordinator's own grant.
            var onlyCoordinator = await repository.ReplaceParticipantsAsync(coordinator.AccountId, id,
                new ReplaceIncidentParticipantsRequest([]), default);
            var remaining = Assert.Single(onlyCoordinator!.Participants);
            Assert.Equal(coordinator.ProfileId, remaining.ProfileId);
            Assert.True(remaining.IsCoordinator);

            // Re-selecting a removed participant creates a new grant.
            var readded = await repository.ReplaceParticipantsAsync(coordinator.AccountId, id,
                new ReplaceIncidentParticipantsRequest([b.ProfileId]), default);
            Assert.Contains(readded!.Participants, p => p.ProfileId == b.ProfileId);
            var newGrantB = await GrantIdAsync(db, id, b.ProfileId);
            Assert.NotEqual(oldGrantB, newGrantB);
            await repository.AddEntryAsync(b.AccountId, id, Observation("Before leaving"), default);

            // Leaving the Family ends access immediately; rejoining restores nothing.
            var oldVersion = b.Membership.AccessVersion;
            b.Membership.End(Start.AddHours(1));
            await db.SaveChangesAsync();
            clock.Set(Start.AddHours(2));
            Assert.Null(await repository.GetAsync(b.AccountId, id, default));
            Assert.Null(await repository.AddEntryAsync(b.AccountId, id, Observation("Stale write"), default));
            Assert.Empty(await repository.GetVisibleAsync(b.AccountId, default));
            await db.FamilyGroupMemberships.Where(m => m.Id == b.Membership.Id)
                .ExecuteUpdateAsync(update => update.SetProperty(m => m.IsActive, true));
            db.ChangeTracker.Clear();
            Assert.Null(await repository.GetAsync(b.AccountId, id, default));
            Assert.Null(await repository.AddEntryAsync(b.AccountId, id, Observation("Rejoined stale write"), default));

            var coordinatorView = await repository.GetAsync(coordinator.AccountId, id, default);
            Assert.DoesNotContain(coordinatorView!.Participants, p => p.ProfileId == b.ProfileId);
            var priorEntry = Assert.Single(coordinatorView.Entries, e => e.Text == "Before leaving");
            Assert.Equal("Participant B", priorEntry.ContributorName);
            var ended = Assert.Single(coordinatorView.Timeline, t => t.Type == IncidentTimelineItemType.ParticipantAccessEnded);
            Assert.Equal((b.ProfileId, Start.AddHours(1), "System"), (ended.AffectedProfileId, ended.RecordedAt, ended.ActorName));

            var reselected = await repository.ReplaceParticipantsAsync(coordinator.AccountId, id,
                new ReplaceIncidentParticipantsRequest([b.ProfileId]), default);
            Assert.Contains(reselected!.Participants, p => p.ProfileId == b.ProfileId);
            var freshGrant = await db.IncidentParticipantGrants.AsNoTracking()
                .SingleAsync(g => g.IncidentId == id && g.ProfileId == b.ProfileId && g.RemovedAt == null);
            Assert.Equal(oldVersion + 1, freshGrant.MembershipAccessVersion);
            Assert.NotNull(await repository.GetAsync(b.AccountId, id, default));

            await repository.ReconcileAsync(default);
            Assert.Equal(1, await db.IncidentAuditEvents.CountAsync(a => a.Action == "AccessRevoked"
                && a.AffectedProfileId == b.ProfileId));
            Assert.True(await db.IncidentAuditEvents.AnyAsync(a => a.Action == "ParticipantRemoved"
                && a.AffectedProfileId == b.ProfileId && a.ActorAccountId == coordinator.AccountId));
        });
    }

    [Fact]
    public async Task SqlCoordinatorHandoffMovesAuthorityExactlyOnce()
    {
        if (!Enabled) return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var group = Group(db);
            var a = Join(db, group, "Creator A");
            var b = Join(db, group, "Participant B");
            var c = Join(db, group, "Participant C");
            var d = Join(db, group, "Member D");
            var e = Join(db, group, "Participant E");
            await db.SaveChangesAsync();
            var repository = Repo(db, clock);
            var created = await repository.CreateAsync(a.AccountId, group.Id,
                Create(d.ProfileId, [b.ProfileId, c.ProfileId, e.ProfileId]), default);
            var id = created!.IncidentId;

            Assert.Null(await Transfer(repository, a, id, a.ProfileId));
            Assert.Null(await Transfer(repository, a, id, d.ProfileId));
            Assert.Null(await Transfer(repository, b, id, c.ProfileId));
            e.Membership.End(Start.AddMinutes(1));
            await db.SaveChangesAsync();
            Assert.Null(await Transfer(repository, a, id, e.ProfileId));
            await repository.ReplaceParticipantsAsync(a.AccountId, id,
                new ReplaceIncidentParticipantsRequest([b.ProfileId]), default);
            Assert.Null(await Transfer(repository, a, id, c.ProfileId));

            clock.Set(Start.AddMinutes(10));
            var handedOff = await Transfer(repository, a, id, b.ProfileId);
            Assert.Equal(b.ProfileId, handedOff!.CoordinatorProfileId);
            Assert.True(handedOff.Participants.Single(p => p.ProfileId == b.ProfileId).IsCoordinator);
            Assert.False(handedOff.Participants.Single(p => p.ProfileId == a.ProfileId).IsCoordinator);
            Assert.Equal(a.ProfileId, handedOff.CreatedByProfileId);
            var change = await db.IncidentCoordinatorChanges.AsNoTracking().SingleAsync(x => x.IncidentId == id);
            Assert.Equal((a.ProfileId, b.ProfileId, a.AccountId, Start.AddMinutes(10)),
                (change.FromProfileId, change.ToProfileId, change.ChangedByAccountId, change.ChangedAt));
            var audit = await db.IncidentAuditEvents.AsNoTracking().SingleAsync(x => x.Action == "CoordinatorTransferred");
            Assert.Equal((b.ProfileId, a.ProfileId, id), (audit.AffectedProfileId, audit.FromProfileId, audit.IncidentId));
            Assert.Single(handedOff.Timeline, t => t.Type == IncidentTimelineItemType.CoordinatorHandedOff
                && t.ActorProfileId == a.ProfileId && t.AffectedProfileId == b.ProfileId);

            // The former coordinator (also the creator) keeps only participant rights.
            Assert.Null(await repository.ReplaceParticipantsAsync(a.AccountId, id,
                new ReplaceIncidentParticipantsRequest([]), default));
            Assert.Null(await Transfer(repository, a, id, b.ProfileId));
            Assert.Null(await repository.ResolveAsync(a.AccountId, id, Resolve(), default));
            Assert.Null(await repository.AddEntryAsync(a.AccountId, id, SummaryEntry("Old coordinator summary"), default));
            Assert.Null(await repository.CreateTaskAsync(a.AccountId, id, TaskFor("For B", b.ProfileId), default));
            Assert.NotNull(await repository.AddEntryAsync(a.AccountId, id, Observation("Former coordinator observation"), default));
            Assert.NotNull(await repository.CreateTaskAsync(a.AccountId, id, TaskFor("Self task", a.ProfileId), default));

            // The new coordinator's powers apply immediately.
            Assert.NotNull(await repository.AddEntryAsync(b.AccountId, id, SummaryEntry("New coordinator summary"), default));
            Assert.NotNull(await repository.CreateTaskAsync(b.AccountId, id, TaskFor("For A", a.ProfileId), default));
            var reduced = await repository.ReplaceParticipantsAsync(b.AccountId, id,
                new ReplaceIncidentParticipantsRequest([c.ProfileId]), default);
            Assert.DoesNotContain(reduced!.Participants, p => p.ProfileId == a.ProfileId);
            // The creator retains no authority and no access once removed.
            Assert.Null(await repository.GetAsync(a.AccountId, id, default));
            Assert.Equal(1, await db.IncidentCoordinatorChanges.CountAsync());
        });
    }

    [Fact]
    public async Task SqlCoordinatorLossFreezesAtLossTimeWithoutPromotion()
    {
        if (!Enabled) return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var group = Group(db);
            var coordinator = Join(db, group, "Coordinator");
            var participant = Join(db, group, "Participant");
            var subject = Join(db, group, "Subject");
            await db.SaveChangesAsync();
            var repository = Repo(db, clock);
            var created = await repository.CreateAsync(coordinator.AccountId, group.Id,
                Create(subject.ProfileId, [participant.ProfileId]), default);
            var id = created!.IncidentId;

            var lossAt = Start.AddDays(1);
            coordinator.Membership.End(lossAt);
            await db.SaveChangesAsync();
            clock.Set(Start.AddDays(2));
            Assert.Null(await repository.GetAsync(coordinator.AccountId, id, default));
            var frozen = await repository.GetAsync(participant.AccountId, id, default);
            Assert.Equal(IncidentState.Frozen, frozen!.State);
            Assert.Equal(lossAt, frozen.FrozenAt);
            Assert.Equal(IncidentFreezeReason.CoordinatorEligibilityLost, frozen.FreezeReason);
            Assert.Equal(lossAt.AddDays(365), frozen.ExpiresAt);
            Assert.Null(frozen.Resolution);
            // No promotion: the coordinator of record is unchanged and the participant is not promoted.
            Assert.Equal(coordinator.ProfileId, frozen.CoordinatorProfileId);
            var remaining = Assert.Single(frozen.Participants);
            Assert.False(remaining.IsCoordinator);

            Assert.Null(await repository.AddEntryAsync(participant.AccountId, id, Observation("Read-only"), default));
            Assert.Null(await repository.CreateTaskAsync(participant.AccountId, id,
                TaskFor("Read-only", participant.ProfileId), default));
            Assert.Null(await repository.ResolveAsync(participant.AccountId, id, Resolve(), default));
            Assert.Null(await Transfer(repository, participant, id, participant.ProfileId));
            Assert.Null(await repository.ReplaceParticipantsAsync(participant.AccountId, id,
                new ReplaceIncidentParticipantsRequest([]), default));

            await repository.ReconcileAsync(default);
            await repository.ReconcileAsync(default);
            var frozenEvent = await db.IncidentAuditEvents.AsNoTracking().SingleAsync(a => a.Action == "Frozen");
            Assert.Equal((id, lossAt, (Guid?)null), (frozenEvent.IncidentId!.Value, frozenEvent.OccurredAt, frozenEvent.ActorAccountId));
            var revoked = await db.IncidentAuditEvents.AsNoTracking().SingleAsync(a => a.Action == "AccessRevoked");
            Assert.Equal((coordinator.ProfileId, lossAt), (revoked.AffectedProfileId!.Value, revoked.OccurredAt));

            await db.FamilyGroupMemberships.Where(m => m.Id == coordinator.Membership.Id)
                .ExecuteUpdateAsync(update => update.SetProperty(m => m.IsActive, true));
            db.ChangeTracker.Clear();
            Assert.Null(await repository.GetAsync(coordinator.AccountId, id, default));
            Assert.Equal(IncidentState.Frozen, (await repository.GetAsync(participant.AccountId, id, default))!.State);
        });

        // An earlier inactivity deadline keeps its reason and time even with no normalization in between.
        await WithDatabaseAsync(async (db, clock) =>
        {
            var group = Group(db);
            var coordinator = Join(db, group, "Coordinator");
            var participant = Join(db, group, "Participant");
            await db.SaveChangesAsync();
            var repository = Repo(db, clock);
            var created = await repository.CreateAsync(coordinator.AccountId, group.Id,
                Create(participant.ProfileId, [participant.ProfileId]), default);
            coordinator.Membership.End(Start.AddDays(40));
            await db.SaveChangesAsync();
            clock.Set(Start.AddDays(41));
            var frozen = await repository.GetAsync(participant.AccountId, created!.IncidentId, default);
            Assert.Equal((IncidentState.Frozen, Start.AddDays(30), IncidentFreezeReason.Inactivity),
                (frozen!.State, frozen.FrozenAt!.Value, frozen.FreezeReason!.Value));
            Assert.True(await db.IncidentAuditEvents.AnyAsync(a => a.Action == "AccessRevoked"
                && a.AffectedProfileId == coordinator.ProfileId && a.OccurredAt == Start.AddDays(40)));
        });
    }

    [Fact]
    public async Task SqlSubjectMembershipLossLeavesIncidentAndOtherAccessIntact()
    {
        if (!Enabled) return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var group = Group(db);
            var coordinator = Join(db, group, "Coordinator");
            var subject = Join(db, group, "Subject");
            var participant = Join(db, group, "Participant");
            await db.SaveChangesAsync();
            var repository = Repo(db, clock);
            var withSubject = await repository.CreateAsync(coordinator.AccountId, group.Id,
                Create(subject.ProfileId, [subject.ProfileId, participant.ProfileId]), default);
            var withoutSubject = await repository.CreateAsync(coordinator.AccountId, group.Id,
                Create(subject.ProfileId, [participant.ProfileId]), default);
            Assert.NotNull(await repository.GetAsync(subject.AccountId, withSubject!.IncidentId, default));
            Assert.Null(await repository.GetAsync(subject.AccountId, withoutSubject!.IncidentId, default));

            subject.Membership.End(Start.AddHours(1));
            await db.SaveChangesAsync();
            clock.Set(Start.AddHours(2));
            Assert.Null(await repository.GetAsync(subject.AccountId, withSubject.IncidentId, default));
            Assert.Empty(await repository.GetVisibleAsync(subject.AccountId, default));
            foreach (var id in new[] { withSubject.IncidentId, withoutSubject.IncidentId })
            {
                var view = await repository.GetAsync(participant.AccountId, id, default);
                Assert.Equal(IncidentState.Open, view!.State);
                Assert.Equal((subject.ProfileId, "Subject"), (view.SubjectProfileId, view.SubjectName));
                Assert.Equal(new HashSet<Guid> { coordinator.ProfileId, participant.ProfileId },
                    view.Participants.Select(p => p.ProfileId).ToHashSet());
                Assert.Equal(coordinator.ProfileId, view.CoordinatorProfileId);
            }
            Assert.Equal(4, await db.IncidentParticipantGrants.CountAsync(g => g.RemovedAt == null));

            await db.FamilyGroupMemberships.Where(m => m.Id == subject.Membership.Id)
                .ExecuteUpdateAsync(update => update.SetProperty(m => m.IsActive, true));
            db.ChangeTracker.Clear();
            Assert.Null(await repository.GetAsync(subject.AccountId, withSubject.IncidentId, default));
        });
    }

    [Fact]
    public async Task SqlEntriesPreserveProvenanceWithAppendOnlyCorrectionsAndChallenges()
    {
        if (!Enabled) return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var group = Group(db);
            var coordinator = Join(db, group, "Coordinator");
            var participant = Join(db, group, "Participant");
            await db.SaveChangesAsync();
            var repository = Repo(db, clock);
            var id = (await repository.CreateAsync(coordinator.AccountId, group.Id,
                Create(participant.ProfileId, [participant.ProfileId]), default))!.IncidentId;

            clock.Set(Start.AddMinutes(1));
            var eventAt = Start.AddHours(-3);
            var observed = await repository.AddEntryAsync(participant.AccountId, id,
                Observation("Seen leaving work") with { EventAt = eventAt }, default);
            var observation = observed!.Entries.Single(e => e.Text == "Seen leaving work");
            Assert.Equal((IncidentEntryKind.Observation, IncidentInformationCategory.Reported, false),
                (observation.Kind, observation.Category, observation.IsOutdated));
            Assert.Equal((Start.AddMinutes(1), (DateTimeOffset?)eventAt, "Direct conversation", participant.ProfileId),
                (observation.RecordedAt, observation.EventAt, observation.SourceDescription, observation.ContributorProfileId));

            var withUnknown = await repository.AddEntryAsync(participant.AccountId, id, Unknown("Arrival time unknown"), default);
            var unknown = withUnknown!.Entries.Single(e => e.Kind == IncidentEntryKind.Unknown);
            Assert.True(unknown.SourceUnknown);
            Assert.Null(unknown.SourceDescription);
            Assert.Equal(IncidentInformationCategory.Unknown, unknown.Category);

            var withOutdated = await repository.AddEntryAsync(participant.AccountId, id, Observation("Earlier sighting") with
            { Category = IncidentInformationCategory.Outdated, OriginalCategory = IncidentInformationCategory.Confirmed }, default);
            var outdated = withOutdated!.Entries.Single(e => e.Text == "Earlier sighting");
            Assert.Equal((IncidentInformationCategory.Confirmed, true), (outdated.Category, outdated.IsOutdated));

            Assert.Null(await repository.AddEntryAsync(participant.AccountId, id, SummaryEntry("Participant summary"), default));
            Assert.NotNull(await repository.AddEntryAsync(coordinator.AccountId, id, SummaryEntry("Current summary"), default));

            var corrected = await repository.AddEntryAsync(participant.AccountId, id, Correction(unknown.EntryId), default);
            Assert.Contains(corrected!.Entries, e => e.Kind == IncidentEntryKind.Correction && e.RelatedEntryId == unknown.EntryId
                && e.Explanation == "Correcting my earlier entry");
            var coordinatorEntry = corrected.Entries.Single(e => e.Text == "Current summary").EntryId;
            Assert.Null(await repository.AddEntryAsync(participant.AccountId, id, Correction(coordinatorEntry), default));
            Assert.Null(await repository.AddEntryAsync(participant.AccountId, id, Challenge(unknown.EntryId), default));
            var challenged = await repository.AddEntryAsync(coordinator.AccountId, id, Challenge(observation.EntryId), default);
            Assert.Contains(challenged!.Entries, e => e.Kind == IncidentEntryKind.Challenge
                && e.RelatedEntryId == observation.EntryId && e.ContributorProfileId == coordinator.ProfileId);

            // Targets are unchanged: corrections and challenges are separate linked records.
            var original = challenged.Entries.Single(e => e.EntryId == observation.EntryId);
            Assert.Equal(("Seen leaving work", IncidentInformationCategory.Reported), (original.Text, original.Category));
            var originalUnknown = challenged.Entries.Single(e => e.EntryId == unknown.EntryId);
            Assert.Equal("Arrival time unknown", originalUnknown.Text);
            Assert.Equal(7, challenged.Entries.Count);

            var actions = await db.IncidentAuditEvents.Where(a => a.IncidentId == id).Select(a => a.Action).ToListAsync();
            foreach (var expected in EntryAuditActions)
                Assert.Contains(expected, actions);
            Assert.DoesNotContain(actions, action => action.Contains("leaving", StringComparison.OrdinalIgnoreCase));
            var entryAudit = await db.IncidentAuditEvents.AsNoTracking()
                .SingleAsync(a => a.Action == "ChallengeCreated");
            Assert.NotNull(entryAudit.EntryId);
        });
    }

    [Fact]
    public async Task SqlContactAttemptsAreRecordOnlyAndPullNoContactDetails()
    {
        if (!Enabled) return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var group = Group(db);
            var coordinator = Join(db, group, "Coordinator");
            var subject = Join(db, group, "Subject");
            await db.SaveChangesAsync();
            var subjectProfile = await db.AdultProfiles.SingleAsync(p => p.Id == subject.ProfileId);
            subjectProfile.Update("Subject", "subject.private@example.invalid", ProfileFieldVisibility.ActiveFamilyGroup,
                "+1-555-0100", ProfileFieldVisibility.ActiveFamilyGroup, 4, 12, ProfileFieldVisibility.ActiveFamilyGroup);
            await db.SaveChangesAsync();
            var repository = Repo(db, clock);
            var id = (await repository.CreateAsync(coordinator.AccountId, group.Id,
                Create(subject.ProfileId, []), default))!.IncidentId;

            var attempt = new AddIncidentEntryRequest(IncidentEntryKind.ContactAttempt, IncidentInformationCategory.Confirmed,
                "Rang twice", "My own call log", false, EventAt: Start.AddMinutes(-20), TargetLabel: "Sam's mobile",
                ContactMethod: IncidentContactMethod.Call, ContactOutcome: IncidentContactOutcome.NoAnswer);
            var first = await repository.AddEntryAsync(coordinator.AccountId, id, attempt, default);
            var reported = await repository.AddEntryAsync(coordinator.AccountId, id, attempt with
            {
                Category = IncidentInformationCategory.Reported, Text = null, SourceDescription = "Told by Jo",
                EventAt = null, EventTimeUnknown = true, AttemptedByDescription = "Neighbour Jo",
                TargetLabel = "Sam", ContactMethod = IncidentContactMethod.InPerson,
                ContactOutcome = IncidentContactOutcome.Reached,
            }, default);

            // A Reached outcome changes no state.
            Assert.Equal(IncidentState.Open, reported!.State);
            Assert.Null(reported.Resolution);
            var attempts = reported.Entries.Where(e => e.Kind == IncidentEntryKind.ContactAttempt).ToList();
            Assert.Equal(2, attempts.Count);
            var own = attempts.Single(e => e.ContactOutcome == IncidentContactOutcome.NoAnswer);
            Assert.Equal(((string?)null, "Sam's mobile", IncidentContactMethod.Call, "Rang twice", (DateTimeOffset?)Start.AddMinutes(-20)),
                (own.AttemptedByDescription, own.TargetLabel, own.ContactMethod!.Value, own.Text, own.EventAt));
            var described = attempts.Single(e => e.ContactOutcome == IncidentContactOutcome.Reached);
            Assert.Equal(("Neighbour Jo", true, (DateTimeOffset?)null, (string?)null),
                (described.AttemptedByDescription, described.EventTimeUnknown, described.EventAt, described.Text));

            // Nothing from the profile's contact fields is stored or returned.
            var json = JsonSerializer.Serialize(reported);
            Assert.DoesNotContain("555-0100", json, StringComparison.Ordinal);
            Assert.DoesNotContain("subject.private@", json, StringComparison.Ordinal);
            var stored = await db.IncidentEntries.AsNoTracking().Where(e => e.IncidentId == id).ToListAsync();
            Assert.DoesNotContain(stored, e => (e.TargetLabel ?? string.Empty).Contains("555", StringComparison.Ordinal)
                || (e.Text ?? string.Empty).Contains('@', StringComparison.Ordinal));
            Assert.Equal(2, await db.IncidentAuditEvents.CountAsync(a => a.Action == "ContactAttemptRecorded"));
            Assert.NotNull(first);

            // Every SQL statement the Incident repository issues avoids protected profile fields and other domains.
            var capture = new SqlCommandCapture();
            await using var observedDb = new FamilySafetyDbContext(new DbContextOptionsBuilder<FamilySafetyDbContext>()
                .UseSqlServer(db.Database.GetDbConnection().ConnectionString).AddInterceptors(capture).Options);
            var observed = Repo(observedDb, clock);
            Assert.NotNull(await observed.CreateAsync(coordinator.AccountId, group.Id, Create(subject.ProfileId, [subject.ProfileId]), default));
            Assert.NotNull(await observed.AddEntryAsync(coordinator.AccountId, id, attempt, default));
            Assert.NotEmpty(await observed.GetVisibleAsync(subject.AccountId, default));
            Assert.NotNull(await observed.GetAsync(coordinator.AccountId, id, default));
            await observed.ReconcileAsync(default);
            Assert.Contains(capture.Commands, sql => sql.Contains("[AdultProfiles]", StringComparison.Ordinal));
            Assert.DoesNotContain(capture.Commands, sql => ProtectedSqlNames.Any(word => sql.Contains(word, StringComparison.Ordinal)));
        });
    }

    [Fact]
    public async Task SqlTaskCreationCompletionAndCancellationRules()
    {
        if (!Enabled) return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var group = Group(db);
            var a = Join(db, group, "Coordinator A");
            var b = Join(db, group, "Participant B");
            var c = Join(db, group, "Participant C");
            await db.SaveChangesAsync();
            var repository = Repo(db, clock);
            var id = (await repository.CreateAsync(a.AccountId, group.Id,
                Create(c.ProfileId, [b.ProfileId, c.ProfileId]), default))!.IncidentId;

            async Task<Guid> CreateTaskAsync(IncidentAdult actor, string description, IncidentAdult assignee)
            {
                var view = await repository.CreateTaskAsync(actor.AccountId, id, TaskFor(description, assignee.ProfileId), default);
                return view!.Tasks.Single(t => t.Description == description).TaskId;
            }

            var forB = await CreateTaskAsync(a, "Call the hotel", b);
            var forA = await CreateTaskAsync(a, "Check the station", a);
            var bSelf = await CreateTaskAsync(b, "Ask the neighbour", b);
            Assert.Null(await repository.CreateTaskAsync(b.AccountId, id, TaskFor("For C", c.ProfileId), default));
            Assert.Null(await repository.CreateTaskAsync(b.AccountId, id, TaskFor("For A", a.ProfileId), default));
            Assert.Null(await repository.CreateTaskAsync(a.AccountId, id, TaskFor("Outsider", Guid.NewGuid()), default));
            var task = await db.IncidentTasks.AsNoTracking().SingleAsync(t => t.Id == forB);
            Assert.Equal((b.AccountId, b.ProfileId, a.AccountId, IncidentTaskStatus.Open),
                (task.AssigneeAccountId, task.AssigneeProfileId, task.CreatedByAccountId, task.Status));
            Assert.Equal(await GrantIdAsync(db, id, b.ProfileId), task.AssigneeGrantId);

            // Only the assignee completes, with a categorized and sourced result, once.
            Assert.Null(await repository.CompleteTaskAsync(c.AccountId, id, forB, Complete(), default));
            Assert.Null(await repository.CompleteTaskAsync(a.AccountId, id, forB, Complete(), default));
            clock.Set(Start.AddMinutes(30));
            var completed = await repository.CompleteTaskAsync(b.AccountId, id, forB, Complete(), default);
            var done = completed!.Tasks.Single(t => t.TaskId == forB);
            Assert.Equal((IncidentTaskStatus.Done, (DateTimeOffset?)Start.AddMinutes(30)), (done.Status, done.CompletedAt));
            var result = completed.Entries.Single(e => e.EntryId == done.ResultEntryId);
            Assert.Equal((IncidentEntryKind.TaskResult, forB, IncidentInformationCategory.Reported, b.ProfileId, "Hotel front desk"),
                (result.Kind, result.RelatedTaskId!.Value, result.Category, result.ContributorProfileId, result.SourceDescription));
            Assert.Null(await repository.CompleteTaskAsync(b.AccountId, id, forB, Complete(), default));
            Assert.Null(await repository.CancelTaskAsync(a.AccountId, id, forB, Cancel(), default));

            // Cancellation: participants only their own self-created tasks; the coordinator any Open task.
            var forBByCoordinator = await CreateTaskAsync(a, "Check messages", b);
            var cSelf = await CreateTaskAsync(c, "Drive past", c);
            Assert.Null(await repository.CancelTaskAsync(c.AccountId, id, bSelf, Cancel(), default));
            Assert.Null(await repository.CancelTaskAsync(b.AccountId, id, forBByCoordinator, Cancel(), default));
            Assert.Null(await repository.CancelTaskAsync(b.AccountId, id, forA, Cancel(), default));
            var cancelled = await repository.CancelTaskAsync(b.AccountId, id, bSelf, Cancel(), default);
            var cancelledTask = cancelled!.Tasks.Single(t => t.TaskId == bSelf);
            Assert.Equal((IncidentTaskStatus.Cancelled, b.ProfileId, "No longer needed"),
                (cancelledTask.Status, cancelledTask.CancelledByProfileId!.Value, cancelledTask.CancelReason));
            Assert.Null(await repository.CompleteTaskAsync(b.AccountId, id, bSelf, Complete(), default));
            Assert.NotNull(await repository.CancelTaskAsync(a.AccountId, id, cSelf, Cancel(), default));

            // A re-added assignee holds a new grant and cannot complete the old task; the coordinator can cancel it.
            await repository.ReplaceParticipantsAsync(a.AccountId, id, new ReplaceIncidentParticipantsRequest([c.ProfileId]), default);
            await repository.ReplaceParticipantsAsync(a.AccountId, id,
                new ReplaceIncidentParticipantsRequest([b.ProfileId, c.ProfileId]), default);
            Assert.Null(await repository.CompleteTaskAsync(b.AccountId, id, forBByCoordinator, Complete(), default));
            Assert.NotNull(await repository.CancelTaskAsync(a.AccountId, id, forBByCoordinator, Cancel(), default));

            // After a handoff the former coordinator cannot cancel a task they created for someone else.
            var forCByA = await CreateTaskAsync(a, "Phone the school", c);
            await Transfer(repository, a, id, c.ProfileId);
            Assert.Null(await repository.CancelTaskAsync(a.AccountId, id, forCByA, Cancel(), default));

            // At resolution, Open tasks become read-only and are shown as not completed.
            var resolved = await repository.ResolveAsync(c.AccountId, id, Resolve(), default);
            Assert.True(resolved!.Tasks.Single(t => t.TaskId == forCByA).NotCompleted);
            Assert.True(resolved.Tasks.Single(t => t.TaskId == forA).NotCompleted);
            Assert.False(resolved.Tasks.Single(t => t.TaskId == forB).NotCompleted);
            Assert.Null(await repository.CompleteTaskAsync(c.AccountId, id, forCByA, Complete(), default));
            Assert.Equal(IncidentTaskStatus.Open,
                await db.IncidentTasks.Where(t => t.Id == forCByA).Select(t => t.Status).SingleAsync());

            Assert.Equal(6, await db.IncidentAuditEvents.CountAsync(x => x.Action == "TaskCreated" && x.TaskId != null));
            Assert.Equal(1, await db.IncidentAuditEvents.CountAsync(x => x.Action == "TaskCompleted" && x.TaskId == forB));
            Assert.Equal(3, await db.IncidentAuditEvents.CountAsync(x => x.Action == "TaskCancelled"));
        });
    }

    [Fact]
    public async Task SqlTimelineIsAReadTimeProjectionWithDeterministicOrder()
    {
        if (!Enabled) return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var group = Group(db);
            var a = Join(db, group, "Coordinator A");
            var b = Join(db, group, "Participant B");
            var c = Join(db, group, "Participant C");
            var d = Join(db, group, "Participant D");
            await db.SaveChangesAsync();
            var repository = Repo(db, clock);
            var id = (await repository.CreateAsync(a.AccountId, group.Id,
                Create(c.ProfileId, [b.ProfileId, c.ProfileId, d.ProfileId]), default))!.IncidentId;
            clock.Set(Start.AddMinutes(1));
            await repository.AddEntryAsync(b.AccountId, id, Observation("Seen at noon") with
            { EventAt = Start.AddHours(-2) }, default);
            await repository.AddEntryAsync(b.AccountId, id, Unknown("Current location unknown"), default);
            clock.Set(Start.AddMinutes(2));
            var withTask = await repository.CreateTaskAsync(a.AccountId, id, TaskFor("Call", b.ProfileId), default);
            var taskId = Assert.Single(withTask!.Tasks).TaskId;
            clock.Set(Start.AddMinutes(3));
            await repository.CompleteTaskAsync(b.AccountId, id, taskId, Complete(), default);
            var cancelId = (await repository.CreateTaskAsync(a.AccountId, id, TaskFor("Drive", a.ProfileId), default))!
                .Tasks.Single(t => t.Description == "Drive").TaskId;
            await repository.CancelTaskAsync(a.AccountId, id, cancelId, Cancel(), default);
            clock.Set(Start.AddMinutes(4));
            await repository.ReplaceParticipantsAsync(a.AccountId, id,
                new ReplaceIncidentParticipantsRequest([b.ProfileId, d.ProfileId]), default);
            d.Membership.End(Start.AddMinutes(5));
            await db.SaveChangesAsync();
            clock.Set(Start.AddMinutes(6));
            await Transfer(repository, a, id, b.ProfileId);
            clock.Set(Start.AddMinutes(7));
            var view = await repository.ResolveAsync(b.AccountId, id, Resolve(), default);

            var timeline = view!.Timeline;
            var types = timeline.Select(t => t.Type).ToHashSet();
            foreach (var expected in Enum.GetValues<IncidentTimelineItemType>().Where(t => t != IncidentTimelineItemType.Frozen))
                Assert.Contains(expected, types);
            Assert.Equal(Start.AddHours(-2), timeline[0].EventAt);
            Assert.Equal(IncidentEntryKind.Observation, timeline[0].EntryKind);
            Assert.Equal(IncidentInformationCategory.Reported, timeline[0].Category);
            Assert.Equal(IncidentTimelineItemType.Resolved, timeline[^1].Type);
            for (var i = 1; i < timeline.Count; i++)
            {
                var previous = timeline[i - 1];
                var current = timeline[i];
                var key = (previous.EventAt ?? previous.RecordedAt).CompareTo(current.EventAt ?? current.RecordedAt);
                if (key == 0) key = previous.RecordedAt.CompareTo(current.RecordedAt);
                if (key == 0) key = previous.Type.CompareTo(current.Type);
                if (key == 0) key = previous.ItemId.CompareTo(current.ItemId);
                Assert.True(key <= 0, $"Timeline item {i} is out of order.");
            }
            var ended = Assert.Single(timeline, t => t.Type == IncidentTimelineItemType.ParticipantAccessEnded);
            Assert.Equal((d.ProfileId, "System", (IncidentFreezeReason?)null), (ended.AffectedProfileId!.Value, ended.ActorName, ended.FreezeReason));
            var removed = Assert.Single(timeline, t => t.Type == IncidentTimelineItemType.ParticipantRemoved);
            Assert.Equal((c.ProfileId, a.ProfileId), (removed.AffectedProfileId!.Value, removed.ActorProfileId!.Value));
            var created = Assert.Single(timeline, t => t.Type == IncidentTimelineItemType.IncidentCreated);
            var summary = timeline.First(t => t.EntryKind == IncidentEntryKind.Summary);
            Assert.True(timeline.ToList().IndexOf(created) < timeline.ToList().IndexOf(summary));

            // The same order is produced on every read; no timeline table exists.
            var again = await repository.GetAsync(b.AccountId, id, default);
            Assert.Equal(timeline.Select(t => (t.Type, t.ItemId)), again!.Timeline.Select(t => (t.Type, t.ItemId)));
            var timelineTables = await db.Database.SqlQueryRaw<int>(
                "SELECT COUNT(*) AS [Value] FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE '%Timeline%'").SingleAsync();
            Assert.Equal(0, timelineTables);
        });
    }

    [Fact]
    public async Task SqlResolutionIsCoordinatorOnlyFinalAndTerminalStatesDenyEveryMutation()
    {
        if (!Enabled) return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var group = Group(db);
            var coordinator = Join(db, group, "Coordinator");
            var participant = Join(db, group, "Participant");
            var other = Join(db, group, "Other");
            await db.SaveChangesAsync();
            var repository = Repo(db, clock);
            var id = (await repository.CreateAsync(coordinator.AccountId, group.Id,
                Create(participant.ProfileId, [participant.ProfileId]), default))!.IncidentId;
            var taskId = Assert.Single((await repository.CreateTaskAsync(coordinator.AccountId, id,
                TaskFor("Call", participant.ProfileId), default))!.Tasks).TaskId;

            Assert.Null(await repository.ResolveAsync(participant.AccountId, id, Resolve(), default));
            clock.Set(Start.AddDays(2));
            var resolved = await repository.ResolveAsync(coordinator.AccountId, id, Resolve() with
            {
                ContactRestoredAt = null, ContactRestoredTimeUnknown = true, AssistanceProvided = "Neighbour drove over",
                UnresolvedFollowUp = "Check the phone charger", Lessons = "Share the hotel name earlier",
            }, default);
            Assert.Equal(IncidentState.Resolved, resolved!.State);
            var resolution = resolved.Resolution!;
            Assert.Equal(("Coordination ended", "Contact restored by phone", IncidentInformationCategory.Reported, "Phone call"),
                (resolution.Summary, resolution.OutcomeText, resolution.OutcomeCategory, resolution.OutcomeSourceDescription));
            Assert.Equal((true, (DateTimeOffset?)null, coordinator.ProfileId, Start.AddDays(2)),
                (resolution.ContactRestoredTimeUnknown, resolution.ContactRestoredAt, resolution.ResolvedByProfileId, resolution.ResolvedAt));
            Assert.Equal(("Neighbour drove over", "Check the phone charger", "Share the hotel name earlier"),
                (resolution.AssistanceProvided, resolution.UnresolvedFollowUp, resolution.Lessons));
            Assert.Equal(Start.AddDays(367), resolved.ExpiresAt);
            Assert.Null(resolved.InactivityFreezeAt);

            Assert.Null(await repository.AddEntryAsync(coordinator.AccountId, id, Observation("Late"), default));
            Assert.Null(await repository.AddEntryAsync(coordinator.AccountId, id, SummaryEntry("Late summary"), default));
            Assert.Null(await repository.CreateTaskAsync(coordinator.AccountId, id, TaskFor("Late", coordinator.ProfileId), default));
            Assert.Null(await repository.CompleteTaskAsync(participant.AccountId, id, taskId, Complete(), default));
            Assert.Null(await repository.CancelTaskAsync(coordinator.AccountId, id, taskId, Cancel(), default));
            Assert.Null(await repository.ReplaceParticipantsAsync(coordinator.AccountId, id,
                new ReplaceIncidentParticipantsRequest([other.ProfileId]), default));
            Assert.Null(await Transfer(repository, coordinator, id, participant.ProfileId));
            Assert.Null(await repository.ResolveAsync(coordinator.AccountId, id, Resolve(), default));

            // A Resolved Incident never becomes Frozen and remains readable until expiry.
            clock.Set(Start.AddDays(200));
            await repository.ReconcileAsync(default);
            var later = await repository.GetAsync(participant.AccountId, id, default);
            Assert.Equal((IncidentState.Resolved, (DateTimeOffset?)null), (later!.State, later.FrozenAt));
            Assert.False(await db.IncidentAuditEvents.AnyAsync(a => a.Action == "Frozen"));

            // Resolve after the inactivity deadline is denied: the effective Frozen state wins.
            var second = (await repository.CreateAsync(coordinator.AccountId, group.Id,
                Create(participant.ProfileId, []), default))!.IncidentId;
            clock.Set(Start.AddDays(230));
            Assert.Null(await repository.ResolveAsync(coordinator.AccountId, second, Resolve() with
            { ContactRestoredAt = Start.AddDays(229), ContactRestoredTimeUnknown = false }, default));
            var frozen = await repository.GetAsync(coordinator.AccountId, second, default);
            Assert.Equal((IncidentState.Frozen, Start.AddDays(230)), (frozen!.State, frozen.FrozenAt!.Value));
            Assert.Null(frozen.Resolution);
        });
    }

    [Fact]
    public async Task SqlInactivityFreezesAt30DaysAndOnlyAuthorizedMutationsResetTheClock()
    {
        if (!Enabled) return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var group = Group(db);
            var a = Join(db, group, "Coordinator");
            var b = Join(db, group, "Participant");
            var outsider = Join(db, group, "Outsider");
            await db.SaveChangesAsync();
            var repository = Repo(db, clock);
            var id = (await repository.CreateAsync(a.AccountId, group.Id,
                Create(b.ProfileId, [b.ProfileId]), default))!.IncidentId;

            // Reads, list, denied attempts, and maintenance do not extend the deadline.
            clock.Set(Start.AddDays(29));
            await repository.GetAsync(b.AccountId, id, default);
            await repository.GetVisibleAsync(b.AccountId, default);
            await repository.GetAsync(outsider.AccountId, id, default);
            await repository.AddEntryAsync(outsider.AccountId, id, Observation("Denied"), default);
            await repository.ResolveAsync(b.AccountId, id, Resolve(), default);
            await repository.ReconcileAsync(default);
            Assert.Equal(Start.AddDays(30), (await repository.GetAsync(b.AccountId, id, default))!.InactivityFreezeAt);

            // Every listed mutation type resets the 30-day clock.
            var day = 29;
            async Task<IncidentView> AfterAsync(Func<Task<IncidentView?>> mutation)
            {
                clock.Set(Start.AddDays(++day));
                var view = await mutation();
                Assert.NotNull(view);
                Assert.Equal(Start.AddDays(day), view.LastAuthorizedMutationAt);
                Assert.Equal(Start.AddDays(day + 30), view.InactivityFreezeAt);
                return view;
            }
            day -= 1;
            await AfterAsync(() => repository.AddEntryAsync(b.AccountId, id, Observation("Entry"), default));
            var withTask = await AfterAsync(() => repository.CreateTaskAsync(a.AccountId, id, TaskFor("Task", b.ProfileId), default));
            var taskId = Assert.Single(withTask.Tasks).TaskId;
            await AfterAsync(() => repository.CompleteTaskAsync(b.AccountId, id, taskId, Complete(), default));
            var other = (await AfterAsync(() => repository.CreateTaskAsync(a.AccountId, id, TaskFor("Other", a.ProfileId), default)))
                .Tasks.Single(t => t.Description == "Other").TaskId;
            await AfterAsync(() => repository.CancelTaskAsync(a.AccountId, id, other, Cancel(), default));
            await AfterAsync(() => repository.ReplaceParticipantsAsync(a.AccountId, id,
                new ReplaceIncidentParticipantsRequest([b.ProfileId]), default));
            await AfterAsync(() => Transfer(repository, a, id, b.ProfileId));
            var last = Start.AddDays(day);

            clock.Set(last.AddDays(30).AddTicks(-1));
            Assert.Equal(IncidentState.Open, (await repository.GetAsync(b.AccountId, id, default))!.State);
            clock.Set(last.AddDays(30).AddHours(5));
            var frozen = await repository.GetAsync(b.AccountId, id, default);
            Assert.Equal((IncidentState.Frozen, last.AddDays(30), IncidentFreezeReason.Inactivity),
                (frozen!.State, frozen.FrozenAt!.Value, frozen.FreezeReason!.Value));
            Assert.Single(frozen.Timeline, t => t.Type == IncidentTimelineItemType.Frozen && t.ActorName == "System");
            await repository.GetAsync(a.AccountId, id, default);
            await repository.ReconcileAsync(default);
            Assert.Equal(1, await db.IncidentAuditEvents.CountAsync(x => x.Action == "Frozen" && x.IncidentId == id));
            Assert.Equal(0, await db.Incidents.CountAsync(i => i.Id != id));
        });
    }

    private static Task<IncidentView?> Transfer(EfIncidentRepository repository, IncidentAdult actor, Guid incidentId,
        Guid targetProfileId) =>
        repository.TransferCoordinatorAsync(actor.AccountId, incidentId,
            new TransferIncidentCoordinatorRequest(targetProfileId), default);

    private static Task<Guid> GrantIdAsync(FamilySafetyDbContext db, Guid incidentId, Guid profileId) =>
        db.IncidentParticipantGrants.AsNoTracking()
            .Where(g => g.IncidentId == incidentId && g.ProfileId == profileId && g.RemovedAt == null)
            .Select(g => g.Id).SingleAsync();
}

internal sealed record IncidentAdult(Guid AccountId, Guid ProfileId, FamilyGroupMembership Membership);

/// <summary>Shared fixture for the SQL-gated Incident test classes.</summary>
internal static class IncidentSqlFixture
{
    public const string Reason = "Contact has not been confirmed since this morning";
    public static readonly DateTimeOffset Start = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

    public static bool Enabled => Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") == "1";

    public static FamilyGroup Group(FamilySafetyDbContext db, string name = "Incident family")
    {
        var group = new FamilyGroup(Guid.NewGuid(), name, 100);
        db.Add(group);
        return group;
    }

    public static IncidentAdult Join(FamilySafetyDbContext db, FamilyGroup group, string name, bool active = true)
    {
        var (accountId, profileId) = AddPerson(db, name);
        var membership = new FamilyGroupMembership(Guid.NewGuid(), accountId, profileId, group.Id, active);
        db.Add(membership);
        return new IncidentAdult(accountId, profileId, membership);
    }

    public static IncidentAdult Join(FamilySafetyDbContext db, FamilyGroup group, IncidentAdult existing)
    {
        var membership = new FamilyGroupMembership(Guid.NewGuid(), existing.AccountId, existing.ProfileId, group.Id, true);
        db.Add(membership);
        return existing with { Membership = membership };
    }

    public static (Guid AccountId, Guid ProfileId) AddPerson(FamilySafetyDbContext db, string name)
    {
        var accountId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        db.AddRange(new Account(accountId, $"{accountId:N}@example.invalid"), new AdultProfile(profileId, accountId, name));
        return (accountId, profileId);
    }

    public static EfIncidentRepository Repo(FamilySafetyDbContext db, TimeProvider clock) =>
        new(db, clock, new EfConcernAccessQuery(db, clock));

    public static DbContextOptions<FamilySafetyDbContext> Options(FamilySafetyDbContext db) =>
        new DbContextOptionsBuilder<FamilySafetyDbContext>()
            .UseSqlServer(db.Database.GetDbConnection().ConnectionString).Options;

    public static CreateIncidentRequest Create(Guid subjectProfileId, IReadOnlyList<Guid> participants,
        Guid? originatingConcernId = null) =>
        new(subjectProfileId, Reason, IncidentInformationCategory.Reported, "Phone call with a neighbour", false,
            participants, originatingConcernId);

    public static AddIncidentEntryRequest Observation(string text) =>
        new(IncidentEntryKind.Observation, IncidentInformationCategory.Reported, text, "Direct conversation", false);

    public static AddIncidentEntryRequest Unknown(string text) =>
        new(IncidentEntryKind.Unknown, IncidentInformationCategory.Unknown, text, null, true);

    public static AddIncidentEntryRequest SummaryEntry(string text) =>
        new(IncidentEntryKind.Summary, IncidentInformationCategory.Assumption, text, "Coordinator review", false);

    public static AddIncidentEntryRequest Correction(Guid target) =>
        new(IncidentEntryKind.Correction, IncidentInformationCategory.Reported, "It was 5 PM", "Direct call", false,
            RelatedEntryId: target, Explanation: "Correcting my earlier entry");

    public static AddIncidentEntryRequest Challenge(Guid target) =>
        new(IncidentEntryKind.Challenge, IncidentInformationCategory.Assumption, "I heard a different time",
            "Second-hand report", false, RelatedEntryId: target, Explanation: "Contrary report");

    public static CreateIncidentTaskRequest TaskFor(string description, Guid assigneeProfileId) =>
        new(description, assigneeProfileId);

    public static CompleteIncidentTaskRequest Complete() =>
        new("Called the hotel; they confirmed check-in", IncidentInformationCategory.Reported, "Hotel front desk", false);

    public static CancelIncidentTaskRequest Cancel() => new("No longer needed");

    public static ResolveIncidentRequest Resolve() =>
        new("Coordination ended", "Contact restored by phone", IncidentInformationCategory.Reported, "Phone call", false,
            Start.AddHours(5), false);

    public static async Task WithDatabaseAsync(Func<FamilySafetyDbContext, ManualClock, Task> test)
    {
        var connection = $"Server=localhost;Database=FamilySafety_WI008003_IncidentTests_{Guid.NewGuid():N};Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
        var options = new DbContextOptionsBuilder<FamilySafetyDbContext>().UseSqlServer(connection).Options;
        var clock = new ManualClock(Start);
        await using var db = new FamilySafetyDbContext(options);
        var cleanupRequired = false;
        try
        {
            await db.Database.EnsureDeletedAsync();
            cleanupRequired = true;
            await db.Database.MigrateAsync();
            await test(db, clock);
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
}

/// <summary>Records the text of every SQL command a DbContext executes.</summary>
internal sealed class SqlCommandCapture : DbCommandInterceptor
{
    private readonly ConcurrentQueue<string> commands = new();
    public IReadOnlyCollection<string> Commands => commands;

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
        CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        commands.Enqueue(command.CommandText);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command,
        CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        commands.Enqueue(command.CommandText);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command,
        CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        commands.Enqueue(command.CommandText);
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }
}

internal sealed class ManualClock(DateTimeOffset now) : TimeProvider
{
    private DateTimeOffset current = now;
    public override DateTimeOffset GetUtcNow() => current;
    public void Set(DateTimeOffset value) => current = value;
}
