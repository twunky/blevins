using FamilySafety.Application.Concerns;
using FamilySafety.Domain.Concerns;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Infrastructure.Persistence;

/// <summary>
/// Read-only Concern access check for Incident lineage. It mirrors the authorization in
/// <see cref="EfConcernRepository"/> (creator membership match or a current participant grant, plus
/// the effective freeze and expiry) without normalizing, persisting, or auditing anything.
/// </summary>
public sealed class EfConcernAccessQuery(FamilySafetyDbContext db, TimeProvider clock) : IConcernAccessQuery
{
    // REQ-PRIV-025: Frozen and Resolved Concern history is retained 180 days from the transition.
    private const int ConcernRetentionDays = 180;

    public async Task<bool> CanReadAsync(Guid accountId, Guid concernId, CancellationToken cancellationToken)
    {
        var current = await LoadUnexpiredAsync(concernId, cancellationToken);
        if (current is not { } found) return false;
        if (found.Concern.CreatorAccountId == accountId && found.CreatorAccessCurrent) return true;
        return await (from grant in db.ConcernParticipantGrants.AsNoTracking()
                      join membership in db.FamilyGroupMemberships.AsNoTracking()
                          on grant.MembershipId equals membership.Id
                      where grant.ConcernId == concernId && grant.AccountId == accountId && grant.RemovedAt == null
                          && membership.IsActive && membership.AccountId == accountId
                          && membership.FamilyGroupId == found.Concern.FamilyGroupId
                          && membership.AccessVersion == grant.MembershipAccessVersion
                      select grant.Id).AnyAsync(cancellationToken);
    }

    public async Task<bool> IsCurrentCreatorAsync(Guid accountId, Guid concernId, Guid familyGroupId,
        CancellationToken cancellationToken)
    {
        var current = await LoadUnexpiredAsync(concernId, cancellationToken);
        return current is { } found && found.Concern.FamilyGroupId == familyGroupId
            && found.Concern.CreatorAccountId == accountId && found.CreatorAccessCurrent;
    }

    private async Task<(Concern Concern, bool CreatorAccessCurrent)?> LoadUnexpiredAsync(Guid concernId,
        CancellationToken cancellationToken)
    {
        var concern = await db.Concerns.AsNoTracking().SingleOrDefaultAsync(c => c.Id == concernId, cancellationToken);
        if (concern is null) return null;
        var creatorMembership = await db.FamilyGroupMemberships.AsNoTracking().SingleOrDefaultAsync(m =>
            m.Id == concern.CreatorMembershipId && m.AdultProfileId == concern.CreatorProfileId
            && m.FamilyGroupId == concern.FamilyGroupId, cancellationToken);
        var now = clock.GetUtcNow();
        var creatorAccessCurrent = creatorMembership is { IsActive: true }
            && creatorMembership.AccessVersion == concern.CreatorMembershipAccessVersion;
        DateTimeOffset? creatorLostAt = creatorAccessCurrent ? null : creatorMembership?.EndedAt ?? now;

        // Effective expiry, before any Concern normalization has been persisted.
        var expiresAt = concern.State == ConcernState.Open && concern.IsEffectivelyFrozen(now, creatorLostAt)
            ? concern.EffectiveFreezeAt(creatorLostAt)?.AddDays(ConcernRetentionDays)
            : concern.ExpiresAt;
        return expiresAt is { } expiry && now >= expiry ? null : (concern, creatorAccessCurrent);
    }
}
