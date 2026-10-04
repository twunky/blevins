using FamilySafety.Application.Directory;
using FamilySafety.Application.Profiles;

namespace FamilySafety.Foundation.Tests;

public sealed class FamilyDirectoryServiceTests
{
    private static readonly Guid AccountId = Guid.NewGuid();
    private static readonly Guid GroupId = Guid.NewGuid();

    [Fact]
    public async Task MembersAreTrimmedFilteredAndDeterministicallyOrdered()
    {
        var repository = new FakeDirectoryRepository
        {
            Members =
            [
                new(Guid.Parse("30000000-0000-0000-0000-000000000002"), "alex"),
                new(Guid.Parse("30000000-0000-0000-0000-000000000001"), "Alex"),
                new(Guid.NewGuid(), "Bailey"),
            ],
        };
        var service = new FamilyDirectoryService(new CurrentAccessor(AccountId), repository);

        var result = await service.GetMembersAsync(GroupId, "  ALEx ", CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(Guid.Parse("30000000-0000-0000-0000-000000000001"), result[0].ProfileId);
        Assert.Equal(Guid.Parse("30000000-0000-0000-0000-000000000002"), result[1].ProfileId);
    }

    [Fact]
    public async Task UnavailableGroupUsesOneApplicationError()
    {
        var service = new FamilyDirectoryService(
            new CurrentAccessor(AccountId),
            new FakeDirectoryRepository { Members = null });

        await Assert.ThrowsAsync<FamilyDirectoryUnavailableException>(() =>
            service.GetMembersAsync(GroupId, null, CancellationToken.None));
    }

    [Fact]
    public void DirectoryContractContainsOnlyProfileIdentity()
    {
        var properties = typeof(FamilyDirectoryEntry).GetProperties().Select(property => property.Name).ToArray();

        Assert.Equal(["ProfileId", "DisplayName"], properties);
    }

    private sealed class CurrentAccessor(Guid accountId) : ICurrentAccountAccessor
    {
        public CurrentAccount Current { get; } = new(accountId, IsApplicationOperator: true);
    }

    private sealed class FakeDirectoryRepository : IFamilyDirectoryRepository
    {
        public IReadOnlyList<FamilyDirectoryMemberData>? Members { get; init; } = [];

        public Task<IReadOnlyList<ActiveFamilyGroupView>> GetActiveGroupsAsync(
            Guid accountId,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<ActiveFamilyGroupView>>([]);

        public Task<IReadOnlyList<FamilyDirectoryMemberData>?> GetActiveMembersAsync(
            Guid accountId,
            Guid familyGroupId,
            CancellationToken cancellationToken) => Task.FromResult(Members);
    }
}
