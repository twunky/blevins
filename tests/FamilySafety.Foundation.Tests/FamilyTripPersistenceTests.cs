using FamilySafety.Application.Trips;
using FamilySafety.Domain.Profiles;
using FamilySafety.Domain.Trips;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Foundation.Tests;

public sealed class FamilyTripPersistenceTests
{
    [Fact]
    public async Task GroupSnapshotExcludesJoinersAndNeverRestoresEndedMemberAccess()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        await WithDatabaseAsync(async db =>
        {
            var creator = AddAdult(db, "Creator");
            var viewer = AddAdult(db, "Viewer");
            var traveler = AddAdult(db, "Traveler");
            var group = new FamilyGroup(Guid.NewGuid(), "Trip family", 100);
            var creatorMembership = new FamilyGroupMembership(Guid.NewGuid(), creator.AccountId, creator.ProfileId, group.Id, true);
            var viewerMembership = new FamilyGroupMembership(Guid.NewGuid(), viewer.AccountId, viewer.ProfileId, group.Id, true);
            db.AddRange(group, creatorMembership, viewerMembership,
                new FamilyGroupMembership(Guid.NewGuid(), traveler.AccountId, traveler.ProfileId, group.Id, true));
            await db.SaveChangesAsync();

            var repository = new EfFamilyTripRepository(db);
            var created = await repository.CreateAsync(creator.AccountId, group.Id,
                Create([traveler.ProfileId], TripAudienceType.FamilyGroup, []), CancellationToken.None);
            Assert.NotNull(created);
            Assert.True(created.CanEdit);
            Assert.Equal("UTC", created.ExpectedArrivalTimeZone);
            Assert.Single(created.Contacts);
            Assert.NotNull(await repository.GetAsync(viewer.AccountId, created.TripId, CancellationToken.None));
            var creatorAsTraveler = await repository.CreateAsync(creator.AccountId, group.Id,
                Create([creator.ProfileId], TripAudienceType.FamilyGroup, []) with
                {
                    Contacts = [], ExpectedArrivalDate = null,
                    ExpectedArrivalTime = null, ExpectedArrivalTimeZone = null,
                }, CancellationToken.None);
            Assert.Equal(creator.ProfileId, Assert.Single(creatorAsTraveler!.Travelers).ProfileId);
            Assert.Empty(creatorAsTraveler.Contacts);

            var newcomer = AddAdult(db, "Newcomer");
            db.FamilyGroupMemberships.Add(new FamilyGroupMembership(
                Guid.NewGuid(), newcomer.AccountId, newcomer.ProfileId, group.Id, true));
            await db.SaveChangesAsync();
            Assert.Null(await repository.GetAsync(newcomer.AccountId, created.TripId, CancellationToken.None));

            viewerMembership.End();
            await db.SaveChangesAsync();
            Assert.Null(await repository.GetAsync(viewer.AccountId, created.TripId, CancellationToken.None));
            await db.FamilyGroupMemberships.Where(m => m.Id == viewerMembership.Id)
                .ExecuteUpdateAsync(update => update.SetProperty(m => m.IsActive, true));
            Assert.Null(await repository.GetAsync(viewer.AccountId, created.TripId, CancellationToken.None));
            Assert.DoesNotContain(await repository.GetVisibleAsync(viewer.AccountId, CancellationToken.None),
                trip => trip.TripId == created.TripId);

            creatorMembership.End();
            await db.SaveChangesAsync();
            Assert.NotNull(await repository.GetAsync(creator.AccountId, created.TripId, CancellationToken.None));
            Assert.Contains(await repository.GetVisibleAsync(creator.AccountId, CancellationToken.None),
                trip => trip.TripId == created.TripId);
            var updated = await repository.UpdateAsync(creator.AccountId, created.TripId,
                Update([traveler.ProfileId]) with { Title = "Updated Trip" }, CancellationToken.None);
            Assert.Equal("Updated Trip", updated?.Title);
            Assert.Equal(3, await db.FamilyTripAudienceMembers.CountAsync(member => member.TripId == created.TripId));
        });
    }

    [Fact]
    public async Task SelectedAudienceKeepsTravelerAndViewerAccessSeparateAndProtectsContacts()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        await WithDatabaseAsync(async db =>
        {
            var creator = AddAdult(db, "Creator");
            var viewer = AddAdult(db, "Viewer");
            var traveler = AddAdult(db, "Traveler");
            var otherFamilyPerson = AddAdult(db, "Other Family Person");
            var administrator = AddAdult(db, "Administrator");
            var outsider = AddAdult(db, "Outsider");
            var group = new FamilyGroup(Guid.NewGuid(), "Trip family", 100);
            var otherGroup = new FamilyGroup(Guid.NewGuid(), "Other family", 100);
            var administratorMembership = new FamilyGroupMembership(
                Guid.NewGuid(), administrator.AccountId, administrator.ProfileId, group.Id, true);
            db.AddRange(group, otherGroup,
                new FamilyGroupMembership(Guid.NewGuid(), creator.AccountId, creator.ProfileId, group.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), viewer.AccountId, viewer.ProfileId, group.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), traveler.AccountId, traveler.ProfileId, group.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), otherFamilyPerson.AccountId, otherFamilyPerson.ProfileId, otherGroup.Id, true),
                administratorMembership,
                new FamilyAdministratorAssignment(Guid.NewGuid(), administratorMembership.Id),
                new FamilyGroupMembership(Guid.NewGuid(), outsider.AccountId, outsider.ProfileId, otherGroup.Id, true));
            await db.SaveChangesAsync();
            var repository = new EfFamilyTripRepository(db);

            var created = await repository.CreateAsync(creator.AccountId, group.Id,
                Create([traveler.ProfileId], TripAudienceType.SelectedPeople, [viewer.ProfileId]), CancellationToken.None);
            Assert.NotNull(created);
            Assert.Equal(viewer.ProfileId, Assert.Single(created.AudienceRecipients).ProfileId);
            var viewerTrip = await repository.GetAsync(viewer.AccountId, created.TripId, CancellationToken.None);
            Assert.NotNull(viewerTrip);
            Assert.Empty(viewerTrip.AudienceRecipients);
            Assert.Equal("guide@example.com", Assert.Single(viewerTrip.Contacts).Email);
            Assert.Null(await repository.GetAsync(traveler.AccountId, created.TripId, CancellationToken.None));
            Assert.Null(await repository.GetAsync(administrator.AccountId, created.TripId, CancellationToken.None));
            Assert.Null(await repository.GetAsync(outsider.AccountId, created.TripId, CancellationToken.None));
            Assert.Empty(await repository.GetVisibleAsync(traveler.AccountId, CancellationToken.None));
            Assert.Null(await repository.UpdateAsync(viewer.AccountId, created.TripId,
                Update([traveler.ProfileId]), CancellationToken.None));
            Assert.Null(await repository.PreviewAudienceReplacementAsync(viewer.AccountId, created.TripId,
                new ReplaceTripAudienceRequest(TripAudienceType.SelectedPeople, [traveler.ProfileId]), CancellationToken.None));
            Assert.Null(await repository.ReplaceAudienceAsync(viewer.AccountId, created.TripId,
                new ReplaceTripAudienceRequest(TripAudienceType.SelectedPeople, [traveler.ProfileId]), "forged", CancellationToken.None));
            await Assert.ThrowsAsync<FamilyTripValidationException>(() => repository.PreviewAudienceReplacementAsync(
                creator.AccountId, created.TripId,
                new ReplaceTripAudienceRequest(TripAudienceType.SelectedPeople, [otherFamilyPerson.ProfileId]), CancellationToken.None));
            await Assert.ThrowsAsync<FamilyTripValidationException>(() => repository.PreviewAudienceReplacementAsync(
                creator.AccountId, created.TripId,
                new ReplaceTripAudienceRequest(TripAudienceType.SelectedPeople, [creator.ProfileId]), CancellationToken.None));

            var before = await db.FamilyTripAudienceMembers.AsNoTracking()
                .Where(member => member.TripId == created.TripId).Select(member => member.ProfileId).ToListAsync();
            var updated = await repository.UpdateAsync(creator.AccountId, created.TripId,
                Update([traveler.ProfileId]) with
                {
                    Contacts = [new TripContactInput("New guide", null, "+44 20 7946 0958", null)],
                }, CancellationToken.None);
            Assert.Equal("New guide", Assert.Single(updated!.Contacts).Name);
            Assert.Equal(before, await db.FamilyTripAudienceMembers.AsNoTracking()
                .Where(member => member.TripId == created.TripId).Select(member => member.ProfileId).ToListAsync());
        });
    }

    [Fact]
    public async Task CreationRejectsIneligibleTravelersAndRecipientsWithoutSavingTrip()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        await WithDatabaseAsync(async db =>
        {
            var creator = AddAdult(db, "Creator");
            var active = AddAdult(db, "Active");
            var inactive = AddAdult(db, "Inactive");
            var outsider = AddAdult(db, "Outsider");
            var group = new FamilyGroup(Guid.NewGuid(), "Trip family", 100);
            var otherGroup = new FamilyGroup(Guid.NewGuid(), "Other", 100);
            db.AddRange(group, otherGroup,
                new FamilyGroupMembership(Guid.NewGuid(), creator.AccountId, creator.ProfileId, group.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), active.AccountId, active.ProfileId, group.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), inactive.AccountId, inactive.ProfileId, group.Id, false),
                new FamilyGroupMembership(Guid.NewGuid(), outsider.AccountId, outsider.ProfileId, otherGroup.Id, true));
            await db.SaveChangesAsync();
            var repository = new EfFamilyTripRepository(db);

            Assert.Null(await repository.CreateAsync(creator.AccountId, group.Id,
                Create([inactive.ProfileId], TripAudienceType.FamilyGroup, []), CancellationToken.None));
            Assert.Null(await repository.CreateAsync(creator.AccountId, group.Id,
                Create([active.ProfileId], TripAudienceType.SelectedPeople, [outsider.ProfileId]), CancellationToken.None));
            Assert.Null(await repository.CreateAsync(creator.AccountId, group.Id,
                Create([active.ProfileId], TripAudienceType.SelectedPeople, [inactive.ProfileId]), CancellationToken.None));
            var selfOnly = await Assert.ThrowsAsync<FamilyTripValidationException>(() =>
                repository.CreateAsync(creator.AccountId, group.Id,
                    Create([active.ProfileId], TripAudienceType.SelectedPeople, [creator.ProfileId]), CancellationToken.None));
            Assert.Contains("AudienceProfileIds", selfOnly.Errors.Keys);
            Assert.Empty(db.FamilyTrips);
        });
    }

    [Fact]
    public async Task AudienceReplacementIsAtomicAndImmediatelyRevokesTripAndNoteAccess()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        await WithDatabaseAsync(async db =>
        {
            var creator = AddAdult(db, "Creator");
            var oldViewer = AddAdult(db, "Old viewer");
            var newViewer = AddAdult(db, "New viewer");
            var group = new FamilyGroup(Guid.NewGuid(), "Audience family", 100);
            db.AddRange(group,
                new FamilyGroupMembership(Guid.NewGuid(), creator.AccountId, creator.ProfileId, group.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), oldViewer.AccountId, oldViewer.ProfileId, group.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), newViewer.AccountId, newViewer.ProfileId, group.Id, true));
            await db.SaveChangesAsync();
            var trips = new EfFamilyTripRepository(db);
            var trip = await trips.CreateAsync(creator.AccountId, group.Id,
                Create([creator.ProfileId], TripAudienceType.SelectedPeople, [oldViewer.ProfileId]), CancellationToken.None);
            Assert.NotNull(trip);

            var now = DateTimeOffset.UtcNow;
            var active = new FamilyTripNote(Guid.NewGuid(), trip.TripId, oldViewer.ProfileId, "Active data", now);
            var deleted = new FamilyTripNote(Guid.NewGuid(), trip.TripId, oldViewer.ProfileId, "Deleted data", now.AddMinutes(1));
            deleted.DeleteByAuthor(now.AddMinutes(2));
            var hidden = new FamilyTripNote(Guid.NewGuid(), trip.TripId, oldViewer.ProfileId, "Hidden data", now.AddMinutes(3));
            hidden.HideByCreator(now.AddMinutes(4));
            db.FamilyTripNotes.AddRange(active, deleted, hidden);
            await db.SaveChangesAsync();

            var proposal = new ReplaceTripAudienceRequest(TripAudienceType.SelectedPeople, [newViewer.ProfileId]);
            var preview = await trips.PreviewAudienceReplacementAsync(creator.AccountId, trip.TripId,
                proposal, CancellationToken.None);
            Assert.Equal("New viewer", Assert.Single(preview!.GainingAccess).DisplayName);
            Assert.Equal("Old viewer", Assert.Single(preview.LosingAccess).DisplayName);
            Assert.Equal(TripAudienceType.SelectedPeople, preview.AudienceType);

            await trips.ReplaceAudienceAsync(creator.AccountId, trip.TripId, proposal,
                preview.PreviewToken, CancellationToken.None);
            Assert.Null(await trips.GetAsync(oldViewer.AccountId, trip.TripId, CancellationToken.None));
            Assert.NotNull(await trips.GetAsync(newViewer.AccountId, trip.TripId, CancellationToken.None));
            Assert.DoesNotContain(await trips.GetVisibleAsync(oldViewer.AccountId, CancellationToken.None), item => item.TripId == trip.TripId);
            Assert.Contains(await trips.GetVisibleAsync(newViewer.AccountId, CancellationToken.None), item => item.TripId == trip.TripId);
            Assert.Equal(new[] { newViewer.ProfileId }, await db.FamilyTripAudienceMembers.AsNoTracking()
                .Where(row => row.TripId == trip.TripId).Select(row => row.ProfileId).ToArrayAsync());
            var notes = new EfFamilyTripNoteRepository(db, TimeProvider.System);
            Assert.Null(await notes.ListAsync(oldViewer.AccountId, trip.TripId, CancellationToken.None));
            Assert.Null(await notes.AddAsync(oldViewer.AccountId, trip.TripId, "stale write", CancellationToken.None));
            Assert.Null(await notes.EditAsync(oldViewer.AccountId, trip.TripId, active.Id, "stale edit", CancellationToken.None));
            var visibleToNew = await notes.ListAsync(newViewer.AccountId, trip.TripId, CancellationToken.None);
            Assert.Equal("Active data", Assert.Single(visibleToNew!).Content);
            Assert.False(await notes.DeleteAsync(oldViewer.AccountId, trip.TripId, active.Id, CancellationToken.None));

            var persisted = await db.FamilyTripNotes.AsNoTracking().Where(note => note.TripId == trip.TripId)
                .OrderBy(note => note.CreatedAt).ToListAsync();
            Assert.Equal(3, persisted.Count);
            Assert.Equal("Active data", persisted[0].Content);
            Assert.Equal(oldViewer.ProfileId, persisted[0].AuthorProfileId);
            Assert.Equal(now, persisted[0].CreatedAt);
            Assert.Equal(TripNoteState.AuthorDeleted, persisted[1].State);
            Assert.Equal(TripNoteState.CreatorHidden, persisted[2].State);
        });
    }

    [Fact]
    public async Task StalePreviewAndDepartedCreatorCannotExpandSavedAudience()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        await WithDatabaseAsync(async db =>
        {
            var creator = AddAdult(db, "Creator");
            var saved = AddAdult(db, "Saved viewer");
            var newPerson = AddAdult(db, "New person");
            var group = new FamilyGroup(Guid.NewGuid(), "Audience family", 100);
            var creatorMembership = new FamilyGroupMembership(Guid.NewGuid(), creator.AccountId, creator.ProfileId, group.Id, true);
            db.AddRange(group, creatorMembership,
                new FamilyGroupMembership(Guid.NewGuid(), saved.AccountId, saved.ProfileId, group.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), newPerson.AccountId, newPerson.ProfileId, group.Id, true));
            await db.SaveChangesAsync();
            var repository = new EfFamilyTripRepository(db);
            var trip = await repository.CreateAsync(creator.AccountId, group.Id,
                Create([creator.ProfileId], TripAudienceType.SelectedPeople, [saved.ProfileId]), CancellationToken.None);
            var staleProposal = new ReplaceTripAudienceRequest(TripAudienceType.SelectedPeople, [newPerson.ProfileId]);
            var stalePreview = await repository.PreviewAudienceReplacementAsync(creator.AccountId,
                trip!.TripId, staleProposal, CancellationToken.None);
            var newMembership = await db.FamilyGroupMemberships.SingleAsync(m => m.AdultProfileId == newPerson.ProfileId);
            newMembership.End();
            await db.SaveChangesAsync();
            await Assert.ThrowsAsync<FamilyTripAudienceConflictException>(() => repository.ReplaceAudienceAsync(
                creator.AccountId, trip.TripId, staleProposal, stalePreview!.PreviewToken, CancellationToken.None));
            Assert.Null(await repository.GetAsync(newPerson.AccountId, trip.TripId, CancellationToken.None));
            Assert.NotNull(await repository.GetAsync(saved.AccountId, trip.TripId, CancellationToken.None));

            creatorMembership.End();
            await db.SaveChangesAsync();
            var departedAllowed = new ReplaceTripAudienceRequest(TripAudienceType.SelectedPeople, [saved.ProfileId]);
            var reductionPreview = await repository.PreviewAudienceReplacementAsync(creator.AccountId,
                trip.TripId, departedAllowed, CancellationToken.None);
            Assert.NotNull(reductionPreview);
            await Assert.ThrowsAsync<FamilyTripValidationException>(() => repository.PreviewAudienceReplacementAsync(
                creator.AccountId, trip.TripId,
                new ReplaceTripAudienceRequest(TripAudienceType.SelectedPeople, [newPerson.ProfileId]), CancellationToken.None));
            await Assert.ThrowsAsync<FamilyTripValidationException>(() => repository.PreviewAudienceReplacementAsync(
                creator.AccountId, trip.TripId,
                new ReplaceTripAudienceRequest(TripAudienceType.FamilyGroup, []), CancellationToken.None));
            Assert.NotNull(await repository.GetAsync(creator.AccountId, trip.TripId, CancellationToken.None));
            Assert.Null(await repository.GetAsync(newPerson.AccountId, trip.TripId, CancellationToken.None));
        });
    }

    [Fact]
    public async Task GroupRefreshUsesFreshSnapshotAndCoreEditPreservesReplacement()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        await WithDatabaseAsync(async db =>
        {
            var creator = AddAdult(db, "Creator");
            var first = AddAdult(db, "First");
            var second = AddAdult(db, "Second");
            var later = AddAdult(db, "Later joiner");
            var group = new FamilyGroup(Guid.NewGuid(), "Audience family", 100);
            var secondMembership = new FamilyGroupMembership(
                Guid.NewGuid(), second.AccountId, second.ProfileId, group.Id, true);
            db.AddRange(group,
                new FamilyGroupMembership(Guid.NewGuid(), creator.AccountId, creator.ProfileId, group.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), first.AccountId, first.ProfileId, group.Id, true),
                secondMembership);
            await db.SaveChangesAsync();
            var repository = new EfFamilyTripRepository(db);
            var trip = await repository.CreateAsync(creator.AccountId, group.Id,
                Create([first.ProfileId], TripAudienceType.SelectedPeople, [first.ProfileId]), CancellationToken.None);
            db.FamilyGroupMemberships.Add(new FamilyGroupMembership(
                Guid.NewGuid(), later.AccountId, later.ProfileId, group.Id, true));
            await db.SaveChangesAsync();
            var refresh = new ReplaceTripAudienceRequest(TripAudienceType.FamilyGroup, []);
            var preview = await repository.PreviewAudienceReplacementAsync(creator.AccountId,
                trip!.TripId, refresh, CancellationToken.None);
            Assert.Equal(4, preview!.AudienceRecipients.Count);
            Assert.Contains(preview.GainingAccess, person => person.ProfileId == second.ProfileId);
            await repository.ReplaceAudienceAsync(creator.AccountId, trip.TripId,
                refresh, preview.PreviewToken, CancellationToken.None);
            Assert.Equal(TripAudienceType.FamilyGroup,
                (await repository.GetAsync(later.AccountId, trip.TripId, CancellationToken.None))!.AudienceType);

            secondMembership.End();
            var refreshJoiner = AddAdult(db, "Refresh joiner");
            db.FamilyGroupMemberships.Add(new FamilyGroupMembership(
                Guid.NewGuid(), refreshJoiner.AccountId, refreshJoiner.ProfileId, group.Id, true));
            await db.SaveChangesAsync();
            var refreshAgain = await repository.PreviewAudienceReplacementAsync(creator.AccountId,
                trip.TripId, refresh, CancellationToken.None);
            Assert.DoesNotContain(refreshAgain!.AudienceRecipients, person => person.ProfileId == second.ProfileId);
            Assert.Contains(refreshAgain.AudienceRecipients, person => person.ProfileId == refreshJoiner.ProfileId);
            await repository.ReplaceAudienceAsync(creator.AccountId, trip.TripId,
                refresh, refreshAgain.PreviewToken, CancellationToken.None);
            var refreshedIds = await db.FamilyTripAudienceMembers.AsNoTracking()
                .Where(row => row.TripId == trip.TripId).Select(row => row.ProfileId).ToArrayAsync();
            Assert.DoesNotContain(second.ProfileId, refreshedIds);
            Assert.Contains(refreshJoiner.ProfileId, refreshedIds);

            var selectedAgain = new ReplaceTripAudienceRequest(TripAudienceType.SelectedPeople, [first.ProfileId]);
            var selectedPreview = await repository.PreviewAudienceReplacementAsync(creator.AccountId,
                trip.TripId, selectedAgain, CancellationToken.None);
            Assert.DoesNotContain(selectedPreview!.LosingAccess, person => person.ProfileId == second.ProfileId);
            Assert.Contains(selectedPreview.LosingAccess, person => person.ProfileId == later.ProfileId);
            Assert.Contains(selectedPreview.LosingAccess, person => person.ProfileId == refreshJoiner.ProfileId);
            await repository.ReplaceAudienceAsync(creator.AccountId, trip.TripId,
                selectedAgain, selectedPreview.PreviewToken, CancellationToken.None);
            Assert.Null(await repository.GetAsync(later.AccountId, trip.TripId, CancellationToken.None));

            var joiner = AddAdult(db, "After refresh");
            db.FamilyGroupMemberships.Add(new FamilyGroupMembership(
                Guid.NewGuid(), joiner.AccountId, joiner.ProfileId, group.Id, true));
            await db.SaveChangesAsync();
            Assert.Null(await repository.GetAsync(joiner.AccountId, trip.TripId, CancellationToken.None));
            var beforeEdit = await db.FamilyTripAudienceMembers.AsNoTracking()
                .Where(row => row.TripId == trip.TripId).OrderBy(row => row.ProfileId)
                .Select(row => row.ProfileId).ToArrayAsync();
            await repository.UpdateAsync(creator.AccountId, trip.TripId, Update([first.ProfileId]), CancellationToken.None);
            Assert.Equal(beforeEdit, await db.FamilyTripAudienceMembers.AsNoTracking()
                .Where(row => row.TripId == trip.TripId).OrderBy(row => row.ProfileId)
                .Select(row => row.ProfileId).ToArrayAsync());
        });
    }

    private static CreateFamilyTripRequest Create(IReadOnlyList<Guid> travelers,
        TripAudienceType audienceType, IReadOnlyList<Guid> recipients) => new(
        "Family visit", "Atlanta", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5),
        "Visit relatives", "Flight booked", "Staying with family",
        new DateOnly(2026, 10, 1), new TimeOnly(17, 0), "UTC",
        travelers, audienceType, recipients,
        [new TripContactInput("Guide", "Tour guide", "+44 20 7946 0958", "guide@example.com")]);

    private static UpdateFamilyTripRequest Update(IReadOnlyList<Guid> travelers) => new(
        "Family visit", "Atlanta", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5),
        null, null, null, null, null, null, travelers, []);

    private static (Guid AccountId, Guid ProfileId) AddAdult(FamilySafetyDbContext db, string name)
    {
        var account = Guid.NewGuid();
        var profile = Guid.NewGuid();
        db.AddRange(new Account(account, $"{account:N}@example.invalid"), new AdultProfile(profile, account, name));
        return (account, profile);
    }

    private static async Task WithDatabaseAsync(Func<FamilySafetyDbContext, Task> test)
    {
        var connection = $"Server=localhost;Database=FamilySafety_WI007003_TripTests_{Guid.NewGuid():N};Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
        var options = new DbContextOptionsBuilder<FamilySafetyDbContext>().UseSqlServer(connection).Options;
        await using var db = new FamilySafetyDbContext(options);
        var cleanupRequired = false;
        try
        {
            await db.Database.EnsureDeletedAsync();
            cleanupRequired = true;
            await db.Database.MigrateAsync();
            await test(db);
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
