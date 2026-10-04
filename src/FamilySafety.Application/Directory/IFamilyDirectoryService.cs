namespace FamilySafety.Application.Directory;

public interface IFamilyDirectoryService
{
    Task<IReadOnlyList<ActiveFamilyGroupView>> GetActiveGroupsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<FamilyDirectoryEntry>> GetMembersAsync(
        Guid familyGroupId,
        string? search,
        CancellationToken cancellationToken);
}

public sealed class FamilyDirectoryUnavailableException : Exception;
