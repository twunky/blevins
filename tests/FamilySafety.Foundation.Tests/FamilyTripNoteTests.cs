using FamilySafety.Application.Profiles;
using FamilySafety.Application.Trips;
using FamilySafety.Domain.Profiles;
using FamilySafety.Domain.Relationships;
using FamilySafety.Domain.Trips;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Foundation.Tests;

public sealed class FamilyTripNoteTests
{
    [Fact]
    public void DomainTransitionsRetainContentAndProvenance()
    {
        var created = DateTimeOffset.UtcNow;
        var note = new FamilyTripNote(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "  First\nline  ", created);
        Assert.Equal("First\nline", note.Content);
        Assert.Equal(TripNoteState.Active, note.State);
        note.Edit("Second", created.AddMinutes(1));
        Assert.Equal(created, note.CreatedAt);
        Assert.Equal(created.AddMinutes(1), note.UpdatedAt);
        note.HideByCreator(created.AddMinutes(2));
        Assert.Equal(TripNoteState.CreatorHidden, note.State);
        Assert.Equal("Second", note.Content);
        Assert.Equal(created.AddMinutes(2), note.RemovedAt);
        Assert.Throws<InvalidOperationException>(() => note.Edit("Changed", created.AddMinutes(3)));
        Assert.Throws<InvalidOperationException>(() => note.DeleteByAuthor(created.AddMinutes(3)));

        var deleted = new FamilyTripNote(Guid.NewGuid(), note.TripId, note.AuthorProfileId, "Original", created);
        deleted.DeleteByAuthor(created.AddMinutes(1));
        Assert.Equal(TripNoteState.AuthorDeleted, deleted.State);
        Assert.Equal("Original", deleted.Content);
        Assert.Throws<InvalidOperationException>(() => deleted.HideByCreator(created.AddMinutes(2)));
    }

    [Fact]
    public async Task ServiceValidatesContentAndDoesNotTrustClientMetadata()
    {
        var repository = new StubNoteRepository();
        var service = new FamilyTripNoteService(new StubAccountAccessor(), repository);
        var tripId = Guid.NewGuid();
        await Assert.ThrowsAsync<FamilyTripNoteValidationException>(() =>
            service.AddAsync(tripId, new TripNoteContentRequest("   "), CancellationToken.None));
        await Assert.ThrowsAsync<FamilyTripNoteValidationException>(() =>
            service.AddAsync(tripId, new TripNoteContentRequest(new string('x', 1001)), CancellationToken.None));
        await service.AddAsync(tripId, new TripNoteContentRequest("  Useful\nDetail  "), CancellationToken.None);
        Assert.Equal("Useful\nDetail", repository.LastContent);
    }

    [Fact]
    public async Task SqlSelectedAudienceActionsRetentionAndNegativePermissions()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        await WithDatabaseAsync(async db =>
        {
            var creator = AddAdult(db, "Creator");
            var viewer = AddAdult(db, "Viewer");
            var traveler = AddAdult(db, "Traveler");
            var admin = AddAdult(db, "Admin");
            var relative = AddAdult(db, "Relative");
            var outsider = AddAdult(db, "Outsider");
            var group = new FamilyGroup(Guid.NewGuid(), "Family", 100);
            var otherGroup = new FamilyGroup(Guid.NewGuid(), "Other", 100);
            var creatorMembership = new FamilyGroupMembership(Guid.NewGuid(), creator.AccountId, creator.ProfileId, group.Id, true);
            var viewerMembership = new FamilyGroupMembership(Guid.NewGuid(), viewer.AccountId, viewer.ProfileId, group.Id, true);
            var adminMembership = new FamilyGroupMembership(Guid.NewGuid(), admin.AccountId, admin.ProfileId, group.Id, true);
            db.AddRange(group, otherGroup, creatorMembership, viewerMembership,
                new FamilyGroupMembership(Guid.NewGuid(), traveler.AccountId, traveler.ProfileId, group.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), relative.AccountId, relative.ProfileId, group.Id, true),
                adminMembership,
                new FamilyAdministratorAssignment(Guid.NewGuid(), adminMembership.Id),
                new FamilyRelationship(Guid.NewGuid(), group.Id, creator.ProfileId,
                    relative.ProfileId, FamilyRelationshipLabel.Sibling),
                new FamilyGroupMembership(Guid.NewGuid(), outsider.AccountId, outsider.ProfileId, otherGroup.Id, true));
            await db.SaveChangesAsync();

            var trips = new EfFamilyTripRepository(db);
            var trip = await trips.CreateAsync(creator.AccountId, group.Id,
                Create([traveler.ProfileId], TripAudienceType.SelectedPeople, [viewer.ProfileId]), CancellationToken.None);
            Assert.NotNull(trip);
            var otherTrip = await trips.CreateAsync(creator.AccountId, group.Id,
                Create([traveler.ProfileId], TripAudienceType.SelectedPeople, [viewer.ProfileId]), CancellationToken.None);
            Assert.NotNull(otherTrip);
            var notes = new EfFamilyTripNoteRepository(db, TimeProvider.System);
            Assert.Empty((await notes.ListAsync(viewer.AccountId, trip.TripId, CancellationToken.None))!);
            var first = await notes.AddAsync(viewer.AccountId, trip.TripId, "Flight booked", CancellationToken.None);
            Assert.NotNull(first);
            Assert.Equal(viewer.ProfileId, first.AuthorProfileId);
            Assert.True(first.CanEdit);
            Assert.False(first.CanHide);
            Assert.Equal("Flight booked", Assert.Single((await notes.ListAsync(creator.AccountId, trip.TripId, CancellationToken.None))!).Content);
            Assert.Null(await notes.ListAsync(traveler.AccountId, trip.TripId, CancellationToken.None));
            Assert.Null(await notes.ListAsync(admin.AccountId, trip.TripId, CancellationToken.None));
            Assert.Null(await notes.ListAsync(relative.AccountId, trip.TripId, CancellationToken.None));
            Assert.Null(await notes.ListAsync(outsider.AccountId, trip.TripId, CancellationToken.None));
            Assert.Null(await notes.AddAsync(traveler.AccountId, trip.TripId, "No", CancellationToken.None));
            Assert.Null(await notes.EditAsync(creator.AccountId, trip.TripId, first.NoteId, "Rewrite", CancellationToken.None));
            Assert.False(await notes.DeleteAsync(creator.AccountId, trip.TripId, first.NoteId, CancellationToken.None));
            Assert.False(await notes.HideAsync(viewer.AccountId, trip.TripId, first.NoteId, CancellationToken.None));
            Assert.Null(await notes.EditAsync(viewer.AccountId, otherTrip.TripId, first.NoteId, "Cross-trip", CancellationToken.None));

            var edited = await notes.EditAsync(viewer.AccountId, trip.TripId, first.NoteId, "Flight changed", CancellationToken.None);
            Assert.NotNull(edited?.UpdatedAt);
            Assert.Equal(first.CreatedAt, edited.CreatedAt);
            Assert.True(await notes.HideAsync(creator.AccountId, trip.TripId, first.NoteId, CancellationToken.None));
            Assert.Empty((await notes.ListAsync(viewer.AccountId, trip.TripId, CancellationToken.None))!);
            Assert.False(await notes.DeleteAsync(viewer.AccountId, trip.TripId, first.NoteId, CancellationToken.None));
            Assert.Null(await notes.EditAsync(viewer.AccountId, trip.TripId, first.NoteId, "Stale", CancellationToken.None));
            Assert.False(await notes.HideAsync(creator.AccountId, trip.TripId, first.NoteId, CancellationToken.None));
            var hidden = await db.FamilyTripNotes.AsNoTracking().SingleAsync(note => note.Id == first.NoteId);
            Assert.Equal(TripNoteState.CreatorHidden, hidden.State);
            Assert.Equal("Flight changed", hidden.Content);
            Assert.Equal(viewer.ProfileId, hidden.AuthorProfileId);
            Assert.Equal(edited.UpdatedAt, hidden.UpdatedAt);
            Assert.NotNull(hidden.RemovedAt);

            var own = await notes.AddAsync(creator.AccountId, trip.TripId, "Creator note", CancellationToken.None);
            Assert.NotNull(own);
            var ownEdit = await notes.EditAsync(creator.AccountId, trip.TripId, own.NoteId,
                "Creator note updated", CancellationToken.None);
            Assert.NotNull(ownEdit?.UpdatedAt);
            Assert.True(await notes.DeleteAsync(creator.AccountId, trip.TripId, own.NoteId, CancellationToken.None));
            Assert.Empty((await notes.ListAsync(creator.AccountId, trip.TripId, CancellationToken.None))!);
            var removed = await db.FamilyTripNotes.AsNoTracking().SingleAsync(note => note.Id == own.NoteId);
            Assert.Equal(TripNoteState.AuthorDeleted, removed.State);
            Assert.Equal("Creator note updated", removed.Content);
            Assert.Equal(own.CreatedAt, removed.CreatedAt);
            Assert.Equal(ownEdit.UpdatedAt, removed.UpdatedAt);
            Assert.NotNull(removed.RemovedAt);

            viewerMembership.End();
            await db.SaveChangesAsync();
            Assert.Null(await notes.ListAsync(viewer.AccountId, trip.TripId, CancellationToken.None));
            await db.FamilyGroupMemberships.Where(m => m.Id == viewerMembership.Id)
                .ExecuteUpdateAsync(update => update.SetProperty(m => m.IsActive, true));
            Assert.Null(await notes.ListAsync(viewer.AccountId, trip.TripId, CancellationToken.None));
            creatorMembership.End();
            await db.SaveChangesAsync();
            Assert.NotNull(await notes.AddAsync(creator.AccountId, trip.TripId, "Still here", CancellationToken.None));
            Assert.Single((await notes.ListAsync(creator.AccountId, trip.TripId, CancellationToken.None))!);
        });
    }

    [Fact]
    public async Task SqlGroupAudienceDoesNotIncludeLaterJoiners()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        await WithDatabaseAsync(async db =>
        {
            var creator = AddAdult(db, "Creator");
            var viewer = AddAdult(db, "Viewer");
            var group = new FamilyGroup(Guid.NewGuid(), "Family", 100);
            db.AddRange(group,
                new FamilyGroupMembership(Guid.NewGuid(), creator.AccountId, creator.ProfileId, group.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), viewer.AccountId, viewer.ProfileId, group.Id, true));
            await db.SaveChangesAsync();
            var trip = await new EfFamilyTripRepository(db).CreateAsync(creator.AccountId, group.Id,
                Create([viewer.ProfileId], TripAudienceType.FamilyGroup, []), CancellationToken.None);
            Assert.NotNull(trip);
            var notes = new EfFamilyTripNoteRepository(db, TimeProvider.System);
            Assert.NotNull(await notes.AddAsync(viewer.AccountId, trip.TripId, "Welcome", CancellationToken.None));
            var joiner = AddAdult(db, "Joiner");
            db.FamilyGroupMemberships.Add(new FamilyGroupMembership(Guid.NewGuid(), joiner.AccountId, joiner.ProfileId, group.Id, true));
            await db.SaveChangesAsync();
            Assert.Null(await notes.ListAsync(joiner.AccountId, trip.TripId, CancellationToken.None));
        });
    }

    private static CreateFamilyTripRequest Create(IReadOnlyList<Guid> travelers, TripAudienceType mode,
        IReadOnlyList<Guid> recipients) => new("Visit", "Atlanta", new DateOnly(2026, 10, 1),
        new DateOnly(2026, 10, 5), null, null, null, null, null, null,
        travelers, mode, recipients, [new TripContactInput("Guide", null, null, null)]);

    private static (Guid AccountId, Guid ProfileId) AddAdult(FamilySafetyDbContext db, string name)
    {
        var accountId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        db.AddRange(new Account(accountId, $"{accountId:N}@example.invalid"),
            new AdultProfile(profileId, accountId, name));
        return (accountId, profileId);
    }

    private static async Task WithDatabaseAsync(Func<FamilySafetyDbContext, Task> test)
    {
        var connection = $"Server=localhost;Database=FamilySafety_WI007004_Notes_{Guid.NewGuid():N};Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
        var options = new DbContextOptionsBuilder<FamilySafetyDbContext>().UseSqlServer(connection).Options;
        await using var db = new FamilySafetyDbContext(options);
        var cleanup = false;
        try
        {
            await db.Database.EnsureDeletedAsync();
            cleanup = true;
            await db.Database.MigrateAsync();
            await test(db);
        }
        finally
        {
            if (cleanup)
            {
                await using var cleanupDb = new FamilySafetyDbContext(options);
                await cleanupDb.Database.EnsureDeletedAsync();
            }
        }
    }

    private sealed class StubAccountAccessor : ICurrentAccountAccessor
    {
        public CurrentAccount? Current => new(Guid.NewGuid());
    }

    private sealed class StubNoteRepository : IFamilyTripNoteRepository
    {
        public string? LastContent { get; private set; }
        public Task<IReadOnlyList<TripNoteView>?> ListAsync(Guid accountId, Guid tripId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TripNoteView>?>([]);
        public Task<TripNoteView?> AddAsync(Guid accountId, Guid tripId, string content, CancellationToken cancellationToken)
        {
            LastContent = content;
            return Task.FromResult<TripNoteView?>(new TripNoteView(Guid.NewGuid(), Guid.NewGuid(), "Test", content,
                DateTimeOffset.UtcNow, null, true, true, false));
        }
        public Task<TripNoteView?> EditAsync(Guid accountId, Guid tripId, Guid noteId, string content, CancellationToken cancellationToken) =>
            Task.FromResult<TripNoteView?>(null);
        public Task<bool> DeleteAsync(Guid accountId, Guid tripId, Guid noteId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<bool> HideAsync(Guid accountId, Guid tripId, Guid noteId, CancellationToken cancellationToken) => Task.FromResult(false);
    }
}
