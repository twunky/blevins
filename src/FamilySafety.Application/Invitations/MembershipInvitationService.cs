using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using FamilySafety.Application.Profiles;

namespace FamilySafety.Application.Invitations;

public sealed class MembershipInvitationService(
    ICurrentAccountAccessor currentAccountAccessor,
    IMembershipInvitationRepository repository,
    IMembershipInvitationDelivery delivery,
    TimeProvider timeProvider) : IMembershipInvitationService
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromDays(7);

    public Task<IReadOnlyList<AdministeredFamilyGroupView>> GetAdministeredGroupsAsync(
        CancellationToken cancellationToken) =>
        repository.GetAdministeredGroupsAsync(GetAccountId(), cancellationToken);

    public async Task IssueAsync(IssueMembershipInvitationRequest request, CancellationToken cancellationToken)
    {
        var email = ValidateEmail(request.RecipientEmail);
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var now = timeProvider.GetUtcNow();
        var outcome = await repository.IssueAsync(
            GetAccountId(), request.FamilyGroupId, email, NormalizeEmail(email), Hash(token),
            now, now.Add(Lifetime), cancellationToken);
        ThrowFor(outcome);
        await delivery.SendAsync(email, token, cancellationToken);
    }

    public async Task RevokeAsync(RevokeMembershipInvitationRequest request, CancellationToken cancellationToken)
    {
        var email = ValidateEmail(request.RecipientEmail);
        ThrowFor(await repository.RevokeAsync(
            GetAccountId(), request.FamilyGroupId, NormalizeEmail(email), timeProvider.GetUtcNow(), cancellationToken));
    }

    public async Task<MembershipInvitationPreview> PreviewAsync(
        InvitationTokenRequest request,
        CancellationToken cancellationToken)
    {
        var data = await repository.PreviewAsync(
            GetAccountId(), Hash(ValidateToken(request.Token)), timeProvider.GetUtcNow(), cancellationToken);
        ThrowFor(data.Outcome);
        return new MembershipInvitationPreview(data.FamilyGroupName!, data.InviterDisplayName!);
    }

    public async Task<MembershipInvitationAccepted> AcceptAsync(
        InvitationTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await repository.AcceptAsync(
            GetAccountId(), Hash(ValidateToken(request.Token)), timeProvider.GetUtcNow(), cancellationToken);
        ThrowFor(result.Outcome);
        return new MembershipInvitationAccepted(result.FamilyGroupId
            ?? throw new InvalidOperationException("Successful acceptance must identify the Family Group."));
    }

    public async Task DeclineAsync(InvitationTokenRequest request, CancellationToken cancellationToken) =>
        ThrowFor(await repository.DeclineAsync(
            GetAccountId(), Hash(ValidateToken(request.Token)), timeProvider.GetUtcNow(), cancellationToken));

    private Guid GetAccountId() => currentAccountAccessor.Current?.AccountId
        ?? throw new CurrentAccountUnavailableException();

    private static string ValidateEmail(string? value)
    {
        var email = value?.Trim() ?? string.Empty;
        if (email.Length is 0 or > 254 || !new EmailAddressAttribute().IsValid(email))
        {
            throw new MembershipInvitationValidationException(
                new Dictionary<string, string[]> { ["recipientEmail"] = ["Enter a valid email address."] });
        }

        return email;
    }

    private static string ValidateToken(string? value)
    {
        var token = value?.Trim() ?? string.Empty;
        if (token.Length is 0 or > 128)
        {
            throw new MembershipInvitationUnavailableException();
        }

        return token;
    }

    private static string NormalizeEmail(string email) => email.ToUpperInvariant();

    private static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static void ThrowFor(InvitationRepositoryOutcome outcome)
    {
        if (outcome == InvitationRepositoryOutcome.Unavailable)
        {
            throw new MembershipInvitationUnavailableException();
        }

        if (outcome == InvitationRepositoryOutcome.CapacityReached)
        {
            throw new MembershipInvitationCapacityException();
        }
    }
}
