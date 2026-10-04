namespace FamilySafety.Application.Invitations;

public interface IMembershipInvitationService
{
    Task<IReadOnlyList<AdministeredFamilyGroupView>> GetAdministeredGroupsAsync(CancellationToken cancellationToken);
    Task IssueAsync(IssueMembershipInvitationRequest request, CancellationToken cancellationToken);
    Task RevokeAsync(RevokeMembershipInvitationRequest request, CancellationToken cancellationToken);
    Task<MembershipInvitationPreview> PreviewAsync(InvitationTokenRequest request, CancellationToken cancellationToken);
    Task<MembershipInvitationAccepted> AcceptAsync(InvitationTokenRequest request, CancellationToken cancellationToken);
    Task DeclineAsync(InvitationTokenRequest request, CancellationToken cancellationToken);
}
