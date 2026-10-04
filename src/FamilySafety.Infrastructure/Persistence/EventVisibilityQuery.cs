using FamilySafety.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Infrastructure.Persistence;

// Canonical Event access shared by item/list and participation queries. Invitations
// are deliberately absent: they never expand the Event audience.
internal static class EventVisibilityQuery
{
    internal static IQueryable<FamilyEvent> ForAccount(FamilySafetyDbContext db, Guid accountId) =>
        db.FamilyEvents.AsNoTracking().Where(item => db.FamilyGroupMemberships.Any(member =>
            member.AccountId == accountId && member.IsActive && member.FamilyGroupId == item.FamilyGroupId
            && (member.AdultProfileId == item.CreatorProfileId || db.FamilyEventAudienceMembers.Any(audience =>
                audience.EventId == item.Id && audience.ProfileId == member.AdultProfileId))));
}
