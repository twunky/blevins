using FamilySafety.Application.Profiles;
using FamilySafety.Application.Trips;
using FamilySafety.Domain.Profiles;
using FamilySafety.Domain.Sharing;
using FamilySafety.Domain.Trips;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FamilySafety.Foundation.Tests;

public sealed class FamilyTripCompletionTests
{
    [Fact]
    public void CompletionIsExplicitOneWayAndCoreEditsDoNotReopen()
    {
        var trip = new FamilyTrip(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            TripAudienceType.SelectedPeople, "Trip", "Coast", new(2000, 1, 1), new(2000, 1, 2),
            null, null, null, new(2000, 1, 1), new(12, 0), "UTC");
        Assert.False(trip.IsCompleted); // Past end/expected arrival never infer completion.
        var openVersion = trip.Version;
        trip.Complete();
        Assert.True(trip.IsCompleted);
        Assert.NotEqual(openVersion, trip.Version);
        var completedVersion = trip.Version;
        Assert.Throws<InvalidOperationException>(trip.Complete);
        Assert.Equal(completedVersion, trip.Version);
        trip.Update("Edited", "Coast", new(2030, 1, 1), new(2030, 1, 2), null, null, null, null, null, null);
        trip.ReplaceAudience(TripAudienceType.FamilyGroup);
        Assert.True(trip.IsCompleted);
        Assert.DoesNotContain(typeof(FamilyTrip).GetMethods(), m => m.Name.Contains("Reopen", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(typeof(UpdateFamilyTripRequest).GetProperties(), p => p.Name.Contains("Completed", StringComparison.Ordinal));
    }

    [Fact]
    public async Task SqlExactCreatorIncludingDepartedCreatorCompletesAndAccessIsPreserved()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        await WithDatabase(async db =>
        {
            var creator = AddAdult(db, "Creator");
            var viewer = AddAdult(db, "Viewer");
            var admin = AddAdult(db, "Admin");
            var outsider = AddAdult(db, "Known ID only");
            var group = new FamilyGroup(Guid.NewGuid(), "Family", 100);
            var otherGroup = new FamilyGroup(Guid.NewGuid(), "Selected other Family", 100);
            var creatorMembership = new FamilyGroupMembership(Guid.NewGuid(), creator.AccountId, creator.ProfileId, group.Id, true);
            var adminMembership = new FamilyGroupMembership(Guid.NewGuid(), admin.AccountId, admin.ProfileId, group.Id, true);
            db.AddRange(group, otherGroup, creatorMembership, adminMembership,
                new FamilyAdministratorAssignment(Guid.NewGuid(), adminMembership.Id),
                new FamilyGroupMembership(Guid.NewGuid(), viewer.AccountId, viewer.ProfileId, group.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), outsider.AccountId, outsider.ProfileId, otherGroup.Id, true));
            await db.SaveChangesAsync();
            var repository = new EfFamilyTripRepository(db);
            var trip = (await repository.CreateAsync(creator.AccountId, group.Id, Create(viewer.ProfileId), default))!;
            Assert.False(trip.IsCompleted);
            var now = DateTimeOffset.UtcNow;
            var session = new SharingSession(Guid.NewGuid(), group.Id, admin.ProfileId,
                adminMembership.Id, adminMembership.AccessVersion, now, now.AddHours(1));
            var location = new SharingCurrentLocation(session.Id);
            location.Replace(10, 20, 5, now, now);
            db.AddRange(session, location);
            await db.SaveChangesAsync();
            Assert.False((await repository.GetAsync(creator.AccountId, trip.TripId, default))!.IsCompleted);
            foreach (var denied in new[] { viewer.AccountId, admin.AccountId, outsider.AccountId })
            {
                // The service uses the real SQL privacy projection as well as mutation enforcement.
                var service = new FamilyTripService(new AccountAccessor(denied), repository);
                await Assert.ThrowsAsync<FamilyTripUnavailableException>(() => service.CompleteAsync(
                    trip.TripId, new CompleteFamilyTripRequest(trip.Version), default));
                Assert.Null(await repository.CompleteAsync(denied, trip.TripId, trip.Version, default));
            }
            Assert.Null(await repository.CompleteAsync(creator.AccountId, Guid.NewGuid(), trip.Version, default));
            var active = new FamilyTripService(new AccountAccessor(creator.AccountId), repository);
            var second = (await repository.CreateAsync(creator.AccountId, group.Id, Create(viewer.ProfileId), default))!;
            Assert.True((await active.CompleteAsync(second.TripId, new(second.Version), default)).IsCompleted);
            creatorMembership.End();
            await db.SaveChangesAsync();
            var completed = await active.CompleteAsync(trip.TripId, new(trip.Version), default);
            Assert.True(completed.IsCompleted);
            Assert.True(completed.CanEdit);
            Assert.NotEqual(trip.Version, completed.Version);
            Assert.True((await repository.GetAsync(viewer.AccountId, trip.TripId, default))!.IsCompleted);
            Assert.Contains(await repository.GetVisibleAsync(viewer.AccountId, default), t => t.TripId == trip.TripId);
            Assert.Contains(await repository.GetVisibleAsync(creator.AccountId, default), t => t.TripId == trip.TripId);
            var edited = (await repository.UpdateAsync(creator.AccountId, trip.TripId, Update(viewer.ProfileId), default))!;
            Assert.True(edited.IsCompleted);
            Assert.Equal("Edited completed Trip", edited.Title);
            var notes = new EfFamilyTripNoteRepository(db, TimeProvider.System);
            var note = (await notes.AddAsync(viewer.AccountId, trip.TripId, "Still collaborating", default))!;
            Assert.NotNull(await notes.EditAsync(viewer.AccountId, trip.TripId, note.NoteId, "Edited note", default));
            Assert.NotNull(await notes.AddAsync(creator.AccountId, trip.TripId, "Retained creator note", default));
            Assert.Equal(2, (await notes.ListAsync(creator.AccountId, trip.TripId, default))!.Count);
            var audience = new ReplaceTripAudienceRequest(TripAudienceType.SelectedPeople, [viewer.ProfileId]);
            var preview = (await repository.PreviewAudienceReplacementAsync(creator.AccountId, trip.TripId, audience, default))!;
            Assert.True((await repository.ReplaceAudienceAsync(creator.AccountId, trip.TripId, audience, preview.PreviewToken, default))!.IsCompleted);
            db.ChangeTracker.Clear();
            var persisted = (await repository.GetAsync(creator.AccountId, trip.TripId, default))!;
            Assert.True(persisted.IsCompleted);
            await Assert.ThrowsAsync<FamilyTripCompletionConflictException>(() => repository.CompleteAsync(
                creator.AccountId, trip.TripId, persisted.Version, default));
            db.ChangeTracker.Clear();
            Assert.Equal(persisted.Version, (await repository.GetAsync(creator.AccountId, trip.TripId, default))!.Version);
            var unchangedSession = Assert.Single(await db.SharingSessions.AsNoTracking().ToListAsync());
            Assert.Null(unchangedSession.StoppedAtUtc);
            Assert.Equal(now.AddHours(1), unchangedSession.EndsAtUtc);
            Assert.Equal(1, unchangedSession.Version);
            Assert.Equal(10, (await db.SharingCurrentLocations.AsNoTracking().SingleAsync()).Latitude);
            Assert.Empty(await db.CheckIns.ToListAsync());
            Assert.Empty(await db.Concerns.ToListAsync());
            Assert.Empty(await db.Incidents.ToListAsync());
        });
    }

    [Fact]
    public async Task SqlStaleCoreAndAudienceVersionsConflictWithoutChangingState()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        await WithDatabase(async db =>
        {
            var creator = AddAdult(db, "Creator");
            var viewer = AddAdult(db, "Viewer");
            var group = new FamilyGroup(Guid.NewGuid(), "Family", 100);
            db.AddRange(group,
                new FamilyGroupMembership(Guid.NewGuid(), creator.AccountId, creator.ProfileId, group.Id, true),
                new FamilyGroupMembership(Guid.NewGuid(), viewer.AccountId, viewer.ProfileId, group.Id, true));
            await db.SaveChangesAsync();
            var repository = new EfFamilyTripRepository(db);
            var trip = (await repository.CreateAsync(creator.AccountId, group.Id, Create(viewer.ProfileId), default))!;
            // A separate request/context changes the Trip while this context retains its earlier entity.
            await using (var other = new FamilySafetyDbContext(new DbContextOptionsBuilder<FamilySafetyDbContext>()
                .UseSqlServer(db.Database.GetConnectionString()!).Options))
                await new EfFamilyTripRepository(other).UpdateAsync(creator.AccountId, trip.TripId, Update(viewer.ProfileId), default);
            await Assert.ThrowsAsync<FamilyTripCompletionConflictException>(() => repository.CompleteAsync(
                creator.AccountId, trip.TripId, trip.Version, default));
            var current = (await repository.GetAsync(creator.AccountId, trip.TripId, default))!;
            Assert.False(current.IsCompleted);
            Assert.Equal("Edited completed Trip", current.Title);
            var replacement = new ReplaceTripAudienceRequest(TripAudienceType.SelectedPeople, [viewer.ProfileId]);
            var preview = (await repository.PreviewAudienceReplacementAsync(creator.AccountId, trip.TripId, replacement, default))!;
            await repository.ReplaceAudienceAsync(creator.AccountId, trip.TripId, replacement, preview.PreviewToken, default);
            await Assert.ThrowsAsync<FamilyTripCompletionConflictException>(() => repository.CompleteAsync(
                creator.AccountId, trip.TripId, current.Version, default));
            current = (await repository.GetAsync(creator.AccountId, trip.TripId, default))!;
            Assert.False(current.IsCompleted);
            Assert.True((await repository.CompleteAsync(creator.AccountId, trip.TripId, current.Version, default))!.IsCompleted);
        });
    }

    [Fact]
    public async Task SqlMigrationKeepsExistingTripsOpenAndVersionCanBeUsed()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        await WithDatabase(async db =>
        {
            var creator = AddAdult(db, "Legacy creator");
            var group = new FamilyGroup(Guid.NewGuid(), "Legacy Family", 100);
            db.Add(group);
            await db.SaveChangesAsync();
            var migrator = db.GetService<IMigrator>();
            await migrator.MigrateAsync("20261006004226_HouseholdGrouping");
            var id = Guid.NewGuid();
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO FamilyTrips (Id, FamilyGroupId, CreatorProfileId, AudienceType, Title, PrimaryDestination, StartDate, EndDate) VALUES ({id}, {group.Id}, {creator.ProfileId}, N'FamilyGroup', N'Legacy Trip', N'Coast', '20000101', '20000102')");
            await db.Database.MigrateAsync();
            db.ChangeTracker.Clear();
            var repository = new EfFamilyTripRepository(db);
            var migrated = (await repository.GetAsync(creator.AccountId, id, default))!;
            Assert.False(migrated.IsCompleted);
            Assert.True((await repository.CompleteAsync(creator.AccountId, id, migrated.Version, default))!.IsCompleted);
        });
    }

    private sealed class AccountAccessor(Guid accountId) : ICurrentAccountAccessor
    {
        public CurrentAccount? Current => new(accountId);
    }

    private static CreateFamilyTripRequest Create(Guid viewer) => new("Trip", "Coast", new(2000, 1, 1), new(2000, 1, 2),
        null, null, null, new(2000, 1, 1), new(12, 0), "UTC", [viewer], TripAudienceType.SelectedPeople, [viewer], []);
    private static UpdateFamilyTripRequest Update(Guid viewer) => new("Edited completed Trip", "Coast", new(2000, 1, 1), new(2000, 1, 2),
        null, null, null, null, null, null, [viewer], []);
    private static (Guid AccountId, Guid ProfileId) AddAdult(FamilySafetyDbContext db, string name)
    {
        var account = Guid.NewGuid(); var profile = Guid.NewGuid();
        db.AddRange(new Account(account, $"{account:N}@example.invalid"), new AdultProfile(profile, account, name));
        return (account, profile);
    }
    private static async Task WithDatabase(Func<FamilySafetyDbContext, Task> test)
    {
        var options = new DbContextOptionsBuilder<FamilySafetyDbContext>().UseSqlServer(
            $"Server=localhost;Database=FamilySafety_WI105001_TripTests_{Guid.NewGuid():N};Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true").Options;
        await using var db = new FamilySafetyDbContext(options);
        try { await db.Database.MigrateAsync(); await test(db); }
        finally
        {
            // Exact freshly generated disposable database, never an existing user database.
            await using var cleanup = new FamilySafetyDbContext(options);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }
}
