using FamilySafety.Domain.Events;

namespace FamilySafety.Foundation.Tests;

public sealed class EventNeedDomainTests
{
    [Fact]
    public void AssignmentRequiresAcceptanceAndCompletionCannotBeReleasedOrRetaken()
    {
        var now = DateTimeOffset.UtcNow;
        var need = new FamilyEventNeed(Guid.NewGuid(), Guid.NewGuid(), "Need", null, null, now);
        var profile = Guid.NewGuid(); var original = need.Version;
        Assert.Equal(EventNeedStatus.Open, need.Status); Assert.Null(need.ResponsibleProfileId);
        Assert.Throws<InvalidOperationException>(() => need.Accept(now));
        Assert.Throws<InvalidOperationException>(() => need.Complete(now));
        need.Take(profile, true, now);
        Assert.NotEqual(original, need.Version); Assert.Equal(EventNeedStatus.Assigned, need.Status);
        Assert.Throws<InvalidOperationException>(() => need.Take(Guid.NewGuid(), false, now));
        need.Accept(now); Assert.Equal(EventNeedStatus.Claimed, need.Status);
        need.Complete(now); Assert.Equal(EventNeedStatus.Completed, need.Status); Assert.Equal(profile, need.ResponsibleProfileId);
        Assert.Throws<InvalidOperationException>(() => need.Release(now));
        Assert.Throws<InvalidOperationException>(() => need.Take(profile, false, now));
    }
    [Fact]
    public void ReleaseClearsResponsibilityAndAllowsANewVoluntaryClaim()
    {
        var now = DateTimeOffset.UtcNow;
        var need = new FamilyEventNeed(Guid.NewGuid(), Guid.NewGuid(), "Need", null, null, now);
        need.Take(Guid.NewGuid(), true, now); need.Release(now);
        Assert.Equal(EventNeedStatus.Open, need.Status); Assert.Null(need.ResponsibleProfileId);
        need.Take(Guid.NewGuid(), false, now); need.Release(now);
        Assert.Equal(EventNeedStatus.Open, need.Status); Assert.Null(need.ResponsibleProfileId);
    }
}
