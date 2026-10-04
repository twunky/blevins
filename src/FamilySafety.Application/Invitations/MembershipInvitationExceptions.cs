namespace FamilySafety.Application.Invitations;

public sealed class MembershipInvitationValidationException(
    IReadOnlyDictionary<string, string[]> errors) : Exception("The invitation request is invalid.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}

public sealed class MembershipInvitationUnavailableException : Exception;

public sealed class MembershipInvitationCapacityException : Exception;

public sealed class MembershipInvitationDeliveryUnavailableException : Exception;
