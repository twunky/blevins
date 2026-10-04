namespace FamilySafety.Application.Trips;

public sealed record TripNoteView(Guid NoteId, Guid AuthorProfileId, string AuthorDisplayName,
    string Content, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt,
    bool CanEdit, bool CanDelete, bool CanHide);

public sealed record TripNoteContentRequest(string? Content);

public sealed class FamilyTripNoteUnavailableException : Exception;

public sealed class FamilyTripNoteValidationException : Exception;
