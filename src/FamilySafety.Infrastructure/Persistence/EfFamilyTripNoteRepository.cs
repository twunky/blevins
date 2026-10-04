using System.Data;
using FamilySafety.Application.Trips;
using FamilySafety.Domain.Trips;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Infrastructure.Persistence;

public sealed class EfFamilyTripNoteRepository(FamilySafetyDbContext db, TimeProvider clock)
    : IFamilyTripNoteRepository
{
    public async Task<IReadOnlyList<TripNoteView>?> ListAsync(Guid accountId, Guid tripId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var profileId = await ProfileIdAsync(accountId, cancellationToken);
        if (profileId is null || !await AuthorizedTrip(profileId.Value, tripId)
            .AnyAsync(cancellationToken)) return null;
        var notes = await VisibleNotes(profileId.Value, tripId).ToListAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return notes;
    }

    public async Task<TripNoteView?> AddAsync(Guid accountId, Guid tripId, string content,
        CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var profileId = await ProfileIdAsync(accountId, cancellationToken);
        if (profileId is null || !await AuthorizedTrip(profileId.Value, tripId)
            .AnyAsync(cancellationToken)) return null;

        var note = new FamilyTripNote(Guid.NewGuid(), tripId, profileId.Value, content, clock.GetUtcNow());
        db.FamilyTripNotes.Add(note);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return (await ListAsync(accountId, tripId, cancellationToken))?
            .SingleOrDefault(view => view.NoteId == note.Id);
    }

    public Task<TripNoteView?> EditAsync(Guid accountId, Guid tripId, Guid noteId, string content,
        CancellationToken cancellationToken) => MutateAsync(accountId, tripId, noteId,
            (note, profileId, _) =>
            {
                if (note.AuthorProfileId != profileId) return false;
                note.Edit(content, clock.GetUtcNow());
                return true;
            }, cancellationToken);

    public async Task<bool> DeleteAsync(Guid accountId, Guid tripId, Guid noteId,
        CancellationToken cancellationToken) =>
        await MutateAsync(accountId, tripId, noteId,
            (note, profileId, _) =>
            {
                if (note.AuthorProfileId != profileId) return false;
                note.DeleteByAuthor(clock.GetUtcNow());
                return true;
            }, cancellationToken) is not null;

    public async Task<bool> HideAsync(Guid accountId, Guid tripId, Guid noteId,
        CancellationToken cancellationToken) =>
        await MutateAsync(accountId, tripId, noteId,
            (note, profileId, creatorId) =>
            {
                if (creatorId != profileId || note.AuthorProfileId == profileId) return false;
                note.HideByCreator(clock.GetUtcNow());
                return true;
            }, cancellationToken) is not null;

    private async Task<TripNoteView?> MutateAsync(Guid accountId, Guid tripId, Guid noteId,
        Func<FamilyTripNote, Guid, Guid, bool> mutate, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var profileId = await ProfileIdAsync(accountId, cancellationToken);
        if (profileId is null) return null;
        var creatorId = await AuthorizedTrip(profileId.Value, tripId)
            .Select(trip => (Guid?)trip.CreatorProfileId)
            .SingleOrDefaultAsync(cancellationToken);
        if (creatorId is null) return null;
        var note = await db.FamilyTripNotes.SingleOrDefaultAsync(
            item => item.Id == noteId && item.TripId == tripId && item.State == TripNoteState.Active,
            cancellationToken);
        if (note is null || !mutate(note, profileId.Value, creatorId.Value)) return null;
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return null; }
        await transaction.CommitAsync(cancellationToken);
        // Removed records have no normal projection. This return value is used only as a success marker.
        return note.State == TripNoteState.Active
            ? (await ListAsync(accountId, tripId, cancellationToken))?
                .SingleOrDefault(item => item.NoteId == noteId)
            : new TripNoteView(note.Id, note.AuthorProfileId, string.Empty, string.Empty,
                note.CreatedAt, note.UpdatedAt, false, false, false);
    }

    private IQueryable<FamilyTrip> AuthorizedTrip(Guid profileId, Guid tripId) =>
        db.FamilyTrips.AsNoTracking().Where(trip => trip.Id == tripId
            && (trip.CreatorProfileId == profileId || db.FamilyTripAudienceMembers.Any(recipient =>
                recipient.TripId == trip.Id && recipient.ProfileId == profileId
                && db.FamilyGroupMemberships.Any(membership =>
                    membership.AdultProfileId == recipient.ProfileId
                    && membership.FamilyGroupId == trip.FamilyGroupId
                    && membership.IsActive
                    && membership.AccessVersion == recipient.MembershipAccessVersion))));

    private IQueryable<TripNoteView> VisibleNotes(Guid profileId, Guid tripId) =>
        from note in db.FamilyTripNotes.AsNoTracking()
        join author in db.AdultProfiles.AsNoTracking() on note.AuthorProfileId equals author.Id
        join trip in AuthorizedTrip(profileId, tripId) on note.TripId equals trip.Id
        where note.TripId == tripId && note.State == TripNoteState.Active
        orderby note.CreatedAt, note.Id
        select new TripNoteView(note.Id, note.AuthorProfileId, author.DisplayName,
            note.Content, note.CreatedAt, note.UpdatedAt,
            note.AuthorProfileId == profileId, note.AuthorProfileId == profileId,
            trip.CreatorProfileId == profileId && note.AuthorProfileId != profileId);

    private Task<Guid?> ProfileIdAsync(Guid accountId, CancellationToken cancellationToken) =>
        db.AdultProfiles.AsNoTracking().Where(profile => profile.AccountId == accountId)
            .Select(profile => (Guid?)profile.Id).SingleOrDefaultAsync(cancellationToken);
}
