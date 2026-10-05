using FamilySafety.Domain.Events;

namespace FamilySafety.Foundation.Tests;

public sealed class EventUpdateDomainTests
{
    [Fact]
    public void EditsRetainEachPriorTextAndHidePreservesAuthorshipAndTimestamps()
    {
        var now = DateTimeOffset.UtcNow;
        var author = Guid.NewGuid();
        var update = new FamilyEventUpdate(Guid.NewGuid(), Guid.NewGuid(), author, " Original ", now);
        var originalVersion = update.Version;
        Assert.Null(update.UpdatedAtUtc);
        var revision = update.Edit(" Revised ", now.AddMinutes(1));
        Assert.Equal("Original", revision.Text); Assert.Equal(now, revision.TextAtUtc);
        Assert.Equal(now.AddMinutes(1), revision.RevisedAtUtc); Assert.Equal(author, revision.EditorProfileId);
        Assert.NotEqual(originalVersion, update.Version);
        var second = update.Edit("Latest", now.AddMinutes(2));
        Assert.Equal("Revised", second.Text); Assert.Equal(now.AddMinutes(1), second.TextAtUtc);
        var editedVersion = update.Version;
        update.Hide(now.AddMinutes(3));
        Assert.Equal("Latest", update.Text); Assert.Equal(author, update.AuthorProfileId); Assert.Equal(now, update.CreatedAtUtc);
        Assert.Equal(now.AddMinutes(2), update.UpdatedAtUtc); Assert.Equal(now.AddMinutes(3), update.HiddenAtUtc);
        Assert.NotEqual(editedVersion, update.Version);
        Assert.Throws<InvalidOperationException>(() => update.Edit("No restore", now));
        Assert.Throws<InvalidOperationException>(() => update.Hide(now));
    }
    [Theory]
    [InlineData("")] [InlineData("   ")] [InlineData(null)]
    public void BlankTextIsRejected(string? text) => Assert.Throws<ArgumentException>(() =>
        new FamilyEventUpdate(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), text!, DateTimeOffset.UtcNow));

    [Fact]
    public void TextLimitAppliesBeforeChangingExistingText()
    {
        var update = new FamilyEventUpdate(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new string('a', 1000), DateTimeOffset.UtcNow);
        var version = update.Version;
        Assert.Throws<ArgumentException>(() => update.Edit(new string('b', 1001), DateTimeOffset.UtcNow));
        Assert.Equal(version, update.Version); Assert.Null(update.UpdatedAtUtc);
    }
}
