namespace FamilySafety.Application.Events;

// The only normal Update projection: no hidden state or revision properties.
public sealed record EventUpdateView(Guid UpdateId, Guid AuthorProfileId, string AuthorName, string Text,
    DateTimeOffset CreatedAtUtc, DateTimeOffset? UpdatedAtUtc, Guid Version);
public sealed record EventUpdatesView(bool CanManage, IReadOnlyList<EventUpdateView> Updates);
public sealed record CreateEventUpdateRequest(string Text);
public sealed record EditEventUpdateRequest(Guid Version, string Text);
public sealed record HideEventUpdateRequest(Guid Version);
public sealed class EventUpdateConflictException : Exception { }

public interface IEventUpdateRepository
{
    Task<EventUpdatesView?> GetAsync(Guid accountId, Guid eventId, CancellationToken ct);
    Task<EventUpdateView?> CreateAsync(Guid accountId, Guid eventId, CreateEventUpdateRequest request, CancellationToken ct);
    Task<EventUpdateView?> EditAsync(Guid accountId, Guid eventId, Guid updateId, EditEventUpdateRequest request, CancellationToken ct);
    Task<bool> HideAsync(Guid accountId, Guid eventId, Guid updateId, Guid version, CancellationToken ct);
}
public interface IEventUpdateService
{
    Task<EventUpdatesView> GetAsync(Guid eventId, CancellationToken ct);
    Task<EventUpdateView> CreateAsync(Guid eventId, CreateEventUpdateRequest request, CancellationToken ct);
    Task<EventUpdateView> EditAsync(Guid eventId, Guid updateId, EditEventUpdateRequest request, CancellationToken ct);
    Task HideAsync(Guid eventId, Guid updateId, Guid version, CancellationToken ct);
}
