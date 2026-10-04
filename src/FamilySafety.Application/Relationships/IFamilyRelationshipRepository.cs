using FamilySafety.Domain.Relationships;

namespace FamilySafety.Application.Relationships;

public interface IFamilyRelationshipRepository
{
    Task<IReadOnlyList<FamilyRelationshipView>?> GetForGroupAsync(
        Guid accountId,
        Guid familyGroupId,
        CancellationToken cancellationToken);

    Task<FamilyRelationshipOperationResult> ProposeAsync(
        Guid accountId,
        Guid familyGroupId,
        Guid recipientProfileId,
        FamilyRelationshipLabel proposerLabel,
        CancellationToken cancellationToken);

    Task<FamilyRelationshipOperationResult> RespondAsync(
        Guid accountId,
        Guid relationshipId,
        FamilyRelationshipAction action,
        CancellationToken cancellationToken);
}
