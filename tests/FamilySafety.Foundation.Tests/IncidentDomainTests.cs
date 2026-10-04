using System.Reflection;
using FamilySafety.Application.Incidents;
using FamilySafety.Domain.Incidents;
using FamilySafety.Infrastructure.Persistence.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace FamilySafety.Foundation.Tests;

public sealed class IncidentDomainTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void InactivityFreezeAndRetentionUse30And365DayClocks()
    {
        var incident = NewIncident();
        Assert.Equal(Start.AddDays(30), incident.InactivityFreezeAt);
        Assert.False(incident.IsEffectivelyFrozen(Start.AddDays(30).AddTicks(-1), null));
        Assert.True(incident.IsEffectivelyFrozen(Start.AddDays(30), null));
        Assert.Null(incident.ExpiresAt);

        Assert.True(incident.Freeze(Start.AddDays(30), IncidentFreezeReason.Inactivity));
        Assert.False(incident.Freeze(Start.AddDays(31), IncidentFreezeReason.Inactivity));
        Assert.Equal(Start.AddDays(30), incident.FrozenAt);
        Assert.Equal(Start.AddDays(395), incident.ExpiresAt);
        Assert.False(incident.IsExpired(Start.AddDays(395).AddTicks(-1)));
        Assert.True(incident.IsExpired(Start.AddDays(395)));
    }

    [Fact]
    public void EarlierFreezeCauseControlsTimeAndReason()
    {
        var incident = NewIncident();
        var coordinatorLoss = Start.AddDays(5);
        Assert.True(incident.IsEffectivelyFrozen(Start.AddDays(6), coordinatorLoss));
        Assert.Equal(coordinatorLoss, incident.EffectiveFreezeAt(coordinatorLoss));
        Assert.Equal(IncidentFreezeReason.CoordinatorEligibilityLost, incident.EffectiveFreezeReason(coordinatorLoss));

        var laterLoss = Start.AddDays(31);
        Assert.Equal(Start.AddDays(30), incident.EffectiveFreezeAt(laterLoss));
        Assert.Equal(IncidentFreezeReason.Inactivity, incident.EffectiveFreezeReason(laterLoss));
    }

    [Fact]
    public void AuthorizedMutationResetsInactivityOnlyWhileOpen()
    {
        var incident = NewIncident();
        incident.RecordAuthorizedMutation(Start.AddDays(10));
        Assert.Equal(Start.AddDays(40), incident.InactivityFreezeAt);
        incident.Freeze(Start.AddDays(40), IncidentFreezeReason.Inactivity);
        Assert.Throws<InvalidOperationException>(() => incident.RecordAuthorizedMutation(Start.AddDays(41)));
        Assert.Throws<InvalidOperationException>(() => incident.TransferCoordinator(Guid.NewGuid(), Start.AddDays(41)));
    }

    [Fact]
    public void ResolutionIsFinalNeverFreezesAndHasNoVerdictField()
    {
        var incident = NewIncident();
        incident.Resolve(Guid.NewGuid(), Guid.NewGuid(), Resolution(), Start.AddDays(2));
        Assert.Equal(IncidentState.Resolved, incident.State);
        Assert.Equal(Start.AddDays(367), incident.ExpiresAt);
        Assert.False(incident.Freeze(Start.AddDays(100), IncidentFreezeReason.Inactivity));
        Assert.False(incident.IsEffectivelyFrozen(Start.AddDays(100), Start.AddDays(50)));
        Assert.Null(incident.EffectiveFreezeAt(Start.AddDays(50)));
        Assert.Throws<InvalidOperationException>(() =>
            incident.Resolve(Guid.NewGuid(), Guid.NewGuid(), Resolution(), Start.AddDays(3)));

        var forbidden = new[] { "Verdict", "Safe", "Danger", "Emergency", "Severity", "Risk", "Score" };
        Assert.DoesNotContain(typeof(SafetyIncident).GetProperties().Select(p => p.Name),
            name => forbidden.Any(word => name.Contains(word, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void ResolutionRequiresCategorizedOutcomeSourceAndExactlyOneContactTime()
    {
        Assert.Throws<ArgumentException>(() => Resolution(restoredAt: null, restoredUnknown: false));
        Assert.Throws<ArgumentException>(() => Resolution(restoredAt: Start, restoredUnknown: true));
        Assert.Throws<ArgumentOutOfRangeException>(() => Resolution(category: IncidentInformationCategory.Outdated));
        Assert.Throws<ArgumentException>(() => Resolution(category: IncidentInformationCategory.Confirmed,
            source: null, sourceUnknown: true));
        Assert.Throws<ArgumentException>(() => new IncidentResolution(" ", "Outcome",
            IncidentInformationCategory.Reported, "Call", false, Start, false, null, null, null));
        var unknownTime = Resolution(restoredAt: null, restoredUnknown: true, category: IncidentInformationCategory.Unknown);
        Assert.True(unknownTime.ContactRestoredTimeUnknown);
        Assert.Null(unknownTime.ContactRestoredAt);
    }

    [Fact]
    public void EntryCategoryAndProvenanceRulesMirrorConcern()
    {
        Assert.Throws<ArgumentException>(() => Entry(IncidentEntryKind.Unknown, IncidentInformationCategory.Reported));
        Assert.Throws<ArgumentException>(() => Entry(IncidentEntryKind.Observation, IncidentInformationCategory.Unknown));
        Assert.Throws<ArgumentException>(() => Entry(IncidentEntryKind.Observation, IncidentInformationCategory.Outdated));
        Assert.Throws<ArgumentException>(() => Entry(IncidentEntryKind.Observation, IncidentInformationCategory.Confirmed,
            source: null, sourceUnknown: true));
        Assert.Throws<ArgumentException>(() => Entry(IncidentEntryKind.Observation, IncidentInformationCategory.Reported,
            source: "Both", sourceUnknown: true));
        Assert.Throws<ArgumentException>(() => Entry(IncidentEntryKind.Unknown, IncidentInformationCategory.Unknown,
            isOutdated: true, source: null, sourceUnknown: true));

        var outdated = Entry(IncidentEntryKind.Observation, IncidentInformationCategory.Reported, isOutdated: true);
        Assert.Equal(IncidentInformationCategory.Reported, outdated.Category);
        Assert.True(outdated.IsOutdated);
        var unknown = Entry(IncidentEntryKind.Unknown, IncidentInformationCategory.Unknown, source: null, sourceUnknown: true);
        Assert.True(unknown.SourceUnknown);
        Assert.Null(unknown.SourceDescription);
    }

    [Fact]
    public void EntryLinksAreRestrictedByKind()
    {
        var target = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => Entry(IncidentEntryKind.Correction, IncidentInformationCategory.Reported));
        Assert.ThrowsAny<ArgumentException>(() => Entry(IncidentEntryKind.Correction, IncidentInformationCategory.Reported,
            relatedEntryId: target));
        Assert.Throws<ArgumentException>(() => Entry(IncidentEntryKind.Observation, IncidentInformationCategory.Reported,
            relatedEntryId: target, explanation: "Why"));
        Assert.Throws<ArgumentException>(() => Entry(IncidentEntryKind.TaskResult, IncidentInformationCategory.Reported));
        Assert.Throws<ArgumentException>(() => Entry(IncidentEntryKind.Observation, IncidentInformationCategory.Reported,
            relatedTaskId: Guid.NewGuid()));
        var challenge = Entry(IncidentEntryKind.Challenge, IncidentInformationCategory.Assumption,
            relatedEntryId: target, explanation: "A different report");
        Assert.Equal(target, challenge.RelatedEntryId);
        var result = Entry(IncidentEntryKind.TaskResult, IncidentInformationCategory.Reported, relatedTaskId: Guid.NewGuid());
        Assert.NotNull(result.RelatedTaskId);
        Assert.Throws<ArgumentException>(() => Entry(IncidentEntryKind.TaskResult, IncidentInformationCategory.Reported,
            relatedTaskId: Guid.NewGuid(), isOutdated: true));
    }

    [Fact]
    public void ContactAttemptRequiresFixedSetsAndExactlyOneAttemptTime()
    {
        var details = new IncidentContactAttemptDetails(null, "Sam's mobile", IncidentContactMethod.Call,
            IncidentContactOutcome.NoAnswer);
        Assert.Throws<ArgumentException>(() => Entry(IncidentEntryKind.ContactAttempt,
            IncidentInformationCategory.Confirmed, contactAttempt: details));
        Assert.Throws<ArgumentException>(() => Entry(IncidentEntryKind.ContactAttempt,
            IncidentInformationCategory.Confirmed, contactAttempt: details, eventAt: Start, eventTimeUnknown: true));
        Assert.Throws<ArgumentException>(() => Entry(IncidentEntryKind.Observation,
            IncidentInformationCategory.Confirmed, contactAttempt: details));
        Assert.Throws<ArgumentOutOfRangeException>(() => Entry(IncidentEntryKind.ContactAttempt,
            IncidentInformationCategory.Confirmed, eventAt: Start,
            contactAttempt: details with { Method = (IncidentContactMethod)99 }));
        Assert.Throws<ArgumentException>(() => Entry(IncidentEntryKind.ContactAttempt,
            IncidentInformationCategory.Confirmed, eventAt: Start, contactAttempt: details with { TargetLabel = " " }));

        var attempt = Entry(IncidentEntryKind.ContactAttempt, IncidentInformationCategory.Reported, text: null,
            eventTimeUnknown: true, contactAttempt: details with { AttemptedByDescription = "Neighbour Jo" });
        Assert.Null(attempt.Text);
        Assert.True(attempt.EventTimeUnknown);
        Assert.Equal("Neighbour Jo", attempt.AttemptedByDescription);
        Assert.Equal(IncidentContactOutcome.NoAnswer, attempt.ContactOutcome);
        Assert.Equal(["Call", "TextMessage", "Email", "InAppMessage", "InPerson", "Other"],
            Enum.GetNames<IncidentContactMethod>());
        Assert.Equal(["Reached", "NoAnswer", "LeftMessage", "SentNoReplyYet", "Other", "Unknown"],
            Enum.GetNames<IncidentContactOutcome>());
    }

    [Fact]
    public void TaskStatusOnlyMovesForwardAndOnlyTheAssigneeGrantCompletes()
    {
        var assigneeGrant = Guid.NewGuid();
        var task = NewTask(assigneeGrant);
        Assert.Throws<InvalidOperationException>(() => task.Complete(Guid.NewGuid(), Start));
        task.Complete(assigneeGrant, Start.AddHours(1));
        Assert.Equal(IncidentTaskStatus.Done, task.Status);
        Assert.Equal(Start.AddHours(1), task.CompletedAt);
        Assert.Throws<InvalidOperationException>(() => task.Complete(assigneeGrant, Start.AddHours(2)));
        Assert.Throws<InvalidOperationException>(() => task.Cancel(Guid.NewGuid(), Guid.NewGuid(), "Late", Start));

        var cancelled = NewTask(assigneeGrant);
        cancelled.Cancel(Guid.NewGuid(), Guid.NewGuid(), "No longer needed", Start.AddHours(1));
        Assert.Equal(IncidentTaskStatus.Cancelled, cancelled.Status);
        Assert.Throws<InvalidOperationException>(() => cancelled.Complete(assigneeGrant, Start.AddHours(2)));
        Assert.Equal(["Open", "Done", "Cancelled"], Enum.GetNames<IncidentTaskStatus>());
    }

    [Fact]
    public void GrantRevocationIsRecordedOnceAndNeverRefreshed()
    {
        var grant = new IncidentParticipantGrant(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3,
            Start, Guid.NewGuid());
        Assert.True(grant.MatchesMembership(grant.MembershipId, grant.AccountId, true, 3));
        Assert.False(grant.MatchesMembership(grant.MembershipId, grant.AccountId, true, 4));
        Assert.False(grant.MatchesMembership(grant.MembershipId, grant.AccountId, false, 3));
        Assert.True(grant.RevokeForEligibilityLoss(Start.AddDays(1)));
        Assert.False(grant.RevokeForEligibilityLoss(Start.AddDays(2)));
        Assert.Equal(Start.AddDays(1), grant.RemovedAt);
        Assert.False(grant.MatchesMembership(grant.MembershipId, grant.AccountId, true, 3));
    }

    [Fact]
    public void DeniedAuditSummaryHasNoIncidentIdAndExpiresAfter365Days()
    {
        var summary = IncidentAuditEvent.DeniedSummary(Guid.NewGuid(), IncidentAuditEvent.ReadDenied,
            Start, Start.AddMinutes(3), Start.AddMinutes(3));
        summary.RecordDeniedAttempt(Start.AddMinutes(42));
        Assert.Null(summary.IncidentId);
        Assert.False(summary.Authorized);
        Assert.Equal(Start, summary.UtcHourBucket);
        Assert.Equal(Start.AddMinutes(3), summary.FirstAttemptAt);
        Assert.Equal(Start.AddMinutes(42), summary.LastAttemptAt);
        Assert.Equal(2, summary.DeniedAttemptCount);
        Assert.Equal(Start.AddMinutes(3).AddDays(365), summary.ExpiresAt);
        Assert.Equal(Start, IncidentAuditEvent.HourBucket(Start.AddMinutes(59).AddSeconds(59)));
        Assert.Throws<ArgumentOutOfRangeException>(() => IncidentAuditEvent.DeniedSummary(Guid.NewGuid(),
            "ConcernReadDenied", Start, Start, Start));
        Assert.DoesNotContain(typeof(IncidentAuditEvent).GetProperties().Select(p => p.Name),
            name => name is "Text" or "Description" or "TargetLabel" or "Summary" or "Notes" or "SourceDescription");
    }

    [Fact]
    public void TimelineOrdersByEventTimeThenRecordedTimeThenTypeThenId()
    {
        var at = Start;
        var low = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var high = Guid.Parse("00000000-0000-0000-0000-000000000002");
        IncidentTimelineItemView Item(Guid id, IncidentTimelineItemType type, DateTimeOffset recorded,
            DateTimeOffset? eventAt = null) =>
            new(id, type, eventAt, recorded, null, "System", null, null, false, null, null, null, null, null, null);

        var ordered = IncidentTimeline.Order(
        [
            Item(high, IncidentTimelineItemType.Entry, at),
            Item(low, IncidentTimelineItemType.Entry, at),
            Item(low, IncidentTimelineItemType.Resolved, at),
            Item(high, IncidentTimelineItemType.IncidentCreated, at),
            Item(low, IncidentTimelineItemType.Entry, at.AddHours(2), eventAt: at.AddHours(-1)),
            Item(high, IncidentTimelineItemType.Entry, at.AddHours(1), eventAt: at),
        ]);

        Assert.Equal(at.AddHours(-1), ordered[0].EventAt);
        Assert.Equal(IncidentTimelineItemType.IncidentCreated, ordered[1].Type);
        Assert.Equal((IncidentTimelineItemType.Entry, low), (ordered[2].Type, ordered[2].ItemId));
        Assert.Equal((IncidentTimelineItemType.Entry, high), (ordered[3].Type, ordered[3].ItemId));
        Assert.Equal(IncidentTimelineItemType.Resolved, ordered[4].Type);
        // Same effective time, later recorded time sorts after.
        Assert.Equal(at.AddHours(1), ordered[5].RecordedAt);
    }

    [Fact]
    public void TimelineProjectsRecordsAndAccessEndCarriesNoReason()
    {
        var incident = NewIncident();
        var coordinatorGrant = new IncidentParticipantGrant(incident.Id, Guid.NewGuid(), incident.CreatedByProfileId,
            Guid.NewGuid(), 0, Start, null);
        var participant = new IncidentParticipantGrant(incident.Id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0,
            Start.AddMinutes(1), incident.CreatedByProfileId);
        participant.RevokeForEligibilityLoss(Start.AddMinutes(5));
        var removed = new IncidentParticipantGrant(incident.Id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0,
            Start.AddMinutes(1), incident.CreatedByProfileId);
        removed.Remove(incident.CreatedByProfileId, Start.AddMinutes(6));
        var entry = Entry(IncidentEntryKind.Observation, IncidentInformationCategory.Reported, incidentId: incident.Id,
            eventAt: Start.AddMinutes(-30));
        incident.Freeze(Start.AddDays(30), IncidentFreezeReason.Inactivity);

        var timeline = IncidentTimeline.Build(incident, [coordinatorGrant, participant, removed], [], [entry], [],
            _ => "Name");

        Assert.Equal(IncidentTimelineItemType.Entry, timeline[0].Type);
        Assert.Equal(IncidentInformationCategory.Reported, timeline[0].Category);
        Assert.Equal(IncidentTimelineItemType.IncidentCreated, timeline[1].Type);
        Assert.Equal(2, timeline.Count(i => i.Type == IncidentTimelineItemType.ParticipantAdded));
        var ended = Assert.Single(timeline, i => i.Type == IncidentTimelineItemType.ParticipantAccessEnded);
        Assert.Equal(IncidentTimeline.SystemActorName, ended.ActorName);
        Assert.Null(ended.ActorProfileId);
        Assert.Null(ended.FreezeReason);
        Assert.Single(timeline, i => i.Type == IncidentTimelineItemType.ParticipantRemoved);
        var frozen = Assert.Single(timeline, i => i.Type == IncidentTimelineItemType.Frozen);
        Assert.Equal(IncidentFreezeReason.Inactivity, frozen.FreezeReason);
        Assert.Equal(IncidentTimelineItemType.Frozen, timeline[^1].Type);
    }

    [Fact]
    public void ResponseContractsExposeNoProtectedDomainFields()
    {
        var forbiddenWords = new HashSet<string>(StringComparer.Ordinal)
        {
            "Phone", "Email", "Address", "Birthday", "Location", "Latitude", "Longitude", "Trip", "Event",
            "Plan", "Sharing", "Relationship", "Score", "Verdict", "Map",
        };
        var forbiddenPhrases = new[] { "CheckIn", "SafetyCircle", "TrustedContact" };
        var viewTypes = typeof(IncidentView).Assembly.GetTypes()
            .Where(t => t.Namespace == typeof(IncidentView).Namespace && t.Name.StartsWith("Incident", StringComparison.Ordinal)
                && t.Name.EndsWith("View", StringComparison.Ordinal));
        var propertyNames = viewTypes.SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .Select(p => p.Name).Distinct().ToList();
        Assert.NotEmpty(propertyNames);
        // EventAt / EventTimeUnknown are the entry's own event time, not the Events domain.
        var checkedNames = propertyNames.Where(name => name is not ("EventAt" or "EventTimeUnknown")).ToList();
        Assert.DoesNotContain(checkedNames, name =>
            System.Text.RegularExpressions.Regex.Split(name, "(?<!^)(?=[A-Z])").Any(forbiddenWords.Contains)
            || forbiddenPhrases.Any(phrase => name.Contains(phrase, StringComparison.Ordinal)));
    }

    [Fact]
    public void MigrationOnlyCreatesIncidentTablesAndIndexes()
    {
        var migration = new SafetyIncidentDomainAndAccessFoundation();
        var up = migration.UpOperations;
        Assert.NotEmpty(up);
        Assert.All(up, operation => Assert.True(operation is CreateTableOperation or CreateIndexOperation,
            $"Unexpected migration operation {operation.GetType().Name}."));
        var tables = up.OfType<CreateTableOperation>().Select(t => t.Name).Order(StringComparer.Ordinal).ToList();
        Assert.Equal(["IncidentAuditEvents", "IncidentCoordinatorChanges", "IncidentEntries",
            "IncidentParticipantGrants", "IncidentTasks", "Incidents"], tables);
        Assert.All(up.OfType<CreateIndexOperation>(), index => Assert.StartsWith("Incident", index.Table, StringComparison.Ordinal));
        var foreignKeys = up.OfType<CreateTableOperation>().SelectMany(t => t.ForeignKeys).ToList();
        Assert.DoesNotContain(foreignKeys, fk => fk.PrincipalTable == "Concerns");
        Assert.DoesNotContain(foreignKeys, fk => fk.Columns.Contains("CoordinatorGrantId")
            || fk.Columns.Contains("OriginatingConcernId"));
        Assert.DoesNotContain(foreignKeys, fk => fk.Table == "IncidentAuditEvents");
        Assert.All(foreignKeys, fk => Assert.Equal(ReferentialAction.Restrict, fk.OnDelete));
        Assert.All(migration.DownOperations, operation => Assert.IsType<DropTableOperation>(operation));
    }

    private static SafetyIncident NewIncident() => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, Start);

    private static IncidentTask NewTask(Guid assigneeGrant) => new(Guid.NewGuid(), Guid.NewGuid(), "Call the hotel",
        Guid.NewGuid(), Guid.NewGuid(), assigneeGrant, Guid.NewGuid(), Guid.NewGuid(), Start);

    private static IncidentResolution Resolution(DateTimeOffset? restoredAt = null, bool restoredUnknown = true,
        IncidentInformationCategory category = IncidentInformationCategory.Reported, string? source = "Phone call",
        bool sourceUnknown = false) =>
        new("Coordination ended", "Contact was restored by phone", category, source, sourceUnknown, restoredAt,
            restoredUnknown, null, null, null);

    private static IncidentEntry Entry(IncidentEntryKind kind, IncidentInformationCategory category,
        string? text = "Arrival was reported", string? source = "Direct conversation", bool sourceUnknown = false,
        bool isOutdated = false, DateTimeOffset? eventAt = null, bool eventTimeUnknown = false,
        Guid? relatedEntryId = null, Guid? relatedTaskId = null, string? explanation = null,
        IncidentContactAttemptDetails? contactAttempt = null, Guid? incidentId = null) =>
        new(Guid.NewGuid(), incidentId ?? Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), kind, category, isOutdated,
            text, source, sourceUnknown, Start, eventAt, eventTimeUnknown, relatedEntryId, relatedTaskId, explanation,
            contactAttempt);
}
