namespace FamilySafety.Application.Directory;

public interface IFamilyDirectoryRepository
{
    Task<IReadOnlyList<ActiveFamilyGroupView>> GetActiveGroupsAsync(
        Guid accountId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<FamilyDirectoryMemberData>?> GetActiveMembersAsync(
        Guid accountId,
        Guid familyGroupId,
        CancellationToken cancellationToken);
}
