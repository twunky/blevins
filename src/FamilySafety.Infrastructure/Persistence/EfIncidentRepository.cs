using System.Data;
using System.Linq.Expressions;
using FamilySafety.Application.Concerns;
using FamilySafety.Application.Incidents;
using FamilySafety.Domain.Incidents;
using FamilySafety.Domain.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FamilySafety.Infrastructure.Persistence;

/// <summary>
/// Incident persistence and commit-time authorization (REQ-SAFE-016, REQ-PRIV-026).
/// Every read and command runs in a Serializable transaction that first locks the Family Group row,
/// then loads the Incident, applies its effective lifecycle (inactivity or coordinator-loss freeze,
/// grant revocation, expiry), and only then evaluates the caller's grant. A null result is the
/// uniform, nondisclosing unavailable result.
/// </summary>
public sealed class EfIncidentRepository(FamilySafetyDbContext db, TimeProvider clock,
    IConcernAccessQuery concernAccess) : IIncidentRepository
{
    private const string MemberFallbackName = "Member";
    private const int ReconciliationBatchSize = 100;
    private const int MaxReconciliationBatches = 20;

    public async Task<IReadOnlyList<IncidentSummaryView>> GetVisibleAsync(Guid accountId,
        CancellationToken cancellationToken)
    {
        // Candidate pre-filter only; each Incident is then authorized under the full request-time rules.
        var candidates = await db.Incidents.AsNoTracking()
            .Where(i => db.IncidentParticipantGrants.Any(g => g.IncidentId == i.Id && g.AccountId == accountId
                && g.RemovedAt == null))
            .OrderByDescending(i => i.CreatedAt).ThenBy(i => i.Id)
            .Select(i => new { i.Id, i.FamilyGroupId }).ToListAsync(cancellationToken);
        var views = new List<IncidentSummaryView>(candidates.Count);
        foreach (var candidate in candidates)
        {
            await using var tx = await BeginAsync(cancellationToken);
            var access = await AuthorizeAsync(candidate.FamilyGroupId, candidate.Id, accountId,
                clock.GetUtcNow(), cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            // The list never records a denial; inaccessible Incidents are simply omitted.
            if (access is not null) views.Add(await BuildSummaryAsync(access, cancellationToken));
            await tx.CommitAsync(cancellationToken);
        }
        return views;
    }

    public Task<IncidentView?> GetAsync(Guid accountId, Guid incidentId, CancellationToken cancellationToken) =>
        ReadAsync(accountId, incidentId, auditDenial: true, cancellationToken);

    public async Task<IncidentView?> CreateAsync(Guid accountId, Guid familyGroupId,
        CreateIncidentRequest request, CancellationToken cancellationToken)
    {
        await using var tx = await BeginAsync(cancellationToken);
        var now = clock.GetUtcNow();
        if (!await LockFamilyGroupAsync(familyGroupId, cancellationToken))
            return await DenyWriteAsync(accountId, now, cancellationToken);

        var creator = await ActiveMembershipAsync(familyGroupId, m => m.AccountId == accountId, cancellationToken);
        var subject = await ActiveMembershipAsync(familyGroupId,
            m => m.AdultProfileId == request.SubjectProfileId, cancellationToken);
        var participantIds = request.ParticipantProfileIds!;
        if (creator is null || subject is null || participantIds.Contains(creator.AdultProfileId))
            return await DenyWriteAsync(accountId, now, cancellationToken);
        var participants = await GetEligibleMembershipsAsync(familyGroupId, participantIds, cancellationToken);
        if (participants is null) return await DenyWriteAsync(accountId, now, cancellationToken);
        // Lineage is checked read-only; it neither grants access nor changes the Concern.
        if (request.OriginatingConcernId is { } concernId
            && !await concernAccess.IsCurrentCreatorAsync(accountId, concernId, familyGroupId, cancellationToken))
            return await DenyWriteAsync(accountId, now, cancellationToken);

        var incidentId = Guid.NewGuid();
        var coordinatorGrant = new IncidentParticipantGrant(incidentId, accountId, creator.AdultProfileId,
            creator.Id, creator.AccessVersion, now, addedByProfileId: null);
        var incident = new SafetyIncident(incidentId, familyGroupId, accountId, creator.AdultProfileId,
            subject.AdultProfileId, coordinatorGrant.Id, request.OriginatingConcernId, now);
        db.Incidents.Add(incident);
        db.IncidentParticipantGrants.Add(coordinatorGrant);
        foreach (var membership in participants)
        {
            db.IncidentParticipantGrants.Add(new IncidentParticipantGrant(incidentId, membership.AccountId,
                membership.AdultProfileId, membership.Id, membership.AccessVersion, now, creator.AdultProfileId));
            db.IncidentAuditEvents.Add(Audit(incidentId, accountId, IncidentAuditActions.ParticipantAdded, now,
                affectedProfileId: membership.AdultProfileId));
        }

        // The creation reason becomes the first coordinator summary, with its provenance.
        var summary = new IncidentEntry(Guid.NewGuid(), incidentId, accountId, creator.AdultProfileId,
            IncidentEntryKind.Summary, request.ReasonCategory!.Value, isOutdated: false, request.Reason,
            request.ReasonSourceDescription, request.ReasonSourceUnknown, now, eventAt: null,
            eventTimeUnknown: false, relatedEntryId: null, relatedTaskId: null, explanation: null);
        db.IncidentEntries.Add(summary);
        db.IncidentAuditEvents.Add(Audit(incidentId, accountId, IncidentAuditActions.IncidentCreated, now,
            affectedProfileId: subject.AdultProfileId, entryId: summary.Id));
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return await ReadAsync(accountId, incidentId, auditDenial: false, cancellationToken);
    }

    public async Task<IncidentView?> ReplaceParticipantsAsync(Guid accountId, Guid incidentId,
        ReplaceIncidentParticipantsRequest request, CancellationToken cancellationToken)
    {
        var groupId = await GroupOfAsync(incidentId, cancellationToken);
        await using var tx = await BeginAsync(cancellationToken);
        var now = clock.GetUtcNow();
        var access = await AuthorizeAsync(groupId, incidentId, accountId, now, cancellationToken);
        var desiredIds = request.ParticipantProfileIds!;
        if (access is not { IsOpen: true, IsCoordinator: true } || desiredIds.Contains(access.Grant.ProfileId))
            return await DenyWriteAsync(accountId, now, cancellationToken);
        var memberships = await GetEligibleMembershipsAsync(access.Incident.FamilyGroupId, desiredIds, cancellationToken);
        if (memberships is null) return await DenyWriteAsync(accountId, now, cancellationToken);

        // After normalization every current grant matches its membership, so a kept profile keeps its grant
        // and a reselected person whose old grant was revoked always receives a new grant.
        var desired = memberships.ToDictionary(m => m.AdultProfileId);
        foreach (var grant in access.CurrentGrants.Where(g => g.Id != access.Incident.CoordinatorGrantId))
        {
            if (desired.Remove(grant.ProfileId)) continue;
            grant.Remove(access.Grant.ProfileId, now);
            db.IncidentAuditEvents.Add(Audit(incidentId, accountId, IncidentAuditActions.ParticipantRemoved, now,
                affectedProfileId: grant.ProfileId));
        }
        foreach (var membership in desired.Values)
        {
            db.IncidentParticipantGrants.Add(new IncidentParticipantGrant(incidentId, membership.AccountId,
                membership.AdultProfileId, membership.Id, membership.AccessVersion, now, access.Grant.ProfileId));
            db.IncidentAuditEvents.Add(Audit(incidentId, accountId, IncidentAuditActions.ParticipantAdded, now,
                affectedProfileId: membership.AdultProfileId));
        }
        access.Incident.RecordAuthorizedMutation(now);
        return await CommitAndReadAsync(tx, accountId, incidentId, cancellationToken);
    }

    public async Task<IncidentView?> TransferCoordinatorAsync(Guid accountId, Guid incidentId,
        TransferIncidentCoordinatorRequest request, CancellationToken cancellationToken)
    {
        var groupId = await GroupOfAsync(incidentId, cancellationToken);
        await using var tx = await BeginAsync(cancellationToken);
        var now = clock.GetUtcNow();
        var access = await AuthorizeAsync(groupId, incidentId, accountId, now, cancellationToken);
        if (access is not { IsOpen: true, IsCoordinator: true })
            return await DenyWriteAsync(accountId, now, cancellationToken);
        // The target must hold a current valid grant and must not already be the coordinator.
        var target = access.CurrentGrants.SingleOrDefault(g => g.ProfileId == request.ParticipantProfileId
            && g.Id != access.Grant.Id);
        if (target is null) return await DenyWriteAsync(accountId, now, cancellationToken);

        access.Incident.TransferCoordinator(target.Id, now);
        db.IncidentCoordinatorChanges.Add(new IncidentCoordinatorChange(incidentId, access.Grant.Id,
            access.Grant.ProfileId, target.Id, target.ProfileId, accountId, now));
        db.IncidentAuditEvents.Add(Audit(incidentId, accountId, IncidentAuditActions.CoordinatorTransferred, now,
            affectedProfileId: target.ProfileId, fromProfileId: access.Grant.ProfileId));
        return await CommitAndReadAsync(tx, accountId, incidentId, cancellationToken);
    }

    public async Task<IncidentView?> AddEntryAsync(Guid accountId, Guid incidentId,
        AddIncidentEntryRequest request, CancellationToken cancellationToken)
    {
        var groupId = await GroupOfAsync(incidentId, cancellationToken);
        await using var tx = await BeginAsync(cancellationToken);
        var now = clock.GetUtcNow();
        var access = await AuthorizeAsync(groupId, incidentId, accountId, now, cancellationToken);
        var kind = request.Kind!.Value;
        if (access is not { IsOpen: true } || (kind == IncidentEntryKind.Summary && !access.IsCoordinator))
            return await DenyWriteAsync(accountId, now, cancellationToken);
        if (request.RelatedEntryId is { } targetId)
        {
            var targetContributor = await db.IncidentEntries.AsNoTracking()
                .Where(e => e.Id == targetId && e.IncidentId == incidentId)
                .Select(e => (Guid?)e.ContributorAccountId).SingleOrDefaultAsync(cancellationToken);
            // A correction targets the caller's own entry; a challenge targets someone else's.
            if (targetContributor is null
                || (kind == IncidentEntryKind.Correction && targetContributor != accountId)
                || (kind == IncidentEntryKind.Challenge && targetContributor == accountId))
                return await DenyWriteAsync(accountId, now, cancellationToken);
        }

        var contactAttempt = kind == IncidentEntryKind.ContactAttempt
            ? new IncidentContactAttemptDetails(request.AttemptedByDescription, request.TargetLabel!,
                request.ContactMethod!.Value, request.ContactOutcome!.Value)
            : null;
        var entry = new IncidentEntry(Guid.NewGuid(), incidentId, accountId, access.Grant.ProfileId, kind,
            IncidentService.StoredCategory(request), request.Category == IncidentInformationCategory.Outdated,
            request.Text, request.SourceDescription, request.SourceUnknown, now, request.EventAt,
            request.EventTimeUnknown, request.RelatedEntryId, relatedTaskId: null, request.Explanation, contactAttempt);
        db.IncidentEntries.Add(entry);
        db.IncidentAuditEvents.Add(Audit(incidentId, accountId, IncidentAuditActions.ForEntry(kind), now,
            entryId: entry.Id));
        access.Incident.RecordAuthorizedMutation(now);
        return await CommitAndReadAsync(tx, accountId, incidentId, cancellationToken);
    }

    public async Task<IncidentView?> CreateTaskAsync(Guid accountId, Guid incidentId,
        CreateIncidentTaskRequest request, CancellationToken cancellationToken)
    {
        var groupId = await GroupOfAsync(incidentId, cancellationToken);
        await using var tx = await BeginAsync(cancellationToken);
        var now = clock.GetUtcNow();
        var access = await AuthorizeAsync(groupId, incidentId, accountId, now, cancellationToken);
        var assignee = access?.CurrentGrants.SingleOrDefault(g => g.ProfileId == request.AssigneeProfileId);
        // The coordinator may assign any current grant holder; a participant only themself.
        if (access is not { IsOpen: true } || assignee is null
            || (!access.IsCoordinator && assignee.Id != access.Grant.Id))
            return await DenyWriteAsync(accountId, now, cancellationToken);

        var task = new IncidentTask(Guid.NewGuid(), incidentId, request.Description!, assignee.AccountId,
            assignee.ProfileId, assignee.Id, accountId, access.Grant.ProfileId, now);
        db.IncidentTasks.Add(task);
        db.IncidentAuditEvents.Add(Audit(incidentId, accountId, IncidentAuditActions.TaskCreated, now,
            affectedProfileId: assignee.ProfileId, taskId: task.Id));
        access.Incident.RecordAuthorizedMutation(now);
        return await CommitAndReadAsync(tx, accountId, incidentId, cancellationToken);
    }

    public async Task<IncidentView?> CompleteTaskAsync(Guid accountId, Guid incidentId, Guid taskId,
        CompleteIncidentTaskRequest request, CancellationToken cancellationToken)
    {
        var groupId = await GroupOfAsync(incidentId, cancellationToken);
        await using var tx = await BeginAsync(cancellationToken);
        var now = clock.GetUtcNow();
        var access = await AuthorizeAsync(groupId, incidentId, accountId, now, cancellationToken);
        var task = access is { IsOpen: true } ? await LoadOpenTaskAsync(incidentId, taskId, cancellationToken) : null;
        // Only the assignee, still holding the same grant that was valid at assignment.
        if (access is null || task is null || task.AssigneeGrantId != access.Grant.Id)
            return await DenyWriteAsync(accountId, now, cancellationToken);

        var result = new IncidentEntry(Guid.NewGuid(), incidentId, accountId, access.Grant.ProfileId,
            IncidentEntryKind.TaskResult, request.Category!.Value, isOutdated: false, request.ResultText,
            request.SourceDescription, request.SourceUnknown, now, request.EventAt, eventTimeUnknown: false,
            relatedEntryId: null, relatedTaskId: task.Id, explanation: null);
        task.Complete(access.Grant.Id, now);
        db.IncidentEntries.Add(result);
        db.IncidentAuditEvents.Add(Audit(incidentId, accountId, IncidentAuditActions.TaskCompleted, now,
            affectedProfileId: task.AssigneeProfileId, entryId: result.Id, taskId: task.Id));
        access.Incident.RecordAuthorizedMutation(now);
        return await CommitAndReadAsync(tx, accountId, incidentId, cancellationToken);
    }

    public async Task<IncidentView?> CancelTaskAsync(Guid accountId, Guid incidentId, Guid taskId,
        CancelIncidentTaskRequest request, CancellationToken cancellationToken)
    {
        var groupId = await GroupOfAsync(incidentId, cancellationToken);
        await using var tx = await BeginAsync(cancellationToken);
        var now = clock.GetUtcNow();
        var access = await AuthorizeAsync(groupId, incidentId, accountId, now, cancellationToken);
        var task = access is { IsOpen: true } ? await LoadOpenTaskAsync(incidentId, taskId, cancellationToken) : null;
        // The coordinator may cancel any Open task; a participant only one they created for themself
        // under their current grant.
        if (access is null || task is null
            || (!access.IsCoordinator && !(task.IsSelfAssignedBy(accountId) && task.AssigneeGrantId == access.Grant.Id)))
            return await DenyWriteAsync(accountId, now, cancellationToken);

        task.Cancel(accountId, access.Grant.ProfileId, request.Reason!, now);
        db.IncidentAuditEvents.Add(Audit(incidentId, accountId, IncidentAuditActions.TaskCancelled, now,
            affectedProfileId: task.AssigneeProfileId, taskId: task.Id));
        access.Incident.RecordAuthorizedMutation(now);
        return await CommitAndReadAsync(tx, accountId, incidentId, cancellationToken);
    }

    public async Task<IncidentView?> ResolveAsync(Guid accountId, Guid incidentId,
        ResolveIncidentRequest request, CancellationToken cancellationToken)
    {
        var groupId = await GroupOfAsync(incidentId, cancellationToken);
        await using var tx = await BeginAsync(cancellationToken);
        var now = clock.GetUtcNow();
        var access = await AuthorizeAsync(groupId, incidentId, accountId, now, cancellationToken);
        if (access is not { IsOpen: true, IsCoordinator: true })
            return await DenyWriteAsync(accountId, now, cancellationToken);

        access.Incident.Resolve(accountId, access.Grant.ProfileId, new IncidentResolution(request.Summary!,
            request.OutcomeText!, request.OutcomeCategory!.Value, request.OutcomeSourceDescription,
            request.OutcomeSourceUnknown, request.ContactRestoredAt, request.ContactRestoredTimeUnknown,
            request.AssistanceProvided, request.UnresolvedFollowUp, request.Lessons), now);
        db.IncidentAuditEvents.Add(Audit(incidentId, accountId, IncidentAuditActions.Resolved, now));
        return await CommitAndReadAsync(tx, accountId, incidentId, cancellationToken);
    }

    /// <summary>
    /// Materializes due freezes and grant revocations, deletes expired Incident aggregates, and deletes
    /// audit rows past 365 days. Candidates are selected set-based; each is processed in its own
    /// Serializable, group-locked transaction, so repeated or concurrent runs are idempotent.
    /// </summary>
    public async Task ReconcileAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var inactivityCutoff = now.AddDays(-SafetyIncident.InactivityFreezeDays);
        var expiryCutoff = now.AddDays(-SafetyIncident.RetentionDays);
        for (var batch = 0; batch < MaxReconciliationBatches; batch++)
        {
            var candidates = await db.Incidents.AsNoTracking()
                .Where(i => (i.State == IncidentState.Open && i.LastAuthorizedMutationAt <= inactivityCutoff)
                    || (i.State == IncidentState.Frozen && i.FrozenAt <= expiryCutoff)
                    || (i.State == IncidentState.Resolved && i.ResolvedAt <= expiryCutoff)
                    || db.IncidentParticipantGrants.Any(g => g.IncidentId == i.Id && g.RemovedAt == null
                        && !db.FamilyGroupMemberships.Any(m => m.Id == g.MembershipId && m.IsActive
                            && m.AccountId == g.AccountId && m.FamilyGroupId == i.FamilyGroupId
                            && m.AccessVersion == g.MembershipAccessVersion)))
                .OrderBy(i => i.Id).Select(i => new { i.Id, i.FamilyGroupId })
                .Take(ReconciliationBatchSize).ToListAsync(cancellationToken);
            foreach (var candidate in candidates)
                await ReconcileIncidentAsync(candidate.Id, candidate.FamilyGroupId, now, cancellationToken);
            if (candidates.Count < ReconciliationBatchSize) break;
        }
        await db.IncidentAuditEvents.Where(e => e.OccurredAt <= expiryCutoff).ExecuteDeleteAsync(cancellationToken);
    }

    private async Task ReconcileIncidentAsync(Guid incidentId, Guid familyGroupId, DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        await using var tx = await BeginAsync(cancellationToken);
        if (!await LockFamilyGroupAsync(familyGroupId, cancellationToken)) return;
        ForgetTrackedAggregate(incidentId);
        var incident = await db.Incidents.SingleOrDefaultAsync(i => i.Id == incidentId, cancellationToken);
        if (incident is null)
        {
            await tx.CommitAsync(cancellationToken);
            return;
        }
        await NormalizeAsync(incident, now, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        var expired = incident.IsExpired(now);
        if (expired) await DeleteAggregateAsync(incidentId, cancellationToken);
        await tx.CommitAsync(cancellationToken);
        if (expired) ForgetTrackedAggregate(incidentId);
    }

    // Set-based deletion in dependency order; nothing is loaded into memory. Audit rows are kept.
    private async Task DeleteAggregateAsync(Guid incidentId, CancellationToken cancellationToken)
    {
        await db.IncidentEntries.Where(e => e.IncidentId == incidentId).ExecuteDeleteAsync(cancellationToken);
        await db.IncidentTasks.Where(t => t.IncidentId == incidentId).ExecuteDeleteAsync(cancellationToken);
        await db.IncidentCoordinatorChanges.Where(c => c.IncidentId == incidentId).ExecuteDeleteAsync(cancellationToken);
        await db.IncidentParticipantGrants.Where(g => g.IncidentId == incidentId).ExecuteDeleteAsync(cancellationToken);
        await db.Incidents.Where(i => i.Id == incidentId).ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>
    /// Detaches any previously tracked rows of this Incident aggregate. EF returns already-tracked
    /// entities without refreshing them, so this guarantees that every decision made after taking the
    /// Family Group lock reads committed state, even if the context outlives a single request.
    /// </summary>
    private void ForgetTrackedAggregate(Guid incidentId)
    {
        foreach (var tracked in db.ChangeTracker.Entries().ToList())
        {
            var belongs = tracked.Entity switch
            {
                SafetyIncident i => i.Id == incidentId,
                IncidentParticipantGrant g => g.IncidentId == incidentId,
                IncidentCoordinatorChange c => c.IncidentId == incidentId,
                IncidentEntry e => e.IncidentId == incidentId,
                IncidentTask t => t.IncidentId == incidentId,
                _ => false,
            };
            if (belongs) tracked.State = EntityState.Detached;
        }
    }

    private async Task<IncidentView?> ReadAsync(Guid accountId, Guid incidentId, bool auditDenial,
        CancellationToken cancellationToken)
    {
        var groupId = await GroupOfAsync(incidentId, cancellationToken);
        await using var tx = await BeginAsync(cancellationToken);
        var now = clock.GetUtcNow();
        var access = await AuthorizeAsync(groupId, incidentId, accountId, now, cancellationToken);
        if (access is null)
        {
            if (auditDenial) await AddDeniedAuditAsync(accountId, IncidentAuditEvent.ReadDenied, now, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return null;
        }
        await db.SaveChangesAsync(cancellationToken);
        var view = await BuildViewAsync(access, accountId, cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return view;
    }

    /// <summary>
    /// Locks the Family Group, loads and normalizes the Incident, and returns the caller's current access.
    /// Null for a missing, expired, or unauthorized Incident. Normalization changes are tracked and are
    /// saved by the caller on both the allowed and denied paths.
    /// </summary>
    private async Task<IncidentAccess?> AuthorizeAsync(Guid? familyGroupId, Guid incidentId, Guid accountId,
        DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (familyGroupId is not { } groupId || !await LockFamilyGroupAsync(groupId, cancellationToken)) return null;
        ForgetTrackedAggregate(incidentId);
        var incident = await db.Incidents.SingleOrDefaultAsync(i => i.Id == incidentId, cancellationToken);
        if (incident is null) return null;
        var currentGrants = await NormalizeAsync(incident, now, cancellationToken);
        if (incident.IsExpired(now)) return null;
        var grant = currentGrants.SingleOrDefault(g => g.AccountId == accountId);
        return grant is null ? null : new IncidentAccess(incident, grant, currentGrants);
    }

    /// <summary>
    /// Applies the effective lifecycle once: revokes grants whose membership no longer matches (at the
    /// actual loss time), and freezes an Open Incident at the earlier of the inactivity deadline and the
    /// coordinator's loss time. Returns the grants that remain current.
    /// </summary>
    private async Task<List<IncidentParticipantGrant>> NormalizeAsync(SafetyIncident incident, DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var grants = await db.IncidentParticipantGrants
            .Where(g => g.IncidentId == incident.Id && (g.RemovedAt == null || g.Id == incident.CoordinatorGrantId))
            .ToListAsync(cancellationToken);
        var membershipIds = grants.Select(g => g.MembershipId).Distinct().ToList();
        var memberships = await db.FamilyGroupMemberships.AsNoTracking()
            .Where(m => membershipIds.Contains(m.Id)).ToDictionaryAsync(m => m.Id, cancellationToken);

        DateTimeOffset LossTime(IncidentParticipantGrant grant) =>
            memberships.GetValueOrDefault(grant.MembershipId)?.EndedAt ?? now;
        bool IsValid(IncidentParticipantGrant grant) =>
            memberships.GetValueOrDefault(grant.MembershipId) is { } m && m.FamilyGroupId == incident.FamilyGroupId
            && m.AdultProfileId == grant.ProfileId && grant.MatchesMembership(m.Id, m.AccountId, m.IsActive, m.AccessVersion);

        var coordinator = grants.SingleOrDefault(g => g.Id == incident.CoordinatorGrantId);
        DateTimeOffset? coordinatorLostAt = coordinator is null ? now
            : coordinator.RemovedAt ?? (IsValid(coordinator) ? null : LossTime(coordinator));

        foreach (var grant in grants.Where(g => g.IsCurrent && !IsValid(g)))
        {
            var lostAt = LossTime(grant);
            if (grant.RevokeForEligibilityLoss(lostAt))
                db.IncidentAuditEvents.Add(Audit(incident.Id, null, IncidentAuditActions.AccessRevoked, lostAt,
                    authorized: false, affectedProfileId: grant.ProfileId));
        }

        if (incident.State == IncidentState.Open && (now >= incident.InactivityFreezeAt || coordinatorLostAt is not null))
        {
            var freezeAt = incident.EffectiveFreezeAt(coordinatorLostAt)!.Value;
            if (incident.Freeze(freezeAt, incident.EffectiveFreezeReason(coordinatorLostAt)))
                db.IncidentAuditEvents.Add(Audit(incident.Id, null, IncidentAuditActions.Frozen, freezeAt));
        }
        return [.. grants.Where(g => g.IsCurrent)];
    }

    private async Task<IncidentView> BuildViewAsync(IncidentAccess access, Guid accountId,
        CancellationToken cancellationToken)
    {
        var incident = access.Incident;
        var grants = await db.IncidentParticipantGrants.AsNoTracking().Where(g => g.IncidentId == incident.Id)
            .OrderBy(g => g.SelectedAt).ThenBy(g => g.Id).ToListAsync(cancellationToken);
        var changes = await db.IncidentCoordinatorChanges.AsNoTracking().Where(c => c.IncidentId == incident.Id)
            .ToListAsync(cancellationToken);
        var entries = await db.IncidentEntries.AsNoTracking().Where(e => e.IncidentId == incident.Id)
            .OrderBy(e => e.RecordedAt).ThenBy(e => e.Id).ToListAsync(cancellationToken);
        var tasks = await db.IncidentTasks.AsNoTracking().Where(t => t.IncidentId == incident.Id)
            .OrderBy(t => t.CreatedAt).ThenBy(t => t.Id).ToListAsync(cancellationToken);

        var profileIds = new HashSet<Guid> { incident.SubjectProfileId, incident.CreatedByProfileId };
        if (incident.ResolvedByProfileId is { } resolver) profileIds.Add(resolver);
        foreach (var g in grants) profileIds.UnionWith(Ids(g.ProfileId, g.AddedByProfileId, g.RemovedByProfileId));
        foreach (var c in changes) profileIds.UnionWith(Ids(c.FromProfileId, c.ToProfileId));
        foreach (var e in entries) profileIds.Add(e.ContributorProfileId);
        foreach (var t in tasks) profileIds.UnionWith(Ids(t.AssigneeProfileId, t.CreatedByProfileId, t.CancelledByProfileId));
        var names = await DisplayNamesAsync(profileIds, cancellationToken);
        string Name(Guid profileId) => names.GetValueOrDefault(profileId, MemberFallbackName);

        var coordinatorProfileId = grants.Single(g => g.Id == incident.CoordinatorGrantId).ProfileId;
        var participants = grants.Where(g => g.IsCurrent)
            .Select(g => new IncidentParticipantView(g.ProfileId, Name(g.ProfileId),
                g.Id == incident.CoordinatorGrantId, g.SelectedAt)).ToList();
        var entryViews = entries.Select(e => new IncidentEntryView(e.Id, e.Kind, e.Category, e.IsOutdated, e.Text,
            e.SourceDescription, e.SourceUnknown, e.ContributorProfileId, Name(e.ContributorProfileId), e.RecordedAt,
            e.EventAt, e.EventTimeUnknown, e.RelatedEntryId, e.RelatedTaskId, e.Explanation, e.AttemptedByDescription,
            e.TargetLabel, e.ContactMethod, e.ContactOutcome)).ToList();
        var taskViews = tasks.Select(t => new IncidentTaskView(t.Id, t.Description, t.AssigneeProfileId,
            Name(t.AssigneeProfileId), t.Status,
            // Open tasks become read-only at freeze or resolution and are shown as not completed.
            NotCompleted: t.Status == IncidentTaskStatus.Open && incident.State != IncidentState.Open,
            t.CreatedByProfileId, Name(t.CreatedByProfileId), t.CreatedAt, t.CompletedAt,
            entries.FirstOrDefault(e => e.RelatedTaskId == t.Id)?.Id, t.CancelledAt, t.CancelledByProfileId,
            t.CancelledByProfileId is { } canceller ? Name(canceller) : null, t.CancelReason)).ToList();
        var resolution = incident is { State: IncidentState.Resolved, ResolvedAt: { } resolvedAt,
            ResolvedByProfileId: { } resolvedBy, OutcomeCategory: { } outcomeCategory }
            ? new IncidentResolutionView(incident.ResolutionSummary!, incident.OutcomeText!, outcomeCategory,
                incident.OutcomeSourceDescription, incident.OutcomeSourceUnknown, incident.ContactRestoredAt,
                incident.ContactRestoredTimeUnknown, incident.AssistanceProvided, incident.UnresolvedFollowUp,
                incident.Lessons, resolvedBy, Name(resolvedBy), resolvedAt)
            : null;

        // Lineage is shown only to a caller currently authorized for both records; otherwise the
        // response is indistinguishable from an Incident without lineage.
        var lineage = incident.OriginatingConcernId is { } concernId
            && await concernAccess.CanReadAsync(accountId, concernId, cancellationToken) ? concernId : (Guid?)null;

        return new IncidentView(incident.Id, incident.FamilyGroupId,
            await FamilyGroupNameAsync(incident.FamilyGroupId, cancellationToken),
            incident.SubjectProfileId, Name(incident.SubjectProfileId),
            incident.CreatedByProfileId, Name(incident.CreatedByProfileId),
            coordinatorProfileId, Name(coordinatorProfileId), incident.State, incident.CreatedAt,
            incident.LastAuthorizedMutationAt,
            incident.State == IncidentState.Open ? incident.InactivityFreezeAt : null,
            incident.FrozenAt, incident.FreezeReason, incident.ExpiresAt, resolution, lineage,
            participants, entryViews, taskViews,
            IncidentTimeline.Build(incident, grants, changes, entries, tasks, Name));
    }

    private async Task<IncidentSummaryView> BuildSummaryAsync(IncidentAccess access, CancellationToken cancellationToken)
    {
        var incident = access.Incident;
        var coordinatorProfileId = await db.IncidentParticipantGrants.AsNoTracking()
            .Where(g => g.Id == incident.CoordinatorGrantId).Select(g => g.ProfileId).SingleAsync(cancellationToken);
        var names = await DisplayNamesAsync([incident.SubjectProfileId, coordinatorProfileId], cancellationToken);
        return new IncidentSummaryView(incident.Id, incident.FamilyGroupId,
            await FamilyGroupNameAsync(incident.FamilyGroupId, cancellationToken), incident.SubjectProfileId,
            names.GetValueOrDefault(incident.SubjectProfileId, MemberFallbackName), incident.State,
            coordinatorProfileId, names.GetValueOrDefault(coordinatorProfileId, MemberFallbackName),
            incident.CreatedAt, incident.LastAuthorizedMutationAt, incident.ExpiresAt);
    }

    private async Task<IncidentView?> CommitAndReadAsync(IDbContextTransaction tx,
        Guid accountId, Guid incidentId, CancellationToken cancellationToken)
    {
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return await ReadAsync(accountId, incidentId, auditDenial: false, cancellationToken);
    }

    private async Task<IncidentView?> DenyWriteAsync(Guid accountId, DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        // Persists any lifecycle normalization already tracked, plus the hourly denied summary.
        await AddDeniedAuditAsync(accountId, IncidentAuditEvent.WriteDenied, now, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await db.Database.CurrentTransaction!.CommitAsync(cancellationToken);
        return null;
    }

    // DEC-069: one summary per actor, broad category, and UTC hour, with no Incident ID. UPDLOCK plus
    // SERIALIZABLE key-range locking on the unique index makes concurrent increments safe.
    private async Task AddDeniedAuditAsync(Guid accountId, string category, DateTimeOffset at,
        CancellationToken cancellationToken)
    {
        var bucket = IncidentAuditEvent.HourBucket(at);
        var updated = await db.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE summary
SET [LastAttemptAt] = {at}, [DeniedAttemptCount] = [DeniedAttemptCount] + 1
FROM [IncidentAuditEvents] AS summary WITH (UPDLOCK, SERIALIZABLE, INDEX([IX_IncidentAuditEvents_ActorAccountId_Action_UtcHourBucket]))
WHERE [ActorAccountId] = {accountId} AND [Action] = {category}
  AND [UtcHourBucket] = {bucket} AND [DeniedAttemptCount] > 0;", cancellationToken);
        if (updated == 0)
            db.IncidentAuditEvents.Add(IncidentAuditEvent.DeniedSummary(accountId, category, bucket, at, at));
    }

    private Task<IncidentTask?> LoadOpenTaskAsync(Guid incidentId, Guid taskId, CancellationToken cancellationToken) =>
        db.IncidentTasks.SingleOrDefaultAsync(t => t.Id == taskId && t.IncidentId == incidentId
            && t.Status == IncidentTaskStatus.Open, cancellationToken);

    private Task<Guid?> GroupOfAsync(Guid incidentId, CancellationToken cancellationToken) =>
        db.Incidents.AsNoTracking().Where(i => i.Id == incidentId)
            .Select(i => (Guid?)i.FamilyGroupId).SingleOrDefaultAsync(cancellationToken);

    private Task<IDbContextTransaction> BeginAsync(
        CancellationToken cancellationToken) =>
        db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

    private async Task<bool> LockFamilyGroupAsync(Guid groupId, CancellationToken cancellationToken) =>
        await db.FamilyGroups.Where(g => g.Id == groupId)
            .ExecuteUpdateAsync(update => update.SetProperty(g => g.CapacityLimit, g => g.CapacityLimit), cancellationToken) == 1;

    private Task<FamilyGroupMembership?> ActiveMembershipAsync(Guid familyGroupId,
        Expression<Func<FamilyGroupMembership, bool>> who, CancellationToken cancellationToken) =>
        db.FamilyGroupMemberships.AsNoTracking().Where(m => m.FamilyGroupId == familyGroupId && m.IsActive)
            .Where(who).SingleOrDefaultAsync(cancellationToken);

    private async Task<List<FamilyGroupMembership>?> GetEligibleMembershipsAsync(Guid groupId,
        IReadOnlyList<Guid> profileIds, CancellationToken cancellationToken)
    {
        if (profileIds.Count > IncidentService.MaxParticipants) return null;
        var rows = await db.FamilyGroupMemberships.AsNoTracking().Where(m => m.FamilyGroupId == groupId
            && m.IsActive && profileIds.Contains(m.AdultProfileId)).ToListAsync(cancellationToken);
        return rows.Count == profileIds.Count ? rows : null;
    }

    // Only ordinary display names are read; no profile contact or other protected field.
    private Task<Dictionary<Guid, string>> DisplayNamesAsync(IReadOnlyCollection<Guid> profileIds,
        CancellationToken cancellationToken) =>
        db.AdultProfiles.AsNoTracking().Where(p => profileIds.Contains(p.Id))
            .Select(p => new { p.Id, p.DisplayName })
            .ToDictionaryAsync(p => p.Id, p => p.DisplayName, cancellationToken);

    private Task<string> FamilyGroupNameAsync(Guid familyGroupId, CancellationToken cancellationToken) =>
        db.FamilyGroups.AsNoTracking().Where(g => g.Id == familyGroupId).Select(g => g.Name)
            .SingleAsync(cancellationToken);

    private static IEnumerable<Guid> Ids(params Guid?[] ids) => ids.OfType<Guid>();

    private static IncidentAuditEvent Audit(Guid incidentId, Guid? actorAccountId, string action,
        DateTimeOffset at, bool authorized = true, Guid? affectedProfileId = null, Guid? fromProfileId = null,
        Guid? entryId = null, Guid? taskId = null) =>
        new(Guid.NewGuid(), incidentId, actorAccountId, action, at, authorized, affectedProfileId,
            fromProfileId, entryId, taskId);

    private sealed record IncidentAccess(SafetyIncident Incident, IncidentParticipantGrant Grant,
        IReadOnlyList<IncidentParticipantGrant> CurrentGrants)
    {
        public bool IsOpen => Incident.State == IncidentState.Open;
        public bool IsCoordinator => Grant.Id == Incident.CoordinatorGrantId;
    }
}

/// <summary>Successful Incident audit action names (metadata only).</summary>
public static class IncidentAuditActions
{
    public const string IncidentCreated = "IncidentCreated";
    public const string ParticipantAdded = "ParticipantAdded";
    public const string ParticipantRemoved = "ParticipantRemoved";
    public const string CoordinatorTransferred = "CoordinatorTransferred";
    public const string TaskCreated = "TaskCreated";
    public const string TaskCompleted = "TaskCompleted";
    public const string TaskCancelled = "TaskCancelled";
    public const string Frozen = "Frozen";
    public const string Resolved = "Resolved";
    public const string AccessRevoked = "AccessRevoked";

    public static string ForEntry(IncidentEntryKind kind) => kind switch
    {
        IncidentEntryKind.Observation => "ObservationCreated",
        IncidentEntryKind.Unknown => "UnknownCreated",
        IncidentEntryKind.Summary => "SummaryCreated",
        IncidentEntryKind.ContactAttempt => "ContactAttemptRecorded",
        IncidentEntryKind.Correction => "CorrectionCreated",
        IncidentEntryKind.Challenge => "ChallengeCreated",
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
}
