using System.Data;
using FamilySafety.Application.Sharing;
using FamilySafety.Domain.Sharing;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Infrastructure.Persistence;

public sealed class EfSharingSessionRepository(FamilySafetyDbContext db, TimeProvider clock) : ISharingSessionRepository
{
    public async Task<SharingSessionView?> CreateAsync(Guid accountId, CreateSharingSessionRequest request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await LockFamily(request.FamilyGroupId, ct);
        var author = await db.FamilyGroupMemberships.AsNoTracking().SingleOrDefaultAsync(m => m.AccountId == accountId
            && m.FamilyGroupId == request.FamilyGroupId && m.IsActive
            && db.AdultProfiles.Any(p => p.Id == m.AdultProfileId && p.AccountId == accountId), ct);
        if (author is null) return null;
        var now = clock.GetUtcNow();
        var end = request.EndsAtUtc ?? now.AddHours(1);
        if (end <= now || end > now.AddHours(24)) Invalid("EndsAtUtc", "Choose an end time in the next 24 hours.");
        if (request.RecipientProfileIds is null || request.RecipientProfileIds.Length == 0)
            Invalid("RecipientProfileIds", "Explicitly choose at least one recipient.");
        var ids = request.RecipientProfileIds!.Distinct().ToArray();
        if (ids.Contains(author.AdultProfileId)) return null;
        var recipients = await db.FamilyGroupMemberships.AsNoTracking().Where(m => m.IsActive
            && m.FamilyGroupId == request.FamilyGroupId && ids.Contains(m.AdultProfileId)
            && db.AdultProfiles.Any(p => p.Id == m.AdultProfileId && p.AccountId == m.AccountId)).ToListAsync(ct);
        if (recipients.Count != ids.Length) return null;
        var row = new SharingSession(Guid.NewGuid(), request.FamilyGroupId, author.AdultProfileId,
            author.Id, author.AccessVersion, now, end);
        db.SharingSessions.Add(row);
        db.SharingSessionRecipients.AddRange(recipients.Select(m => new SharingSessionRecipient(row.Id, m.AdultProfileId, m.Id, m.AccessVersion)));
        await Save(ct);
        var view = await GetAsync(accountId, row.Id, ct);
        await tx.CommitAsync(ct);
        return view;
    }

    public async Task<IReadOnlyList<SharingSessionView>> ListAsync(Guid accountId, bool received, CancellationToken ct) =>
        await Projection(accountId, received ? Received(accountId) : Own(accountId)).ToListAsync(ct);

    public Task<SharingSessionView?> GetAsync(Guid accountId, Guid sessionId, CancellationToken ct) =>
        Projection(accountId, db.SharingSessions.Where(s => s.Id == sessionId
            && (Own(accountId).Any(o => o.Id == s.Id) || Received(accountId).Any(r => r.Id == s.Id)))).SingleOrDefaultAsync(ct);

    public async Task<bool> StopAsync(Guid accountId, Guid sessionId, int version, CancellationToken ct)
    {
        var familyId = await FamilyId(sessionId, ct);
        if (familyId is null) return false;
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var row = await AuthorWrite(accountId, sessionId, familyId.Value, ct);
        if (row is null) return false;
        if (row.Version != version) throw new SharingSessionConflictException();
        row.Stop(clock.GetUtcNow());
        await db.SharingCurrentLocations.Where(l => l.SharingSessionId == sessionId).ExecuteDeleteAsync(ct);
        await Save(ct); await tx.CommitAsync(ct); return true;
    }

    public async Task<SharingSessionView?> UpdateLocationAsync(Guid accountId, Guid sessionId, UpdateSharingLocationRequest request, CancellationToken ct)
    {
        var familyId = await FamilyId(sessionId, ct);
        if (familyId is null) return null;
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var row = await AuthorWrite(accountId, sessionId, familyId.Value, ct);
        if (row is null) return null;
        if (row.Version != request.Version) throw new SharingSessionConflictException();
        var now = clock.GetUtcNow();
        if (request.Latitude is not { } lat || !double.IsFinite(lat) || lat < -90 || lat > 90)
            Invalid("Latitude", "Latitude must be between -90 and 90.");
        if (request.Longitude is not { } lon || !double.IsFinite(lon) || lon < -180 || lon > 180)
            Invalid("Longitude", "Longitude must be between -180 and 180.");
        if (request.Accuracy is not { } accuracy || !double.IsFinite(accuracy) || accuracy < 0)
            Invalid("Accuracy", "Accuracy must be a non-negative number of meters.");
        // Browser samples may be delayed a little, but cannot predate the session, be over five minutes old,
        // or be over one minute in the future (bounded device clock skew). Arrival time never rewrites freshness.
        if (request.CapturedAtUtc is not { } captured || captured < row.StartedAtUtc || captured < now.AddMinutes(-5)
            || captured > now.AddMinutes(1) || captured >= row.EndsAtUtc)
            Invalid("CapturedAtUtc", "Use a recent location captured during this session.");
        var point = await db.SharingCurrentLocations.SingleOrDefaultAsync(l => l.SharingSessionId == sessionId, ct);
        if (point is not null && (request.CapturedAtUtc <= point.CapturedAtUtc || now < point.ReceivedAtUtc.AddMinutes(1)))
            throw new SharingSessionConflictException();
        if (point is null) { point = new SharingCurrentLocation(sessionId); db.SharingCurrentLocations.Add(point); }
        point.Replace(request.Latitude!.Value, request.Longitude!.Value, request.Accuracy!.Value, request.CapturedAtUtc!.Value, now);
        row.LocationReplaced();
        await Save(ct);
        var view = await GetAsync(accountId, sessionId, ct);
        await tx.CommitAsync(ct); return view;
    }

    // Family comes first, even when the caller knows a session ID. No protected projection occurs here.
    private Task<Guid?> FamilyId(Guid sessionId, CancellationToken ct) => db.SharingSessions.Where(s => s.Id == sessionId)
        .Select(s => (Guid?)s.FamilyGroupId).SingleOrDefaultAsync(ct);
    private async Task<SharingSession?> AuthorWrite(Guid accountId, Guid sessionId, Guid familyId, CancellationToken ct)
    {
        await LockFamily(familyId, ct);
        // Refresh after external membership-trigger mutations when a context is reused.
        var tracked = db.ChangeTracker.Entries<SharingSession>().SingleOrDefault(e => e.Entity.Id == sessionId);
        if (tracked is not null) await tracked.ReloadAsync(ct);
        return await Own(accountId).SingleOrDefaultAsync(s => s.Id == sessionId && s.FamilyGroupId == familyId, ct);
    }
    private Task<int> LockFamily(Guid familyId, CancellationToken ct) => db.FamilyGroups.Where(g => g.Id == familyId)
        .ExecuteUpdateAsync(u => u.SetProperty(g => g.CapacityLimit, g => g.CapacityLimit), ct);

    private IQueryable<SharingSession> Active() => db.SharingSessions.Where(s => s.StoppedAtUtc == null && s.EndsAtUtc > clock.GetUtcNow()
        && db.FamilyGroupMemberships.Any(a => a.Id == s.AuthorMembershipId && a.FamilyGroupId == s.FamilyGroupId
            && a.AdultProfileId == s.AuthorProfileId && a.IsActive && a.AccessVersion == s.AuthorMembershipVersion
            && db.AdultProfiles.Any(p => p.Id == a.AdultProfileId && p.AccountId == a.AccountId)));
    private IQueryable<SharingSession> Own(Guid accountId) => Active().Where(s =>
        db.AdultProfiles.Any(p => p.Id == s.AuthorProfileId && p.AccountId == accountId));
    private IQueryable<SharingSession> Received(Guid accountId) => Active().Where(s => db.SharingSessionRecipients.Any(r =>
        r.SharingSessionId == s.Id && !r.IsInvalidated && db.FamilyGroupMemberships.Any(m => m.Id == r.MembershipId
            && m.AccountId == accountId && m.AdultProfileId == r.ProfileId && m.FamilyGroupId == s.FamilyGroupId && m.IsActive
            && m.AccessVersion == r.MembershipVersion && db.AdultProfiles.Any(p => p.Id == r.ProfileId && p.AccountId == accountId))));

    // All predicates run on the server before coordinates/audience are projected. Recipients never get audience identities.
    private IQueryable<SharingSessionView> Projection(Guid accountId, IQueryable<SharingSession> visible) =>
        visible.AsNoTracking().OrderByDescending(s => s.StartedAtUtc).ThenBy(s => s.Id).Select(s => new SharingSessionView(
            s.Id, s.FamilyGroupId, s.AuthorProfileId,
            db.AdultProfiles.Where(p => p.Id == s.AuthorProfileId).Select(p => p.DisplayName).Single(),
            s.StartedAtUtc, s.EndsAtUtc, "Active", s.Version,
            db.AdultProfiles.Any(p => p.Id == s.AuthorProfileId && p.AccountId == accountId)
                ? db.SharingSessionRecipients.Where(r => r.SharingSessionId == s.Id).OrderBy(r => r.ProfileId).Select(r => new SharingRecipientView(
                    r.ProfileId, db.AdultProfiles.Where(p => p.Id == r.ProfileId).Select(p => p.DisplayName).Single(),
                    !r.IsInvalidated && db.FamilyGroupMemberships.Any(m => m.Id == r.MembershipId && m.IsActive
                        && m.FamilyGroupId == s.FamilyGroupId && m.AdultProfileId == r.ProfileId && m.AccessVersion == r.MembershipVersion
                        && db.AdultProfiles.Any(p => p.Id == m.AdultProfileId && p.AccountId == m.AccountId)))).ToList() : null,
            db.SharingCurrentLocations.Where(l => l.SharingSessionId == s.Id).Select(l => new SharingLocationView(
                l.Latitude, l.Longitude, l.Accuracy, l.CapturedAtUtc, l.ReceivedAtUtc)).SingleOrDefault()));

    public async Task CleanupAsync(CancellationToken ct)
    {
        var families = await db.SharingSessions.Where(s => db.SharingCurrentLocations.Any(l => l.SharingSessionId == s.Id)
            && !Active().Any(a => a.Id == s.Id)).Select(s => s.FamilyGroupId).Distinct().ToListAsync(ct);
        foreach (var familyId in families)
        {
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            await LockFamily(familyId, ct);
            await db.SharingCurrentLocations.Where(l => db.SharingSessions.Any(s => s.Id == l.SharingSessionId && s.FamilyGroupId == familyId)
                && !Active().Any(s => s.Id == l.SharingSessionId)).ExecuteDeleteAsync(ct);
            await tx.CommitAsync(ct);
        }
    }
    private static void Invalid(string field, string message) => throw new SharingSessionValidationException(new Dictionary<string, string[]> { [field] = [message] });
    private async Task Save(CancellationToken ct)
    { try { await db.SaveChangesAsync(ct); } catch (DbUpdateConcurrencyException) { throw new SharingSessionConflictException(); } }
}
