using System.Data;
using FamilySafety.Application.CheckIns;
using FamilySafety.Domain.CheckIns;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Infrastructure.Persistence;

public sealed class EfCheckInRepository(FamilySafetyDbContext db, TimeProvider clock) : ICheckInRepository
{
    public async Task<CheckInView?> CreateAsync(Guid accountId, CreateCheckInRequest request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        // Same family lock as existing member-sensitive writes; membership reads remain locked until commit.
        await db.FamilyGroups.Where(g => g.Id == request.FamilyGroupId)
            .ExecuteUpdateAsync(u => u.SetProperty(g => g.CapacityLimit, g => g.CapacityLimit), ct);
        var author = await db.FamilyGroupMemberships.AsNoTracking().SingleOrDefaultAsync(m =>
            m.AccountId == accountId && m.FamilyGroupId == request.FamilyGroupId && m.IsActive
            && db.AdultProfiles.Any(p => p.Id == m.AdultProfileId && p.AccountId == accountId), ct);
        if (author is null) return null;
        var errors = new Dictionary<string, string[]>();
        if (request.Status is null || !Enum.IsDefined(request.Status.Value)) errors["Status"] = ["Choose a Check-In status."];
        if (request.Note?.Trim().Length > CheckIn.NoteLimit) errors["Note"] = ["Use a note of up to 1,000 characters."];
        if (request.RecipientProfileIds is null) errors["RecipientProfileIds"] = ["Choose recipients explicitly, or choose none."];
        if (errors.Count > 0) throw new CheckInValidationException(errors);
        var ids = request.RecipientProfileIds!.Distinct().ToArray();
        if (ids.Contains(author.AdultProfileId)) return null; // Independent author access already exists.
        var recipients = await db.FamilyGroupMemberships.AsNoTracking().Where(m => m.IsActive
            && m.FamilyGroupId == request.FamilyGroupId && ids.Contains(m.AdultProfileId)
            && db.AdultProfiles.Any(p => p.Id == m.AdultProfileId && p.AccountId == m.AccountId)).ToListAsync(ct);
        if (recipients.Count != ids.Length) return null;
        var row = new CheckIn(Guid.NewGuid(), request.FamilyGroupId, author.AdultProfileId, author.Id,
            author.AccessVersion, request.Status!.Value, request.Note, clock.GetUtcNow());
        db.CheckIns.Add(row);
        db.CheckInRecipients.AddRange(recipients.Select(m => new CheckInRecipient(row.Id, m.AdultProfileId, m.Id, m.AccessVersion)));
        await db.SaveChangesAsync(ct);
        var view = await Projection(accountId, Own(accountId).Where(c => c.Id == row.Id)).SingleAsync(ct);
        await tx.CommitAsync(ct);
        return view;
    }

    public async Task<IReadOnlyList<CheckInView>> ListAsync(Guid accountId, bool received, CancellationToken ct) =>
        await Projection(accountId, received ? Received(accountId) : Own(accountId)).ToListAsync(ct);

    public Task<CheckInView?> GetAsync(Guid accountId, Guid checkInId, CancellationToken ct) =>
        Projection(accountId, db.CheckIns.Where(c => c.Id == checkInId &&
            (Own(accountId).Any(own => own.Id == c.Id) || Received(accountId).Any(shared => shared.Id == c.Id))))
            .SingleOrDefaultAsync(ct);

    private IQueryable<CheckIn> Own(Guid accountId) => db.CheckIns.Where(c =>
        db.AdultProfiles.Any(p => p.Id == c.AuthorProfileId && p.AccountId == accountId));

    private IQueryable<CheckIn> Received(Guid accountId) => db.CheckIns.Where(c =>
        db.FamilyGroupMemberships.Any(a => a.Id == c.AuthorMembershipId && a.FamilyGroupId == c.FamilyGroupId
            && a.AdultProfileId == c.AuthorProfileId && a.IsActive && a.AccessVersion == c.AuthorMembershipVersion
            && db.AdultProfiles.Any(p => p.Id == a.AdultProfileId && p.AccountId == a.AccountId))
        && db.CheckInRecipients.Any(r => r.CheckInId == c.Id && !r.IsInvalidated
            && db.FamilyGroupMemberships.Any(m => m.Id == r.MembershipId && m.AccountId == accountId
                && m.AdultProfileId == r.ProfileId && m.FamilyGroupId == c.FamilyGroupId && m.IsActive
                && m.AccessVersion == r.MembershipVersion
                && db.AdultProfiles.Any(p => p.Id == r.ProfileId && p.AccountId == accountId))));

    // Only already-authorized rows reach this projection. Other recipients never get audience identities.
    private IQueryable<CheckInView> Projection(Guid accountId, IQueryable<CheckIn> visible) =>
        visible.AsNoTracking().OrderByDescending(c => c.CreatedAtUtc).ThenBy(c => c.Id).Select(c => new CheckInView(
            c.Id, c.FamilyGroupId, c.AuthorProfileId,
            db.AdultProfiles.Where(p => p.Id == c.AuthorProfileId).Select(p => p.DisplayName).Single(),
            c.CreatedAtUtc, c.Status, c.Note,
            db.AdultProfiles.Any(p => p.Id == c.AuthorProfileId && p.AccountId == accountId)
                ? db.CheckInRecipients.Where(r => r.CheckInId == c.Id).OrderBy(r => r.ProfileId).Select(r => r.ProfileId).ToList()
                : null));
}
