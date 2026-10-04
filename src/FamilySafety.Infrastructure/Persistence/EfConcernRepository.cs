using System.Data;
using FamilySafety.Application.Concerns;
using FamilySafety.Domain.Concerns;
using FamilySafety.Domain.Profiles;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Infrastructure.Persistence;

public sealed class EfConcernRepository(FamilySafetyDbContext db, TimeProvider clock) : IConcernRepository
{
    private const int MaxParticipants = 250;

    public async Task<IReadOnlyList<ConcernView>> GetVisibleAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var profileId = await GetProfileIdAsync(accountId, cancellationToken);
        if (profileId is null) return [];
        var now = clock.GetUtcNow();
        var ids = await db.Concerns.AsNoTracking()
            .Where(c => c.CreatorAccountId == accountId
                || db.ConcernParticipantGrants.Any(g => g.ConcernId == c.Id && g.AccountId == accountId
                    && g.RemovedAt == null))
            .OrderByDescending(c => c.CreatedAt).Select(c => c.Id).ToListAsync(cancellationToken);
        var views = new List<ConcernView>(ids.Count);
        foreach (var id in ids)
        {
            var view = await ReadAuthorizedAsync(accountId, profileId.Value, id, now, auditDenial: false, cancellationToken);
            if (view is not null) views.Add(view);
        }
        return views;
    }

    public async Task<ConcernView?> GetAsync(Guid accountId, Guid concernId, CancellationToken cancellationToken)
    {
        var profileId = await GetProfileIdAsync(accountId, cancellationToken);
        if (profileId is null)
        {
            await using var deniedTx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            await AddDeniedAuditAsync(accountId, "SensitiveReadDenied", clock.GetUtcNow(), cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await deniedTx.CommitAsync(cancellationToken);
            return null;
        }
        return await ReadAuthorizedAsync(accountId, profileId.Value, concernId,
            clock.GetUtcNow(), auditDenial: true, cancellationToken);
    }

    public async Task<ConcernView?> CreateAsync(Guid accountId, Guid familyGroupId,
        CreateConcernRequest request, CancellationToken cancellationToken)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var now = clock.GetUtcNow();
        if (!await LockFamilyGroupAsync(familyGroupId, cancellationToken))
        {
            await AddDeniedAuditAsync(accountId, "ConcernCreateDenied", now, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return null;
        }
        var creator = await ActiveMembershipAsync(accountId, familyGroupId, cancellationToken);
        var subject = await db.FamilyGroupMemberships.AsNoTracking()
            .SingleOrDefaultAsync(m => m.FamilyGroupId == familyGroupId
                && m.AdultProfileId == request.SubjectProfileId && m.IsActive, cancellationToken);
        if (creator is null || subject is null)
        {
            await AddDeniedAuditAsync(accountId, "ConcernCreateDenied", now, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return null;
        }
        var memberships = await GetEligibleMembershipsAsync(familyGroupId, request.ParticipantProfileIds!, cancellationToken);
        if (memberships is null)
        {
            await AddDeniedAuditAsync(accountId, "ConcernCreateDenied", now, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return null;
        }

        var concern = new Concern(Guid.NewGuid(), familyGroupId, accountId, creator.AdultProfileId,
            creator.Id, creator.AccessVersion, request.SubjectProfileId, request.Reason!, now);
        db.Concerns.Add(concern);
        db.ConcernParticipantGrants.AddRange(memberships.Select(m =>
            new ConcernParticipantGrant(concern.Id, m.AccountId, m.AdultProfileId, m.Id, m.AccessVersion, now)));
        db.ConcernAuditEvents.Add(Audit(concern.Id, accountId, "ConcernCreated", now, true));
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return await ReadAuthorizedAsync(accountId, creator.AdultProfileId, concern.Id, now, false, cancellationToken);
    }

    public async Task<ConcernView?> ReplaceParticipantsAsync(Guid accountId, Guid concernId,
        ReplaceConcernParticipantsRequest request, CancellationToken cancellationToken)
    {
        var c = await LoadForCommandAsync(accountId, concernId, "ParticipantChangeDenied", cancellationToken);
        if (c is null) return null;
        var now = clock.GetUtcNow();
        var creatorMembership = await CurrentGrantMembershipAsync(c.CreatorMembershipId,
            c.CreatorProfileId, c.FamilyGroupId, cancellationToken);
        NormalizeLifecycle(c, creatorMembership, now);
        if (c.IsExpired(now) || c.State != ConcernState.Open || c.CreatorAccountId != accountId
            || !Matches(c.CreatorMembershipId, c.CreatorMembershipAccessVersion, creatorMembership))
        {
            await AddDeniedAuditAsync(accountId, "ParticipantChangeDenied", now, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await db.Database.CurrentTransaction!.CommitAsync(cancellationToken);
            return null;
        }
        var memberships = await GetEligibleMembershipsAsync(c.FamilyGroupId, request.ParticipantProfileIds!, cancellationToken);
        if (memberships is null)
        {
            await AddDeniedAuditAsync(accountId, "ParticipantChangeDenied", now, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await db.Database.CurrentTransaction!.CommitAsync(cancellationToken);
            return null;
        }
        var current = await db.ConcernParticipantGrants.Where(g => g.ConcernId == c.Id && g.RemovedAt == null)
            .ToListAsync(cancellationToken);
        var desiredByProfile = memberships.ToDictionary(m => m.AdultProfileId);
        foreach (var grant in current)
        {
            if (!desiredByProfile.TryGetValue(grant.ProfileId, out var desired)
                || grant.MembershipId != desired.Id || grant.MembershipAccessVersion != desired.AccessVersion)
            {
                grant.Remove(now);
                db.ConcernAuditEvents.Add(Audit(c.Id, accountId, "ParticipantRemoved", now, true, grant.ProfileId));
            }
            else desiredByProfile.Remove(grant.ProfileId);
        }
        foreach (var membership in desiredByProfile.Values)
        {
            db.ConcernParticipantGrants.Add(new ConcernParticipantGrant(c.Id, membership.AccountId,
                membership.AdultProfileId, membership.Id, membership.AccessVersion, now));
            db.ConcernAuditEvents.Add(Audit(c.Id, accountId, "ParticipantAdded", now, true, membership.AdultProfileId));
        }
        c.RecordAuthorizedMutation(now);
        db.ConcernAuditEvents.Add(Audit(c.Id, accountId, "ParticipantChange", now, true));
        await db.SaveChangesAsync(cancellationToken);
        await db.Database.CurrentTransaction!.CommitAsync(cancellationToken);
        return await GetAsync(accountId, c.Id, cancellationToken);
    }

    public async Task<ConcernView?> AddEntryAsync(Guid accountId, Guid concernId,
        AddConcernEntryRequest request, CancellationToken cancellationToken)
    {
        var c = await LoadForCommandAsync(accountId, concernId, "ContributionDenied", cancellationToken);
        if (c is null) return null;
        var now = clock.GetUtcNow();
        var caller = await CurrentActorMembershipAsync(accountId, c.FamilyGroupId, cancellationToken);
        var creatorMembership = await CurrentGrantMembershipAsync(c.CreatorMembershipId,
            c.CreatorProfileId, c.FamilyGroupId, cancellationToken);
        NormalizeLifecycle(c, creatorMembership, now);
        var canParticipate = IsCreator(c, accountId, caller)
            || await HasParticipantGrantAsync(c.Id, accountId, caller, cancellationToken);
        if (c.IsExpired(now) || c.State != ConcernState.Open || !canParticipate)
        {
            await AddDeniedAuditAsync(accountId, "ContributionDenied", now, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await db.Database.CurrentTransaction!.CommitAsync(cancellationToken);
            return null;
        }

        ConcernEntry? target = null;
        if (request.RelatedEntryId is { } targetId)
        {
            target = await db.ConcernEntries.SingleOrDefaultAsync(e => e.Id == targetId && e.ConcernId == c.Id, cancellationToken);
            if (target is null
                || (request.Kind == ConcernEntryKind.Correction && target.ContributorAccountId != accountId)
                || (request.Kind == ConcernEntryKind.Challenge && target.ContributorAccountId == accountId))
            {
                await AddDeniedAuditAsync(accountId, "ContributionDenied", now, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                await db.Database.CurrentTransaction!.CommitAsync(cancellationToken);
                return null;
            }
        }
        var profileId = caller?.AdultProfileId ?? c.CreatorProfileId;
        var entry = new ConcernEntry(Guid.NewGuid(), c.Id, accountId, profileId,
            request.Kind, request.Category == ConcernInformationCategory.Outdated
                ? request.OriginalCategory!.Value : request.Category, request.Text!, request.SourceDescription,
            request.SourceUnknown, now, request.EventAt, request.RelatedEntryId,
            request.Explanation, request.IsOutdated || request.Category == ConcernInformationCategory.Outdated);
        db.ConcernEntries.Add(entry);
        c.RecordAuthorizedMutation(now);
        db.ConcernAuditEvents.Add(Audit(c.Id, accountId,
            request.Kind switch
            {
                ConcernEntryKind.Correction => "CorrectionCreated",
                ConcernEntryKind.Challenge => "ChallengeCreated",
                _ => "Contribution",
            }, now, true));
        await db.SaveChangesAsync(cancellationToken);
        await db.Database.CurrentTransaction!.CommitAsync(cancellationToken);
        return await GetAsync(accountId, c.Id, cancellationToken);
    }

    public async Task<ConcernView?> ResolveAsync(Guid accountId, Guid concernId,
        ResolveConcernRequest request, CancellationToken cancellationToken)
    {
        var c = await LoadForCommandAsync(accountId, concernId, "ResolutionDenied", cancellationToken);
        if (c is null) return null;
        var now = clock.GetUtcNow();
        var membership = await CurrentGrantMembershipAsync(c.CreatorMembershipId,
            c.CreatorProfileId, c.FamilyGroupId, cancellationToken);
        NormalizeLifecycle(c, membership, now);
        if (c.IsExpired(now) || c.State != ConcernState.Open || c.CreatorAccountId != accountId
            || !Matches(c.CreatorMembershipId, c.CreatorMembershipAccessVersion, membership))
        {
            await AddDeniedAuditAsync(accountId, "ResolutionDenied", now, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await db.Database.CurrentTransaction!.CommitAsync(cancellationToken);
            return null;
        }
        c.Resolve(accountId, request.Note!, now);
        db.ConcernAuditEvents.Add(Audit(c.Id, accountId, "Resolved", now, true));
        await db.SaveChangesAsync(cancellationToken);
        await db.Database.CurrentTransaction!.CommitAsync(cancellationToken);
        return await GetAsync(accountId, c.Id, cancellationToken);
    }

    public async Task ReconcileAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var concerns = await db.Concerns.AsNoTracking()
            .Select(c => new { c.Id, c.FamilyGroupId }).ToListAsync(cancellationToken);
        foreach (var item in concerns)
        {
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            if (!await LockFamilyGroupAsync(item.FamilyGroupId, cancellationToken)) continue;
            var c = await db.Concerns.SingleOrDefaultAsync(row => row.Id == item.Id, cancellationToken);
            if (c is null) continue;
            var creatorMembership = await CurrentGrantMembershipAsync(c.CreatorMembershipId,
                c.CreatorProfileId, c.FamilyGroupId, cancellationToken);
            NormalizeLifecycle(c, creatorMembership, now);
            await RevokeLostParticipantsAsync(c, now, cancellationToken);
            if (c.IsExpired(now))
            {
                db.ConcernEntries.RemoveRange(await db.ConcernEntries.Where(e => e.ConcernId == item.Id).ToListAsync(cancellationToken));
                db.ConcernParticipantGrants.RemoveRange(await db.ConcernParticipantGrants
                    .Where(g => g.ConcernId == item.Id).ToListAsync(cancellationToken));
                db.Concerns.Remove(c);
            }
            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        await db.ConcernAuditEvents.Where(e => e.OccurredAt <= now.AddDays(-180)).ExecuteDeleteAsync(cancellationToken);
    }

    private async Task<ConcernView?> ReadAuthorizedAsync(Guid accountId, Guid profileId,
        Guid concernId, DateTimeOffset now, bool auditDenial, CancellationToken cancellationToken)
    {
        var groupId = await db.Concerns.AsNoTracking().Where(row => row.Id == concernId)
            .Select(row => (Guid?)row.FamilyGroupId).SingleOrDefaultAsync(cancellationToken);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        if (groupId is null)
        {
            if (auditDenial) await AddDeniedAuditAsync(accountId, "SensitiveReadDenied", now, cancellationToken);
            if (auditDenial) await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return null;
        }
        if (!await LockFamilyGroupAsync(groupId.Value, cancellationToken)) return null;
        var c = await db.Concerns.SingleOrDefaultAsync(row => row.Id == concernId, cancellationToken);
        if (c is null)
        {
            if (auditDenial) await AddDeniedAuditAsync(accountId, "SensitiveReadDenied", now, cancellationToken);
            if (auditDenial) await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return null;
        }
        var creatorMembership = await CurrentGrantMembershipAsync(c.CreatorMembershipId,
            c.CreatorProfileId, c.FamilyGroupId, cancellationToken);
        NormalizeLifecycle(c, creatorMembership, now);
        await RevokeLostParticipantsAsync(c, now, cancellationToken);
        var caller = await CurrentActorMembershipAsync(accountId, c.FamilyGroupId, cancellationToken);
        var authorized = !c.IsExpired(now)
            && (IsCreator(c, accountId, caller) || await HasParticipantGrantAsync(c.Id, accountId, caller, cancellationToken));
        if (!authorized)
        {
            if (auditDenial) await AddDeniedAuditAsync(accountId, "SensitiveReadDenied", now, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return null;
        }
        await db.SaveChangesAsync(cancellationToken);
        var view = await BuildViewAsync(c, cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return view;
    }

    private async Task<Concern?> LoadForCommandAsync(Guid accountId, Guid concernId,
        string action, CancellationToken cancellationToken)
    {
        var groupId = await db.Concerns.AsNoTracking().Where(row => row.Id == concernId)
            .Select(row => (Guid?)row.FamilyGroupId).SingleOrDefaultAsync(cancellationToken);
        await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        if (groupId is null)
        {
            await AddDeniedAuditAsync(accountId, action, clock.GetUtcNow(), cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await db.Database.CurrentTransaction!.CommitAsync(cancellationToken);
            return null;
        }
        if (!await LockFamilyGroupAsync(groupId.Value, cancellationToken))
        {
            await db.Database.CurrentTransaction!.RollbackAsync(cancellationToken);
            return null;
        }
        var c = await db.Concerns.SingleOrDefaultAsync(row => row.Id == concernId, cancellationToken);
        if (c is null)
        {
            await AddDeniedAuditAsync(accountId, action, clock.GetUtcNow(), cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await db.Database.CurrentTransaction!.CommitAsync(cancellationToken);
            return null;
        }
        return c;
    }

    private void NormalizeLifecycle(Concern c, FamilyGroupMembership? creatorMembership,
        DateTimeOffset now)
    {
        DateTimeOffset? creatorLostAt = Matches(c.CreatorMembershipId, c.CreatorMembershipAccessVersion, creatorMembership)
            ? null : creatorMembership?.EndedAt ?? now;
        if (creatorLostAt is { } loss && c.RecordCreatorAccessRevoked(loss))
            db.ConcernAuditEvents.Add(Audit(c.Id, c.CreatorAccountId, "AccessRevoked", loss, false, c.CreatorProfileId));
        if (c.State != ConcernState.Open) return;
        if (now < c.InactivityFreezeAt && creatorLostAt is null) return;
        var freezeAt = c.EffectiveFreezeAt(creatorLostAt) ?? now;
        var reason = c.EffectiveFreezeReason(creatorLostAt);
        c.Freeze(freezeAt, reason);
        db.ConcernAuditEvents.Add(Audit(c.Id, null, "Frozen", freezeAt, true));
    }

    private async Task RevokeLostParticipantsAsync(Concern c, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var grants = await db.ConcernParticipantGrants
            .Where(g => g.ConcernId == c.Id && g.RemovedAt == null).ToListAsync(cancellationToken);
        foreach (var grant in grants)
        {
            var membership = await db.FamilyGroupMemberships.AsNoTracking()
                .SingleOrDefaultAsync(m => m.Id == grant.MembershipId, cancellationToken);
            if (Matches(grant.MembershipId, grant.MembershipAccessVersion, membership)) continue;
            var lostAt = membership?.EndedAt ?? now;
            grant.RevokeForEligibilityLoss(lostAt);
            db.ConcernAuditEvents.Add(Audit(c.Id, grant.AccountId, "AccessRevoked", lostAt, false, grant.ProfileId));
        }
    }

    private async Task<ConcernView> BuildViewAsync(Concern c, CancellationToken cancellationToken)
    {
        var groupName = await db.FamilyGroups.AsNoTracking().Where(g => g.Id == c.FamilyGroupId)
            .Select(g => g.Name).SingleAsync(cancellationToken);
        var names = await db.AdultProfiles.AsNoTracking()
            .Where(p => p.Id == c.CreatorProfileId || p.Id == c.SubjectProfileId
                || db.ConcernParticipantGrants.Any(g => g.ConcernId == c.Id && g.ProfileId == p.Id && g.RemovedAt == null)
                || db.ConcernEntries.Any(e => e.ConcernId == c.Id && e.ContributorProfileId == p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.DisplayName, cancellationToken);
        var participants = await db.ConcernParticipantGrants.AsNoTracking()
            .Where(g => g.ConcernId == c.Id && g.RemovedAt == null)
            .OrderBy(g => g.SelectedAt).Select(g => new ConcernPersonView(g.ProfileId,
                db.AdultProfiles.Where(p => p.Id == g.ProfileId).Select(p => p.DisplayName).FirstOrDefault() ?? "Member"))
            .ToListAsync(cancellationToken);
        var entries = await db.ConcernEntries.AsNoTracking().Where(e => e.ConcernId == c.Id)
            .OrderBy(e => e.RecordedAt).ThenBy(e => e.Id)
            .Select(e => new ConcernEntryView(e.Id, e.Kind, e.Category, e.Text,
                e.SourceDescription, e.SourceUnknown, e.ContributorProfileId,
                db.AdultProfiles.Where(p => p.Id == e.ContributorProfileId).Select(p => p.DisplayName).FirstOrDefault() ?? "Member",
                e.RecordedAt, e.EventAt, e.RelatedEntryId, e.Explanation, e.IsOutdated))
            .ToListAsync(cancellationToken);
        return new ConcernView(c.Id, c.FamilyGroupId, groupName,
            c.CreatorProfileId, names.GetValueOrDefault(c.CreatorProfileId, "Member"),
            c.SubjectProfileId, names.GetValueOrDefault(c.SubjectProfileId, "Member"),
            c.CreationReason, c.State, c.CreatedAt, c.LastAuthorizedMutationAt,
            c.FrozenAt, c.FreezeReason, c.ResolvedAt, c.ResolutionNote,
            c.ExpiresAt, participants, entries);
    }

    private async Task<FamilyGroupMembership?> CurrentActorMembershipAsync(Guid accountId,
        Guid familyGroupId, CancellationToken cancellationToken) => await db.FamilyGroupMemberships
        .SingleOrDefaultAsync(m => m.AccountId == accountId && m.FamilyGroupId == familyGroupId && m.IsActive, cancellationToken);

    private async Task<FamilyGroupMembership?> ActiveMembershipAsync(Guid accountId,
        Guid familyGroupId, CancellationToken cancellationToken) => await CurrentActorMembershipAsync(accountId, familyGroupId, cancellationToken);

    private Task<FamilyGroupMembership?> CurrentGrantMembershipAsync(Guid membershipId,
        Guid profileId, Guid groupId, CancellationToken cancellationToken) => db.FamilyGroupMemberships
        .SingleOrDefaultAsync(m => m.Id == membershipId && m.AdultProfileId == profileId
            && m.FamilyGroupId == groupId, cancellationToken);

    private async Task<bool> HasParticipantGrantAsync(Guid concernId, Guid accountId,
        FamilyGroupMembership? membership, CancellationToken cancellationToken) => membership is { IsActive: true }
        && await db.ConcernParticipantGrants.AnyAsync(g => g.ConcernId == concernId
            && g.AccountId == accountId && g.MembershipId == membership.Id
            && g.MembershipAccessVersion == membership.AccessVersion && g.RemovedAt == null, cancellationToken);

    private static bool IsCreator(Concern c, Guid accountId, FamilyGroupMembership? membership) =>
        c.CreatorAccountId == accountId && Matches(c.CreatorMembershipId,
            c.CreatorMembershipAccessVersion, membership);

    private static bool Matches(Guid id, int version, FamilyGroupMembership? membership) =>
        membership is { IsActive: true } && membership.Id == id && membership.AccessVersion == version;

    private async Task<List<FamilyGroupMembership>?> GetEligibleMembershipsAsync(Guid groupId,
        IReadOnlyList<Guid> profileIds, CancellationToken cancellationToken)
    {
        if (profileIds.Count > MaxParticipants) return null;
        var rows = await db.FamilyGroupMemberships.Where(m => m.FamilyGroupId == groupId
            && m.IsActive && profileIds.Contains(m.AdultProfileId)).ToListAsync(cancellationToken);
        return rows.Count == profileIds.Count ? rows : null;
    }

    private Task<Guid?> GetProfileIdAsync(Guid accountId, CancellationToken cancellationToken) => db.AdultProfiles.AsNoTracking()
        .Where(p => p.AccountId == accountId).Select(p => (Guid?)p.Id).SingleOrDefaultAsync(cancellationToken);

    private async Task<bool> LockFamilyGroupAsync(Guid groupId, CancellationToken cancellationToken) =>
        await db.FamilyGroups.Where(g => g.Id == groupId)
            .ExecuteUpdateAsync(update => update.SetProperty(g => g.CapacityLimit, g => g.CapacityLimit), cancellationToken) == 1;

    private async Task AddDeniedAuditAsync(Guid accountId, string action, DateTimeOffset at,
        CancellationToken cancellationToken)
    {
        var category = action == "SensitiveReadDenied" ? "ConcernReadDenied" : "ConcernWriteDenied";
        var utc = at.UtcDateTime;
        var bucket = new DateTimeOffset(utc.Year, utc.Month, utc.Day, utc.Hour, 0, 0, TimeSpan.Zero);
        var updated = await db.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE summary
SET [LastAttemptAt] = {at}, [DeniedAttemptCount] = [DeniedAttemptCount] + 1
FROM [ConcernAuditEvents] AS summary WITH (UPDLOCK, SERIALIZABLE, INDEX([IX_ConcernAuditEvents_ActorAccountId_Action_UtcHourBucket]))
WHERE [ActorAccountId] = {accountId} AND [Action] = {category}
  AND [UtcHourBucket] = {bucket} AND [DeniedAttemptCount] > 0;", cancellationToken);
        if (updated == 0)
            db.ConcernAuditEvents.Add(ConcernAuditEvent.DeniedSummary(accountId, category, bucket, at, at));
    }

    private static ConcernAuditEvent Audit(Guid? concernId, Guid? actorId,
        string action, DateTimeOffset at, bool authorized, Guid? participantId = null) =>
        new(Guid.NewGuid(), concernId, actorId, action, at, authorized, participantId);
}
