using FamilySafety.Domain.Events;

namespace FamilySafety.Foundation.Tests;

public sealed class EventInvitationDomainTests
{
    [Fact]
    public void CreationIsPendingWithoutADeliberateResponse()
    {
        var row = NewInvitation();
        Assert.Equal(EventRsvpStatus.Pending, row.RsvpStatus);
        Assert.Null(row.RsvpUpdatedAtUtc);
        Assert.Null(row.RevokedAtUtc);
        Assert.NotEqual(Guid.Empty, row.Version);
    }

    [Theory]
    [InlineData(EventRsvpStatus.Pending)]
    [InlineData(EventRsvpStatus.Going)]
    [InlineData(EventRsvpStatus.Maybe)]
    [InlineData(EventRsvpStatus.NotGoing)]
    public void EveryResponseCanTransitionToEveryApprovedState(EventRsvpStatus from)
    {
        foreach (var to in Enum.GetValues<EventRsvpStatus>())
        {
            var row = NewInvitation();
            row.Respond(from, DateTimeOffset.UtcNow);
            var previous = row.Version;
            var now = DateTimeOffset.UtcNow;
            row.Respond(to, now);
            Assert.Equal(to, row.RsvpStatus);
            Assert.Equal(now, row.RsvpUpdatedAtUtc);
            Assert.NotEqual(previous, row.Version);
        }
    }

    [Fact]
    public void RevocationPreservesFinalResponseAndCannotBeReactivated()
    {
        var row = NewInvitation();
        row.Respond(EventRsvpStatus.Going, DateTimeOffset.UtcNow);
        var responseAt = row.RsvpUpdatedAtUtc;
        row.Revoke(DateTimeOffset.UtcNow);
        Assert.Equal(EventRsvpStatus.Going, row.RsvpStatus);
        Assert.Equal(responseAt, row.RsvpUpdatedAtUtc);
        Assert.Throws<InvalidOperationException>(() => row.Respond(EventRsvpStatus.Maybe, DateTimeOffset.UtcNow));
        Assert.Throws<InvalidOperationException>(() => row.Revoke(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void InvalidResponseAndEmptyIdentitiesAreRejected()
    {
        Assert.Throws<ArgumentException>(() => new FamilyEventInvitation(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow));
        Assert.Throws<ArgumentOutOfRangeException>(() => NewInvitation().Respond((EventRsvpStatus)99, DateTimeOffset.UtcNow));
    }

    private static FamilyEventInvitation NewInvitation() => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
}
