namespace FamilySafety.Application.Invitations;

public interface IMembershipInvitationRepository
{
    Task<IReadOnlyList<AdministeredFamilyGroupView>> GetAdministeredGroupsAsync(
        Guid accountId,
        CancellationToken cancellationToken);

    Task<InvitationRepositoryOutcome> IssueAsync(
        Guid accountId,
        Guid familyGroupId,
        string recipientEmail,
        string normalizedRecipientEmail,
        string tokenHash,
        DateTimeOffset issuedAt,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken);

    Task<InvitationRepositoryOutcome> RevokeAsync(
        Guid accountId,
        Guid familyGroupId,
        string normalizedRecipientEmail,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    Task<InvitationPreviewData> PreviewAsync(
        Guid accountId,
        string tokenHash,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    Task<InvitationAcceptanceData> AcceptAsync(
        Guid accountId,
        string tokenHash,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    Task<InvitationRepositoryOutcome> DeclineAsync(
        Guid accountId,
        string tokenHash,
        DateTimeOffset now,
        CancellationToken cancellationToken);
}

public interface IMembershipInvitationDelivery
{
    Task SendAsync(string recipientEmail, string token, CancellationToken cancellationToken);
}
