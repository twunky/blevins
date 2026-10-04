using FamilySafety.Domain.Profiles;

namespace FamilySafety.Application.Profiles;

public interface IAdultProfileRepository
{
    Task<AdultProfile?> FindByAccountIdAsync(Guid accountId, CancellationToken cancellationToken);

    Task<AdultProfile?> FindByIdAsync(Guid profileId, CancellationToken cancellationToken);

    Task<bool> AreActiveMembersOfSameGroupAsync(
        Guid requestingAccountId,
        Guid targetAccountId,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
