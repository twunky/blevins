using FamilySafety.Domain.Concerns;

namespace FamilySafety.Foundation.Tests;

public sealed class ConcernDomainTests
{
    private static readonly DateTimeOffset Start = new(2026, 9, 30, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void InactivityDeadlineAndExpiryAreDeterministic()
    {
        var concern = NewConcern();
        Assert.Equal(Start.AddDays(30), concern.InactivityFreezeAt);
        Assert.True(concern.IsEffectivelyFrozen(Start.AddDays(30), null));
        Assert.Equal(Start.AddDays(30), concern.EffectiveFreezeAt(null));
        concern.Freeze(Start.AddDays(30), ConcernFreezeReason.Inactivity);
        Assert.Equal(ConcernState.Frozen, concern.State);
        Assert.Equal(Start.AddDays(210), concern.ExpiresAt);
        Assert.True(concern.IsExpired(Start.AddDays(210)));
    }

    [Fact]
    public void EarlierCreatorLossControlsFreezeTimeAndReason()
    {
        var concern = NewConcern();
        var loss = Start.AddDays(10);
        Assert.True(concern.IsEffectivelyFrozen(Start.AddDays(11), loss));
        Assert.Equal(loss, concern.EffectiveFreezeAt(loss));
        Assert.Equal(ConcernFreezeReason.CreatorEligibilityLost, concern.EffectiveFreezeReason(loss));
    }

    [Fact]
    public void EarlierInactivityDeadlineControlsWhenCreatorLossComesLater()
    {
        var concern = NewConcern();
        var inactivity = Start.AddDays(30);
        var laterCreatorLoss = Start.AddDays(31);

        Assert.Equal(inactivity, concern.EffectiveFreezeAt(laterCreatorLoss));
        Assert.Equal(ConcernFreezeReason.Inactivity, concern.EffectiveFreezeReason(laterCreatorLoss));
    }

    [Fact]
    public void ResolutionIsAttributedAndCannotReopenOrAcceptMutation()
    {
        var concern = NewConcern();
        concern.Resolve(Guid.Parse("00000000-0000-0000-0000-000000000002"), "Coordination ended", Start.AddDays(1));
        Assert.Equal(ConcernState.Resolved, concern.State);
        Assert.Equal(Start.AddDays(1), concern.ResolvedAt);
        Assert.Equal(Start.AddDays(181), concern.ExpiresAt);
        Assert.True(concern.IsExpired(Start.AddDays(181)));
        Assert.Throws<InvalidOperationException>(() => concern.RecordAuthorizedMutation(Start.AddDays(2)));
    }

    [Fact]
    public void OutdatedMarkerDoesNotChangeOriginalCategory()
    {
        var entry = new ConcernEntry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ConcernEntryKind.Observation, ConcernInformationCategory.Reported, "Arrival was reported",
            "Contributor report", false, Start, null, null, null, isOutdated: true);
        Assert.Equal(ConcernInformationCategory.Reported, entry.Category);
        Assert.True(entry.IsOutdated);
    }

    [Fact]
    public void DeniedAuditSummaryRetainsFirstAttemptAndAdvancesLastAttempt()
    {
        var summary = ConcernAuditEvent.DeniedSummary(Guid.NewGuid(), "ConcernReadDenied",
            Start, Start.AddMinutes(3), Start.AddMinutes(3));
        summary.RecordDeniedAttempt(Start.AddMinutes(42));

        Assert.Null(summary.ConcernId);
        Assert.False(summary.Authorized);
        Assert.Equal("ConcernReadDenied", summary.Action);
        Assert.Equal(Start, summary.UtcHourBucket);
        Assert.Equal(Start.AddMinutes(3), summary.FirstAttemptAt);
        Assert.Equal(Start.AddMinutes(42), summary.LastAttemptAt);
        Assert.Equal(2, summary.DeniedAttemptCount);
        Assert.Equal(Start.AddMinutes(3).AddDays(180), summary.ExpiresAt);
    }

    private static Concern NewConcern() => new(Guid.NewGuid(), Guid.NewGuid(),
        Guid.Parse("00000000-0000-0000-0000-000000000002"), Guid.NewGuid(), Guid.NewGuid(), 0,
        Guid.NewGuid(), "An expected arrival has not been confirmed", Start);
}
