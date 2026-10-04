namespace FamilySafety.Application.Invitations;

public sealed record AdministeredFamilyGroupView(Guid FamilyGroupId, string Name);

public sealed record IssueMembershipInvitationRequest(Guid FamilyGroupId, string? RecipientEmail);

public sealed record RevokeMembershipInvitationRequest(Guid FamilyGroupId, string? RecipientEmail);

public sealed record InvitationTokenRequest(string? Token);

public sealed record MembershipInvitationPreview(string FamilyGroupName, string InviterDisplayName);

public enum InvitationRepositoryOutcome
{
    Success,
    Unavailable,
    CapacityReached
}

public sealed record InvitationPreviewData(
    InvitationRepositoryOutcome Outcome,
    string? FamilyGroupName = null,
    string? InviterDisplayName = null);

public sealed record MembershipInvitationAccepted(Guid FamilyGroupId);

public sealed record InvitationAcceptanceData(InvitationRepositoryOutcome Outcome, Guid? FamilyGroupId = null);
