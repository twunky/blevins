using FamilySafety.Application.Profiles;

namespace FamilySafety.Application.Events;

public sealed class EventInvitationService(IEventInvitationRepository repository,
    ICurrentAccountAccessor currentAccountAccessor) : IEventInvitationService
{
    private Guid AccountId => currentAccountAccessor.Current?.AccountId
        ?? throw new CurrentAccountUnavailableException();

    public async Task<OrganizerInvitationsView> GetOrganizerAsync(Guid eventId, CancellationToken cancellationToken) =>
        await repository.GetOrganizerAsync(AccountId, eventId, cancellationToken) ?? throw new FamilyEventUnavailableException();
    public async Task<EventInvitationView> IssueAsync(Guid eventId, IssueEventInvitationRequest request, CancellationToken cancellationToken) =>
        await repository.IssueAsync(AccountId, eventId, request, cancellationToken) ?? throw new FamilyEventUnavailableException();
    public async Task<EventInvitationView> RevokeAsync(Guid eventId, Guid invitationId, RevokeEventInvitationRequest request, CancellationToken cancellationToken) =>
        await repository.RevokeAsync(AccountId, eventId, invitationId, request, cancellationToken) ?? throw new FamilyEventUnavailableException();
    public async Task<OwnEventInvitationView> GetOwnAsync(Guid eventId, CancellationToken cancellationToken) =>
        await repository.GetOwnAsync(AccountId, eventId, cancellationToken) ?? throw new FamilyEventUnavailableException();
    public async Task<EventInvitationView> RespondAsync(Guid eventId, UpdateEventRsvpRequest request, CancellationToken cancellationToken) =>
        await repository.RespondAsync(AccountId, eventId, request, cancellationToken) ?? throw new FamilyEventUnavailableException();
    public async Task<IReadOnlyList<PendingEventInvitationView>> GetPendingAsync(Guid familyGroupId, CancellationToken cancellationToken) =>
        await repository.GetPendingAsync(AccountId, familyGroupId, cancellationToken) ?? throw new FamilyEventUnavailableException();
}
