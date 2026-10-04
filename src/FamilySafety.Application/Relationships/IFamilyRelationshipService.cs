namespace FamilySafety.Application.Relationships;

public interface IFamilyRelationshipService
{
    Task<IReadOnlyList<FamilyRelationshipView>> GetForGroupAsync(
        Guid familyGroupId,
        CancellationToken cancellationToken);

    Task ProposeAsync(
        Guid familyGroupId,
        ProposeFamilyRelationshipRequest request,
        CancellationToken cancellationToken);

    Task RespondAsync(
        Guid relationshipId,
        FamilyRelationshipAction action,
        CancellationToken cancellationToken);
}
