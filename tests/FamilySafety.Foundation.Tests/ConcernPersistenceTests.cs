using FamilySafety.Application.Concerns;
using FamilySafety.Domain.Concerns;
using FamilySafety.Domain.Profiles;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Foundation.Tests;

public sealed class ConcernPersistenceTests
{
    private static readonly DateTimeOffset Start = new(2026, 9, 30, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task SqlAudienceAndEntryHistoryArePersistedAndNondisclosing()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var creator = AddAdult(db, "Creator");
            var subject = AddAdult(db, "Subject");
            var participant = AddAdult(db, "Participant");
            var administrator = AddAdult(db, "Administrator");
            var otherGroupAdult = AddAdult(db, "Other group");
            var inactiveAdult = AddAdult(db, "Inactive");
            var nonmember = AddAdult(db, "Nonmember");
            var group = new FamilyGroup(Guid.NewGuid(), "Concern family", 100);
            var otherGroup = new FamilyGroup(Guid.NewGuid(), "Other group", 100);
            var adminMembership = new FamilyGroupMembership(Guid.NewGuid(), administrator.AccountId,
                administrator.ProfileId, group.Id, true);
            db.AddRange(group, otherGroup,
                Membership(creator, group.Id), Membership(subject, group.Id),
                Membership(participant, group.Id), adminMembership,
                new FamilyGroupMembership(Guid.NewGuid(), inactiveAdult.AccountId,
                    inactiveAdult.ProfileId, group.Id, false),
                Membership(otherGroupAdult, otherGroup.Id),
                new FamilyAdministratorAssignment(Guid.NewGuid(), adminMembership.Id));
            await db.SaveChangesAsync();

            var repository = new EfConcernRepository(db, clock);
            var created = await repository.CreateAsync(creator.AccountId, group.Id,
                Create(subject.ProfileId, [participant.ProfileId]), CancellationToken.None);
            Assert.NotNull(created);
            var selfConcern = await repository.CreateAsync(creator.AccountId, group.Id,
                Create(creator.ProfileId, []), CancellationToken.None);
            Assert.Equal(creator.ProfileId, selfConcern!.SubjectProfileId);
            Assert.Null(await repository.CreateAsync(otherGroupAdult.AccountId, group.Id,
                Create(subject.ProfileId, []), CancellationToken.None));
            Assert.Null(await repository.CreateAsync(nonmember.AccountId, group.Id,
                Create(subject.ProfileId, []), CancellationToken.None));
            Assert.Null(await repository.CreateAsync(creator.AccountId, group.Id,
                Create(inactiveAdult.ProfileId, []), CancellationToken.None));
            Assert.Null(await repository.CreateAsync(creator.AccountId, group.Id,
                Create(otherGroupAdult.ProfileId, []), CancellationToken.None));
            Assert.Null(await repository.GetAsync(subject.AccountId, created.ConcernId, CancellationToken.None));
            Assert.Null(await repository.GetAsync(administrator.AccountId, created.ConcernId, CancellationToken.None));
            Assert.Null(await repository.GetAsync(otherGroupAdult.AccountId, created.ConcernId, CancellationToken.None));
            Assert.Null(await repository.GetAsync(Guid.NewGuid(), created.ConcernId, CancellationToken.None));
            Assert.NotNull(await repository.GetAsync(participant.AccountId, created.ConcernId, CancellationToken.None));

            var participantReport = await repository.AddEntryAsync(participant.AccountId, created.ConcernId,
                new AddConcernEntryRequest(ConcernEntryKind.Observation, ConcernInformationCategory.Reported,
                    "I heard the arrival was delayed", "Direct conversation", false, null, null, null), CancellationToken.None);
            Assert.Contains(participantReport!.Entries, entry => entry.ContributorProfileId == participant.ProfileId);
            var unknown = new AddConcernEntryRequest(ConcernEntryKind.Unknown,
                ConcernInformationCategory.Unknown, "Arrival time is unknown", null, true,
                null, null, null);
            var afterUnknown = await repository.AddEntryAsync(participant.AccountId, created.ConcernId,
                unknown, CancellationToken.None);
            var original = Assert.Single(afterUnknown!.Entries, entry => entry.Kind == ConcernEntryKind.Unknown);
            Assert.Equal(ConcernInformationCategory.Unknown, original.Category);
            Assert.True(original.SourceUnknown);

            var correction = new AddConcernEntryRequest(ConcernEntryKind.Correction,
                ConcernInformationCategory.Reported, "It was reported at 5 PM", "Direct call", false,
                Start.AddHours(-1), original.EntryId, "Correcting my earlier unknown", OriginalCategory: null);
            var corrected = await repository.AddEntryAsync(participant.AccountId, created.ConcernId,
                correction, CancellationToken.None);
            Assert.Equal(3, corrected!.Entries.Count);
            Assert.Contains(corrected.Entries, entry => entry.EntryId == original.EntryId
                && entry.Kind == ConcernEntryKind.Unknown && entry.Text == "Arrival time is unknown");
            Assert.Contains(corrected.Entries, entry => entry.Kind == ConcernEntryKind.Correction
                && entry.RelatedEntryId == original.EntryId);

            var creatorEntry = await repository.AddEntryAsync(creator.AccountId, created.ConcernId,
                new AddConcernEntryRequest(ConcernEntryKind.Observation, ConcernInformationCategory.Reported,
                    "Creator report", "Direct conversation", false, null, null, null), CancellationToken.None);
            var creatorOriginal = creatorEntry!.Entries.Single(entry => entry.Text == "Creator report");
            Assert.Null(await repository.AddEntryAsync(participant.AccountId, created.ConcernId,
                correction with { RelatedEntryId = creatorOriginal.EntryId }, CancellationToken.None));

            var challenge = new AddConcernEntryRequest(ConcernEntryKind.Challenge,
                ConcernInformationCategory.Assumption, "I heard a different time", "Second-hand report", false,
                null, original.EntryId, "Contrary report");
            var challenged = await repository.AddEntryAsync(creator.AccountId, created.ConcernId,
                challenge, CancellationToken.None);
            Assert.Equal(5, challenged!.Entries.Count);
            Assert.Contains(challenged.Entries, entry => entry.EntryId == original.EntryId
                && entry.Kind == ConcernEntryKind.Unknown);
            Assert.Contains(challenged.Entries, entry => entry.Kind == ConcernEntryKind.Challenge
                && entry.RelatedEntryId == original.EntryId);
            Assert.True(await db.ConcernAuditEvents.AnyAsync(a => a.Action == "ConcernReadDenied" && !a.Authorized));
            Assert.True(await db.ConcernAuditEvents.AnyAsync(a => a.Action == "CorrectionCreated" && a.Authorized));
            Assert.True(await db.ConcernAuditEvents.AnyAsync(a => a.Action == "ChallengeCreated" && a.Authorized));
            Assert.Null(await repository.ReplaceParticipantsAsync(participant.AccountId, created.ConcernId,
                new ReplaceConcernParticipantsRequest([]), CancellationToken.None));
            Assert.Null(await repository.ResolveAsync(participant.AccountId, created.ConcernId,
                new ResolveConcernRequest("Participant cannot resolve"), CancellationToken.None));
            var removed = await repository.ReplaceParticipantsAsync(creator.AccountId, created.ConcernId,
                new ReplaceConcernParticipantsRequest([]), CancellationToken.None);
            Assert.Empty(removed!.Participants);
            Assert.Null(await repository.GetAsync(participant.AccountId, created.ConcernId, CancellationToken.None));
            var resolved = await repository.ResolveAsync(creator.AccountId, created.ConcernId,
                new ResolveConcernRequest("Coordination ended"), CancellationToken.None);
            Assert.Equal(ConcernState.Resolved, resolved!.State);
            Assert.Equal("Coordination ended", resolved.ResolutionNote);
            Assert.Null(await repository.AddEntryAsync(creator.AccountId, created.ConcernId,
                unknown, CancellationToken.None));
            Assert.True(await db.ConcernAuditEvents.AnyAsync(a => a.Action == "ConcernWriteDenied" && !a.Authorized));
            Assert.DoesNotContain(await db.ConcernAuditEvents.Select(a => a.Action).ToListAsync(),
                action => action.Contains("unknown", StringComparison.OrdinalIgnoreCase)
                    || action.Contains("heard", StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public async Task SqlMembershipEndRevokesGrantAndRejoinRequiresCreatorReselection()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var creator = AddAdult(db, "Creator");
            var subject = AddAdult(db, "Subject");
            var participant = AddAdult(db, "Participant");
            var group = new FamilyGroup(Guid.NewGuid(), "Concern family", 100);
            var creatorMembership = Membership(creator, group.Id);
            var participantMembership = Membership(participant, group.Id);
            db.AddRange(group, creatorMembership, Membership(subject, group.Id), participantMembership);
            await db.SaveChangesAsync();
            var repository = new EfConcernRepository(db, clock);
            var created = await repository.CreateAsync(creator.AccountId, group.Id,
                Create(subject.ProfileId, [participant.ProfileId]), CancellationToken.None);
            Assert.NotNull(created);
            var oldVersion = participantMembership.AccessVersion;

            participantMembership.End(Start.AddDays(1));
            await db.SaveChangesAsync();
            Assert.Equal(Start.AddDays(1), participantMembership.EndedAt);
            Assert.Null(await repository.GetAsync(participant.AccountId, created.ConcernId, CancellationToken.None));
            Assert.Null(await repository.AddEntryAsync(participant.AccountId, created.ConcernId,
                new AddConcernEntryRequest(ConcernEntryKind.Unknown, ConcernInformationCategory.Unknown,
                    "Stale write", null, true, null, null, null), CancellationToken.None));
            await db.FamilyGroupMemberships.Where(m => m.Id == participantMembership.Id)
                .ExecuteUpdateAsync(update => update.SetProperty(m => m.IsActive, true));
            db.ChangeTracker.Clear();
            Assert.Null(await repository.GetAsync(participant.AccountId, created.ConcernId, CancellationToken.None));
            Assert.Null(await repository.AddEntryAsync(participant.AccountId, created.ConcernId,
                new AddConcernEntryRequest(ConcernEntryKind.Unknown, ConcernInformationCategory.Unknown,
                    "Rejoined stale write", null, true, null, null, null), CancellationToken.None));
            Assert.Equal(oldVersion + 1, await db.FamilyGroupMemberships.Where(m => m.Id == participantMembership.Id)
                .Select(m => m.AccessVersion).SingleAsync());

            var reselected = await repository.ReplaceParticipantsAsync(creator.AccountId,
                created.ConcernId, new ReplaceConcernParticipantsRequest([participant.ProfileId]), CancellationToken.None);
            Assert.NotNull(reselected);
            Assert.NotNull(await repository.GetAsync(participant.AccountId, created.ConcernId, CancellationToken.None));
        });
    }

    [Fact]
    public async Task SqlCreatorLossFreezesAtLossAndInactivityExpiryDeletesOnlyProductHistory()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var creator = AddAdult(db, "Creator");
            var subject = AddAdult(db, "Subject");
            var participant = AddAdult(db, "Participant");
            var group = new FamilyGroup(Guid.NewGuid(), "Concern family", 100);
            var creatorMembership = Membership(creator, group.Id);
            db.AddRange(group, creatorMembership, Membership(subject, group.Id), Membership(participant, group.Id));
            await db.SaveChangesAsync();
            var repository = new EfConcernRepository(db, clock);
            var created = await repository.CreateAsync(creator.AccountId, group.Id,
                Create(subject.ProfileId, [participant.ProfileId]), CancellationToken.None);
            Assert.NotNull(created);

            var lossAt = Start.AddDays(5);
            creatorMembership.End(lossAt);
            await db.SaveChangesAsync();
            Assert.Null(await repository.GetAsync(creator.AccountId, created.ConcernId, CancellationToken.None));
            var frozen = await repository.GetAsync(participant.AccountId, created.ConcernId, CancellationToken.None);
            Assert.NotNull(frozen);
            Assert.Equal(ConcernState.Frozen, frozen.State);
            Assert.Equal(lossAt, frozen.FrozenAt);
            Assert.Equal(ConcernFreezeReason.CreatorEligibilityLost, frozen.FreezeReason);
            Assert.Null(await repository.AddEntryAsync(participant.AccountId, created.ConcernId,
                new AddConcernEntryRequest(ConcernEntryKind.Unknown, ConcernInformationCategory.Unknown,
                    "Unknown", null, true, null, null, null), CancellationToken.None));
            Assert.Null(await repository.ReplaceParticipantsAsync(participant.AccountId, created.ConcernId,
                new ReplaceConcernParticipantsRequest([]), CancellationToken.None));
            Assert.Null(await repository.ResolveAsync(participant.AccountId, created.ConcernId,
                new ResolveConcernRequest("Not permitted"), CancellationToken.None));
            await repository.ReconcileAsync(CancellationToken.None);
            Assert.Equal(1, await db.ConcernAuditEvents.CountAsync(a => a.Action == "Frozen"));
            Assert.Equal(1, await db.ConcernAuditEvents.CountAsync(a => a.Action == "AccessRevoked"));
        });

        await WithDatabaseAsync(async (db, clock) =>
        {
            var creator = AddAdult(db, "Creator");
            var subject = AddAdult(db, "Subject");
            var participant = AddAdult(db, "Participant");
            var group = new FamilyGroup(Guid.NewGuid(), "Concern family", 100);
            db.AddRange(group, Membership(creator, group.Id), Membership(subject, group.Id), Membership(participant, group.Id));
            await db.SaveChangesAsync();
            var repository = new EfConcernRepository(db, clock);
            var created = await repository.CreateAsync(creator.AccountId, group.Id,
                Create(subject.ProfileId, [participant.ProfileId]), CancellationToken.None);
            Assert.NotNull(created);
            await repository.AddEntryAsync(creator.AccountId, created.ConcernId,
                new AddConcernEntryRequest(ConcernEntryKind.Observation, ConcernInformationCategory.Reported,
                    "Reported detail", "Direct report", false, null, null, null), CancellationToken.None);

            clock.Set(Start.AddDays(30));
            var frozen = await repository.GetAsync(participant.AccountId, created.ConcernId, CancellationToken.None);
            Assert.Equal(ConcernState.Frozen, frozen!.State);
            Assert.Equal(Start.AddDays(30), frozen.FrozenAt);
            clock.Set(Start.AddDays(210));
            Assert.Null(await repository.GetAsync(participant.AccountId, created.ConcernId, CancellationToken.None));
            Assert.Empty(await repository.GetVisibleAsync(participant.AccountId, CancellationToken.None));
            await repository.ReconcileAsync(CancellationToken.None);
            Assert.Equal(0, await db.Concerns.CountAsync());
            Assert.Equal(0, await db.ConcernEntries.CountAsync());
            Assert.Equal(0, await db.ConcernParticipantGrants.CountAsync());
            Assert.True(await db.ConcernAuditEvents.AnyAsync(a => a.Action == "ConcernReadDenied"
                && a.OccurredAt == Start.AddDays(210)));
            Assert.DoesNotContain(await db.ConcernAuditEvents.Select(a => a.Action).ToListAsync(),
                action => action.Contains("Reported detail", StringComparison.Ordinal));
        });
    }

    [Fact]
    public async Task SqlDeniedAuditSummariesAreBoundedByActorCategoryAndUtcHourIncludingConcurrency()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var actor = AddAdult(db, "Denied actor");
            var otherActor = AddAdult(db, "Other denied actor");
            await db.SaveChangesAsync();
            var repository = new EfConcernRepository(db, clock);

            Assert.Null(await repository.GetAsync(actor.AccountId, Guid.NewGuid(), CancellationToken.None));
            clock.Set(Start.AddMinutes(47));
            Assert.Null(await repository.GetAsync(actor.AccountId, Guid.NewGuid(), CancellationToken.None));

            var readSummary = await db.ConcernAuditEvents.AsNoTracking().SingleAsync(a =>
                a.ActorAccountId == actor.AccountId && a.Action == "ConcernReadDenied");
            Assert.Null(readSummary.ConcernId);
            Assert.False(readSummary.Authorized);
            Assert.Equal(Start, readSummary.UtcHourBucket);
            Assert.Equal(Start, readSummary.FirstAttemptAt);
            Assert.Equal(Start.AddMinutes(47), readSummary.LastAttemptAt);
            Assert.Equal(2, readSummary.DeniedAttemptCount);
            Assert.Equal(Start.AddDays(180), readSummary.ExpiresAt);

            clock.Set(Start.AddMinutes(55));
            Assert.Null(await repository.CreateAsync(actor.AccountId, Guid.NewGuid(),
                Create(Guid.NewGuid(), []), CancellationToken.None));
            var writeSummary = await db.ConcernAuditEvents.AsNoTracking().SingleAsync(a =>
                a.ActorAccountId == actor.AccountId && a.Action == "ConcernWriteDenied");
            Assert.Null(writeSummary.ConcernId);
            Assert.Equal(1, writeSummary.DeniedAttemptCount);
            Assert.Equal(Start, writeSummary.UtcHourBucket);

            clock.Set(Start.AddHours(1));
            Assert.Null(await repository.GetAsync(actor.AccountId, Guid.NewGuid(), CancellationToken.None));
            Assert.Null(await repository.GetAsync(otherActor.AccountId, Guid.NewGuid(), CancellationToken.None));
            Assert.Equal(3, await db.ConcernAuditEvents.CountAsync(a => a.Action == "ConcernReadDenied"));

            clock.Set(Start.AddHours(2));
            var connection = db.Database.GetDbConnection().ConnectionString;
            await Task.WhenAll(Enumerable.Range(0, 16).Select(async _ =>
            {
                var options = new DbContextOptionsBuilder<FamilySafetyDbContext>().UseSqlServer(connection).Options;
                await using var concurrentDb = new FamilySafetyDbContext(options);
                var concurrentRepository = new EfConcernRepository(concurrentDb, clock);
                Assert.Null(await concurrentRepository.GetAsync(actor.AccountId, Guid.NewGuid(), CancellationToken.None));
            }));
            var concurrentSummary = await db.ConcernAuditEvents.AsNoTracking().SingleAsync(a =>
                a.ActorAccountId == actor.AccountId && a.Action == "ConcernReadDenied"
                && a.UtcHourBucket == Start.AddHours(2));
            Assert.Equal(16, concurrentSummary.DeniedAttemptCount);
            Assert.Equal(1, await db.ConcernAuditEvents.CountAsync(a =>
                a.ActorAccountId == actor.AccountId && a.Action == "ConcernReadDenied"
                && a.UtcHourBucket == Start.AddHours(2)));
            Assert.Null(concurrentSummary.ConcernId);

            clock.Set(Start.AddDays(180));
            await repository.ReconcileAsync(CancellationToken.None);
            Assert.False(await db.ConcernAuditEvents.AnyAsync(a =>
                a.ActorAccountId == actor.AccountId && a.Action == "ConcernReadDenied"
                && a.UtcHourBucket == Start));
        });
    }

    [Fact]
    public async Task SqlMembershipLossCommitSerializesAgainstConcernWrite()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var creator = AddAdult(db, "Creator");
            var subject = AddAdult(db, "Subject");
            var participant = AddAdult(db, "Participant");
            var group = new FamilyGroup(Guid.NewGuid(), "Concern family", 100);
            var participantMembership = Membership(participant, group.Id);
            db.AddRange(group, Membership(creator, group.Id), Membership(subject, group.Id), participantMembership);
            await db.SaveChangesAsync();
            var created = await new EfConcernRepository(db, clock).CreateAsync(creator.AccountId, group.Id,
                Create(subject.ProfileId, [participant.ProfileId]), CancellationToken.None);
            Assert.NotNull(created);

            var connection = db.Database.GetDbConnection().ConnectionString;
            var options = new DbContextOptionsBuilder<FamilySafetyDbContext>().UseSqlServer(connection).Options;
            await using var endDb = new FamilySafetyDbContext(options);
            await using var endTransaction = await endDb.Database.BeginTransactionAsync();
            var membershipToEnd = await endDb.FamilyGroupMemberships.SingleAsync(m => m.Id == participantMembership.Id);
            membershipToEnd.End(Start.AddMinutes(1));
            await endDb.SaveChangesAsync();

            clock.Set(Start.AddMinutes(2));
            var writeStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var writeTask = Task.Run(async () =>
            {
                await using var writeDb = new FamilySafetyDbContext(options);
                writeStarted.SetResult();
                return await new EfConcernRepository(writeDb, clock).AddEntryAsync(participant.AccountId,
                    created.ConcernId, new AddConcernEntryRequest(ConcernEntryKind.Unknown,
                        ConcernInformationCategory.Unknown, "Stale concurrent write", null, true,
                        null, null, null), CancellationToken.None);
            });
            await writeStarted.Task;
            await Task.Delay(150);
            await endTransaction.CommitAsync();

            Assert.Null(await writeTask.WaitAsync(TimeSpan.FromSeconds(10)));
            Assert.Equal(0, await db.ConcernEntries.CountAsync());
            var denied = await db.ConcernAuditEvents.AsNoTracking().SingleAsync(a =>
                a.ActorAccountId == participant.AccountId && a.Action == "ConcernWriteDenied");
            Assert.Equal(1, denied.DeniedAttemptCount);
        });
    }

    [Fact]
    public async Task SqlConcurrentReconciliationMaterializesLifecycleEventsOnce()
    {
        if (Environment.GetEnvironmentVariable("RUN_FAMILYSAFETY_SQL_TESTS") != "1") return;
        await WithDatabaseAsync(async (db, clock) =>
        {
            var creator = AddAdult(db, "Creator");
            var subject = AddAdult(db, "Subject");
            var group = new FamilyGroup(Guid.NewGuid(), "Concern family", 100);
            db.AddRange(group, Membership(creator, group.Id), Membership(subject, group.Id));
            await db.SaveChangesAsync();
            var created = await new EfConcernRepository(db, clock).CreateAsync(creator.AccountId, group.Id,
                Create(subject.ProfileId, []), CancellationToken.None);
            Assert.NotNull(created);
            clock.Set(Start.AddDays(30));

            var options = new DbContextOptionsBuilder<FamilySafetyDbContext>()
                .UseSqlServer(db.Database.GetDbConnection().ConnectionString).Options;
            async Task SweepAsync()
            {
                await using var sweepDb = new FamilySafetyDbContext(options);
                await new EfConcernRepository(sweepDb, clock).ReconcileAsync(CancellationToken.None);
            }
            await Task.WhenAll(SweepAsync(), SweepAsync());

            Assert.Equal(ConcernState.Frozen,
                await db.Concerns.Where(c => c.Id == created.ConcernId).Select(c => c.State).SingleAsync());
            Assert.Equal(1, await db.ConcernAuditEvents.CountAsync(a => a.ConcernId == created.ConcernId
                && a.Action == "Frozen"));
        });
    }

    private static CreateConcernRequest Create(Guid subjectId, IReadOnlyList<Guid> participants) =>
        new(subjectId, "An expected arrival has not been confirmed", participants);

    private static FamilyGroupMembership Membership((Guid AccountId, Guid ProfileId) adult, Guid groupId) =>
        new(Guid.NewGuid(), adult.AccountId, adult.ProfileId, groupId, true);

    private static (Guid AccountId, Guid ProfileId) AddAdult(FamilySafetyDbContext db, string name)
    {
        var accountId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        db.AddRange(new Account(accountId, $"{accountId:N}@example.invalid"),
            new AdultProfile(profileId, accountId, name));
        return (accountId, profileId);
    }

    private static async Task WithDatabaseAsync(Func<FamilySafetyDbContext, ManualTimeProvider, Task> test)
    {
        var connection = $"Server=localhost;Database=FamilySafety_WI008001_ConcernTests_{Guid.NewGuid():N};Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
        var options = new DbContextOptionsBuilder<FamilySafetyDbContext>().UseSqlServer(connection).Options;
        var clock = new ManualTimeProvider(Start);
        await using var db = new FamilySafetyDbContext(options);
        var cleanupRequired = false;
        try
        {
            await db.Database.EnsureDeletedAsync();
            cleanupRequired = true;
            await db.Database.MigrateAsync();
            await test(db, clock);
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

    private sealed class ManualTimeProvider(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset current = now;
        public override DateTimeOffset GetUtcNow() => current;
        public void Set(DateTimeOffset value) => current = value;
    }
}
