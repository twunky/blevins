using FamilySafety.Domain.Relationships;

namespace FamilySafety.Application.Relationships;

public sealed record FamilyRelationshipView(
    Guid RelationshipId,
    Guid ProposerProfileId,
    string ProposerDisplayName,
    FamilyRelationshipLabel ProposerLabel,
    Guid RecipientProfileId,
    string RecipientDisplayName,
    FamilyRelationshipLabel RecipientLabel,
    FamilyRelationshipStatus Status,
    bool IsCurrentUserProposer,
    bool IsCurrentUserRecipient);

public sealed record ProposeFamilyRelationshipRequest(
    Guid RecipientProfileId,
    FamilyRelationshipLabel ProposerLabel);

public enum FamilyRelationshipAction
{
    Accept,
    Decline,
    Withdraw,
    Remove,
}

public enum FamilyRelationshipOperationResult
{
    Success,
    Unavailable,
    Conflict,
}
