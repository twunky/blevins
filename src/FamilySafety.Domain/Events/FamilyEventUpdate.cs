namespace FamilySafety.Domain.Events;

public sealed class FamilyEventUpdate
{
    public const int TextLimit = 1000;
    private FamilyEventUpdate() { }
    public FamilyEventUpdate(Guid id, Guid eventId, Guid authorProfileId, string text, DateTimeOffset now)
    {
        if (id == Guid.Empty || eventId == Guid.Empty || authorProfileId == Guid.Empty)
            throw new ArgumentException("Update identifiers are required.");
        Id = id; EventId = eventId; AuthorProfileId = authorProfileId;
        Text = Normalize(text); CreatedAtUtc = now; Version = Guid.NewGuid();
    }
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public Guid AuthorProfileId { get; private set; }
    public string Text { get; private set; } = "";
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    public DateTimeOffset? HiddenAtUtc { get; private set; }
    public Guid Version { get; private set; }

    // Return the prior text and its effective time for Event-local retention.
    public EventUpdateRevision Edit(string text, DateTimeOffset now)
    {
        EnsureVisible();
        var normalized = Normalize(text);
        var revision = new EventUpdateRevision(Id, AuthorProfileId, Text, UpdatedAtUtc ?? CreatedAtUtc, now);
        Text = normalized; UpdatedAtUtc = now; Version = Guid.NewGuid();
        return revision;
    }
    public void Hide(DateTimeOffset now)
    {
        EnsureVisible(); HiddenAtUtc = now; Version = Guid.NewGuid();
    }
    private void EnsureVisible()
    {
        if (HiddenAtUtc is not null) throw new InvalidOperationException("This Update is hidden.");
    }
    private static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Trim().Length > TextLimit)
            throw new ArgumentException("Enter Update text of up to 1,000 characters.", nameof(text));
        return text.Trim();
    }
}

// Retained text versions belong only to Event Updates; never serialized in normal APIs.
public sealed class EventUpdateRevision
{
    private EventUpdateRevision() { }
    internal EventUpdateRevision(Guid updateId, Guid editorProfileId, string text, DateTimeOffset textAtUtc, DateTimeOffset revisedAtUtc)
    {
        Id = Guid.NewGuid(); UpdateId = updateId; EditorProfileId = editorProfileId;
        Text = text; TextAtUtc = textAtUtc; RevisedAtUtc = revisedAtUtc;
    }
    public Guid Id { get; private set; }
    public Guid UpdateId { get; private set; }
    public Guid EditorProfileId { get; private set; }
    public string Text { get; private set; } = "";
    public DateTimeOffset TextAtUtc { get; private set; }
    public DateTimeOffset RevisedAtUtc { get; private set; }
}
