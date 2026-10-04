using FamilySafety.Application.Profiles;

namespace FamilySafety.Application.Directory;

public sealed class FamilyDirectoryService(
    ICurrentAccountAccessor currentAccountAccessor,
    IFamilyDirectoryRepository repository) : IFamilyDirectoryService
{
    public Task<IReadOnlyList<ActiveFamilyGroupView>> GetActiveGroupsAsync(CancellationToken cancellationToken) =>
        repository.GetActiveGroupsAsync(RequireAccountId(), cancellationToken);

    public async Task<IReadOnlyList<FamilyDirectoryEntry>> GetMembersAsync(
        Guid familyGroupId,
        string? search,
        CancellationToken cancellationToken)
    {
        if (familyGroupId == Guid.Empty)
        {
            throw new FamilyDirectoryUnavailableException();
        }

        var members = await repository.GetActiveMembersAsync(
            RequireAccountId(), familyGroupId, cancellationToken)
            ?? throw new FamilyDirectoryUnavailableException();
        var normalizedSearch = search?.Trim();

        return members
            .Where(member => string.IsNullOrEmpty(normalizedSearch)
                || member.DisplayName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase))
            .OrderBy(member => member.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(member => member.ProfileId)
            .Select(member => new FamilyDirectoryEntry(member.ProfileId, member.DisplayName))
            .ToArray();
    }

    private Guid RequireAccountId() => currentAccountAccessor.Current?.AccountId
        ?? throw new CurrentAccountUnavailableException();
}
