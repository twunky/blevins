using FamilySafety.Domain.Events;

namespace FamilySafety.Application.Events;

public sealed record EventInvitationView(Guid InvitationId, Guid InviteeProfileId,
    DateTimeOffset CreatedAtUtc, DateTimeOffset? RevokedAtUtc, EventRsvpStatus RsvpStatus,
    DateTimeOffset? RsvpUpdatedAtUtc, Guid Version);
public sealed record NamedEventInvitationView(EventInvitationView Invitation, string DisplayName);
public sealed record EventInviteeChoice(Guid ProfileId, string DisplayName);
public sealed record OrganizerInvitationsView(IReadOnlyList<EventInviteeChoice> EligibleViewers,
    IReadOnlyList<NamedEventInvitationView> Invitations);
public sealed record OwnEventInvitationView(EventInvitationView? Invitation);
public sealed record PendingEventInvitationView(Guid InvitationId, Guid EventId, string Name,
    DateOnly Date, TimeOnly Time);
public sealed record IssueEventInvitationRequest(Guid InviteeProfileId);
public sealed record RevokeEventInvitationRequest(Guid Version);
public sealed record UpdateEventRsvpRequest(Guid InvitationId, Guid Version, EventRsvpStatus RsvpStatus);
public sealed class EventInvitationConflictException : Exception { }

public interface IEventInvitationRepository
{
    Task<OrganizerInvitationsView?> GetOrganizerAsync(Guid accountId, Guid eventId, CancellationToken cancellationToken);
    Task<EventInvitationView?> IssueAsync(Guid accountId, Guid eventId, IssueEventInvitationRequest request, CancellationToken cancellationToken);
    Task<EventInvitationView?> RevokeAsync(Guid accountId, Guid eventId, Guid invitationId, RevokeEventInvitationRequest request, CancellationToken cancellationToken);
    Task<OwnEventInvitationView?> GetOwnAsync(Guid accountId, Guid eventId, CancellationToken cancellationToken);
    Task<EventInvitationView?> RespondAsync(Guid accountId, Guid eventId, UpdateEventRsvpRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<PendingEventInvitationView>?> GetPendingAsync(Guid accountId, Guid familyGroupId, CancellationToken cancellationToken);
}

public interface IEventInvitationService
{
    Task<OrganizerInvitationsView> GetOrganizerAsync(Guid eventId, CancellationToken cancellationToken);
    Task<EventInvitationView> IssueAsync(Guid eventId, IssueEventInvitationRequest request, CancellationToken cancellationToken);
    Task<EventInvitationView> RevokeAsync(Guid eventId, Guid invitationId, RevokeEventInvitationRequest request, CancellationToken cancellationToken);
    Task<OwnEventInvitationView> GetOwnAsync(Guid eventId, CancellationToken cancellationToken);
    Task<EventInvitationView> RespondAsync(Guid eventId, UpdateEventRsvpRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<PendingEventInvitationView>> GetPendingAsync(Guid familyGroupId, CancellationToken cancellationToken);
}
