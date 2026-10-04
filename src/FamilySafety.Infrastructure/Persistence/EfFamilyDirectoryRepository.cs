using FamilySafety.Application.Directory;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Infrastructure.Persistence;

public sealed class EfFamilyDirectoryRepository(FamilySafetyDbContext dbContext) : IFamilyDirectoryRepository
{
    public async Task<IReadOnlyList<ActiveFamilyGroupView>> GetActiveGroupsAsync(
        Guid accountId,
        CancellationToken cancellationToken) =>
        await (from membership in dbContext.FamilyGroupMemberships.AsNoTracking()
               join familyGroup in dbContext.FamilyGroups.AsNoTracking()
                   on membership.FamilyGroupId equals familyGroup.Id
               where membership.AccountId == accountId && membership.IsActive
               orderby familyGroup.Name, familyGroup.Id
               select new ActiveFamilyGroupView(familyGroup.Id, familyGroup.Name))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<FamilyDirectoryMemberData>?> GetActiveMembersAsync(
        Guid accountId,
        Guid familyGroupId,
        CancellationToken cancellationToken)
    {
        var capacity = await (from membership in dbContext.FamilyGroupMemberships.AsNoTracking()
                              join familyGroup in dbContext.FamilyGroups.AsNoTracking()
                                  on membership.FamilyGroupId equals familyGroup.Id
                              where membership.AccountId == accountId
                                  && membership.FamilyGroupId == familyGroupId
                                  && membership.IsActive
                              select (int?)familyGroup.CapacityLimit)
            .SingleOrDefaultAsync(cancellationToken);
        if (capacity is null)
        {
            return null;
        }

        return await (from membership in dbContext.FamilyGroupMemberships.AsNoTracking()
                      join profile in dbContext.AdultProfiles.AsNoTracking()
                          on membership.AdultProfileId equals profile.Id
                      where membership.FamilyGroupId == familyGroupId && membership.IsActive
                      orderby profile.Id
                      select new FamilyDirectoryMemberData(profile.Id, profile.DisplayName))
            .Take(capacity.Value)
            .ToListAsync(cancellationToken);
    }
}
