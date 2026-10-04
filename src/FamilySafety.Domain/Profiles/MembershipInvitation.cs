namespace FamilySafety.Domain.Profiles;

public sealed class MembershipInvitation
{
    private MembershipInvitation()
    {
    }

    public MembershipInvitation(
        Guid id,
        Guid familyGroupId,
        Guid issuedByMembershipId,
        string recipientEmail,
        string normalizedRecipientEmail,
        string tokenHash,
        DateTimeOffset issuedAt,
        DateTimeOffset expiresAt)
    {
        if (id == Guid.Empty || familyGroupId == Guid.Empty || issuedByMembershipId == Guid.Empty)
        {
            throw new ArgumentException("Invitation identifiers are required.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(recipientEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedRecipientEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        if (expiresAt <= issuedAt)
        {
            throw new ArgumentOutOfRangeException(nameof(expiresAt), "Expiration must follow issue time.");
        }

        Id = id;
        FamilyGroupId = familyGroupId;
        IssuedByMembershipId = issuedByMembershipId;
        RecipientEmail = recipientEmail;
        NormalizedRecipientEmail = normalizedRecipientEmail;
        TokenHash = tokenHash;
        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
        Status = MembershipInvitationStatus.Pending;
    }

    public Guid Id { get; private set; }

    public Guid FamilyGroupId { get; private set; }

    public Guid IssuedByMembershipId { get; private set; }

    public string RecipientEmail { get; private set; } = string.Empty;

    public string NormalizedRecipientEmail { get; private set; } = string.Empty;

    public string TokenHash { get; private set; } = string.Empty;

    public MembershipInvitationStatus Status { get; private set; }

    public DateTimeOffset IssuedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? TerminalAt { get; private set; }

    public Guid? AcceptedByAccountId { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public bool IsAvailable(DateTimeOffset now) => Status == MembershipInvitationStatus.Pending && now < ExpiresAt;

    public void Expire(DateTimeOffset now)
    {
        if (Status == MembershipInvitationStatus.Pending && now >= ExpiresAt)
        {
            Complete(MembershipInvitationStatus.Expired, now);
        }
    }

    public void Revoke(DateTimeOffset now) => Complete(MembershipInvitationStatus.Revoked, now);

    public void Decline(DateTimeOffset now) => Complete(MembershipInvitationStatus.Declined, now);

    public void Accept(Guid accountId, DateTimeOffset now)
    {
        if (accountId == Guid.Empty)
        {
            throw new ArgumentException("An accepting account ID is required.", nameof(accountId));
        }

        Complete(MembershipInvitationStatus.Accepted, now);
        AcceptedByAccountId = accountId;
    }

    private void Complete(MembershipInvitationStatus status, DateTimeOffset now)
    {
        if (!IsAvailable(now) && !(status == MembershipInvitationStatus.Expired && Status == MembershipInvitationStatus.Pending))
        {
            throw new InvalidOperationException("Only an available invitation can change state.");
        }

        Status = status;
        TerminalAt = now;
    }
}
