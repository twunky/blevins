using FamilySafety.Application.Profiles;
using FamilySafety.Domain.Relationships;

namespace FamilySafety.Application.Relationships;

public sealed class FamilyRelationshipService(
    ICurrentAccountAccessor currentAccountAccessor,
    IFamilyRelationshipRepository repository) : IFamilyRelationshipService
{
    public async Task<IReadOnlyList<FamilyRelationshipView>> GetForGroupAsync(
        Guid familyGroupId,
        CancellationToken cancellationToken)
    {
        if (familyGroupId == Guid.Empty)
        {
            throw new FamilyRelationshipUnavailableException();
        }

        return await repository.GetForGroupAsync(GetAccountId(), familyGroupId, cancellationToken)
            ?? throw new FamilyRelationshipUnavailableException();
    }

    public async Task ProposeAsync(
        Guid familyGroupId,
        ProposeFamilyRelationshipRequest request,
        CancellationToken cancellationToken)
    {
        if (familyGroupId == Guid.Empty || request.RecipientProfileId == Guid.Empty
            || !Enum.IsDefined(request.ProposerLabel))
        {
            throw InvalidRequest();
        }

        ThrowFor(await repository.ProposeAsync(
            GetAccountId(), familyGroupId, request.RecipientProfileId,
            request.ProposerLabel, cancellationToken));
    }

    public async Task RespondAsync(
        Guid relationshipId,
        FamilyRelationshipAction action,
        CancellationToken cancellationToken)
    {
        if (relationshipId == Guid.Empty || !Enum.IsDefined(action))
        {
            throw new FamilyRelationshipUnavailableException();
        }

        ThrowFor(await repository.RespondAsync(
            GetAccountId(), relationshipId, action, cancellationToken));
    }

    private Guid GetAccountId() => currentAccountAccessor.Current?.AccountId
        ?? throw new CurrentAccountUnavailableException();

    private static FamilyRelationshipValidationException InvalidRequest() =>
        new(new Dictionary<string, string[]>
        {
            ["relationship"] = ["Choose an eligible family member and a supported relationship label."],
        });

    private static void ThrowFor(FamilyRelationshipOperationResult result)
    {
        if (result == FamilyRelationshipOperationResult.Unavailable)
        {
            throw new FamilyRelationshipUnavailableException();
        }

        if (result == FamilyRelationshipOperationResult.Conflict)
        {
            throw new FamilyRelationshipConflictException();
        }
    }
}
