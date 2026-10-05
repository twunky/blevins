using FamilySafety.Domain.Events;

namespace FamilySafety.Application.Events;

public sealed record EventNeedView(Guid NeedId, string Title, string? Detail, EventNeedCategory? Category,
    EventNeedStatus Status, Guid? ResponsibleProfileId, string? ResponsibleName, bool IsResponsible,
    DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc, Guid Version,
    IReadOnlyList<EventNeedHistoryView> History);
public sealed record EventNeedHistoryView(Guid ProfileId, string DisplayName, EventNeedStatus PriorStatus,
    EventNeedStatus Status, string Reason, DateTimeOffset OccurredAtUtc);
public sealed record EventNeedsView(bool CanManage, bool CanParticipate, IReadOnlyList<EventInviteeChoice> EligibleParticipants,
    IReadOnlyList<EventNeedView> Needs);
public sealed record CreateEventNeedRequest(string Title, string? Detail, EventNeedCategory? Category);
public sealed record EditEventNeedRequest(Guid Version, string Title, string? Detail, EventNeedCategory? Category);
public sealed record EventNeedActionRequest(Guid Version);
public sealed record AssignEventNeedRequest(Guid Version, Guid ProfileId);
public sealed class EventNeedConflictException : Exception { }

public interface IEventNeedRepository
{
    Task<EventNeedsView?> GetAsync(Guid accountId, Guid eventId, CancellationToken ct);
    Task<EventNeedView?> CreateAsync(Guid accountId, Guid eventId, CreateEventNeedRequest request, CancellationToken ct);
    Task<EventNeedView?> EditAsync(Guid accountId, Guid eventId, Guid needId, EditEventNeedRequest request, CancellationToken ct);
    Task<bool> RemoveAsync(Guid accountId, Guid eventId, Guid needId, Guid version, CancellationToken ct);
    Task<EventNeedView?> ActAsync(Guid accountId, Guid eventId, Guid needId, Guid version, string action, Guid? assigneeId, CancellationToken ct);
}

public interface IEventNeedService
{
    Task<EventNeedsView> GetAsync(Guid eventId, CancellationToken ct);
    Task<EventNeedView> CreateAsync(Guid eventId, CreateEventNeedRequest request, CancellationToken ct);
    Task<EventNeedView> EditAsync(Guid eventId, Guid needId, EditEventNeedRequest request, CancellationToken ct);
    Task RemoveAsync(Guid eventId, Guid needId, Guid version, CancellationToken ct);
    Task<EventNeedView> ActAsync(Guid eventId, Guid needId, Guid version, string action, Guid? assigneeId, CancellationToken ct);
}
