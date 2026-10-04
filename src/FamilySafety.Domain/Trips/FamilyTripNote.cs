namespace FamilySafety.Domain.Trips;

public sealed class FamilyTripNote
{
    private FamilyTripNote() { }

    public FamilyTripNote(Guid id, Guid tripId, Guid authorProfileId, string content, DateTimeOffset createdAt)
    {
        if (id == Guid.Empty || tripId == Guid.Empty || authorProfileId == Guid.Empty)
            throw new ArgumentException("Note identifiers are required.");
        Id = id;
        TripId = tripId;
        AuthorProfileId = authorProfileId;
        Content = Normalize(content);
        CreatedAt = createdAt;
        State = TripNoteState.Active;
    }

    public Guid Id { get; private set; }
    public Guid TripId { get; private set; }
    public Guid AuthorProfileId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public TripNoteState State { get; private set; }
    public DateTimeOffset? RemovedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public void Edit(string content, DateTimeOffset updatedAt)
    {
        EnsureActive();
        Content = Normalize(content);
        UpdatedAt = updatedAt;
    }

    public void DeleteByAuthor(DateTimeOffset removedAt)
    {
        EnsureActive();
        State = TripNoteState.AuthorDeleted;
        RemovedAt = removedAt;
    }

    public void HideByCreator(DateTimeOffset removedAt)
    {
        EnsureActive();
        State = TripNoteState.CreatorHidden;
        RemovedAt = removedAt;
    }

    private void EnsureActive()
    {
        if (State != TripNoteState.Active) throw new InvalidOperationException("Note is unavailable.");
    }

    private static string Normalize(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        var normalized = content.Trim();
        if (normalized.Length > 1000) throw new ArgumentOutOfRangeException(nameof(content));
        return normalized;
    }
}
