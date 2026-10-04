using FamilySafety.Application.Profiles;
using FamilySafety.Domain.Profiles;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Infrastructure.Persistence;

public sealed class EfAdultProfileRepository(FamilySafetyDbContext dbContext) : IAdultProfileRepository
{
    public Task<AdultProfile?> FindByAccountIdAsync(Guid accountId, CancellationToken cancellationToken) =>
        dbContext.AdultProfiles.SingleOrDefaultAsync(
            profile => profile.AccountId == accountId,
            cancellationToken);

    public Task<AdultProfile?> FindByIdAsync(Guid profileId, CancellationToken cancellationToken) =>
        dbContext.AdultProfiles.SingleOrDefaultAsync(profile => profile.Id == profileId, cancellationToken);

    public Task<bool> AreActiveMembersOfSameGroupAsync(
        Guid requestingAccountId,
        Guid targetAccountId,
        CancellationToken cancellationToken)
    {
        var requestingGroups = dbContext.FamilyGroupMemberships
            .Where(membership => membership.AccountId == requestingAccountId && membership.IsActive)
            .Select(membership => membership.FamilyGroupId);

        return dbContext.FamilyGroupMemberships.AnyAsync(
            membership =>
                membership.AccountId == targetAccountId &&
                membership.IsActive &&
                requestingGroups.Contains(membership.FamilyGroupId),
            cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
