using System.Data;
using System.Security.Cryptography;
using System.Text;
using FamilySafety.Application.Trips;
using FamilySafety.Domain.Trips;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Infrastructure.Persistence;

public sealed class EfFamilyTripRepository(FamilySafetyDbContext dbContext) : IFamilyTripRepository
{
    public async Task<IReadOnlyList<FamilyTripView>> GetVisibleAsync(
        Guid accountId, CancellationToken cancellationToken)
    {
        var profileId = await GetProfileIdAsync(accountId, cancellationToken);
        if (profileId is null) return [];

        var tripIds = await dbContext.FamilyTrips.AsNoTracking()
            .Where(trip => trip.CreatorProfileId == profileId.Value
                || dbContext.FamilyTripAudienceMembers.Any(recipient =>
                    recipient.TripId == trip.Id && recipient.ProfileId == profileId.Value
                    && dbContext.FamilyGroupMemberships.Any(membership =>
                        membership.AdultProfileId == recipient.ProfileId
                        && membership.FamilyGroupId == trip.FamilyGroupId
                        && membership.IsActive
                        && membership.AccessVersion == recipient.MembershipAccessVersion)))
            .OrderBy(trip => trip.StartDate).ThenBy(trip => trip.Title)
            .Select(trip => trip.Id)
            .ToListAsync(cancellationToken);

        var views = new List<FamilyTripView>(tripIds.Count);
        foreach (var tripId in tripIds)
        {
            var view = await GetViewAsync(profileId.Value, tripId, cancellationToken);
            if (view is not null) views.Add(view);
        }
        return views;
    }

    public async Task<FamilyTripView?> GetAsync(
        Guid accountId, Guid tripId, CancellationToken cancellationToken)
    {
        var profileId = await GetProfileIdAsync(accountId, cancellationToken);
        return profileId is null ? null : await GetViewAsync(profileId.Value, tripId, cancellationToken);
    }

    public async Task<FamilyTripView?> CreateAsync(Guid accountId, Guid familyGroupId,
        CreateFamilyTripRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        if (!await LockFamilyGroupAsync(familyGroupId, cancellationToken)) return null;

        var creator = await dbContext.FamilyGroupMemberships.AsNoTracking()
            .Where(membership => membership.AccountId == accountId
                && membership.FamilyGroupId == familyGroupId && membership.IsActive)
            .Select(membership => (Guid?)membership.AdultProfileId)
            .SingleOrDefaultAsync(cancellationToken);
        if (creator is null) return null;

        var eligible = await GetEligibleMembersAsync(familyGroupId, cancellationToken);
        var travelers = request.TravelerProfileIds!;
        if (!AllEligible(travelers, eligible)) return null;

        IReadOnlyList<Guid> recipients;
        if (request.AudienceType == TripAudienceType.FamilyGroup)
            recipients = eligible.Keys.ToArray();
        else if (request.AudienceType == TripAudienceType.SelectedPeople
            && AllEligible(request.AudienceProfileIds!, eligible))
        {
            if (request.AudienceProfileIds!.All(id => id == creator.Value))
                throw new FamilyTripValidationException(new Dictionary<string, string[]>
                {
                    [nameof(request.AudienceProfileIds)] = ["Select at least one person other than the creator."],
                });
            recipients = request.AudienceProfileIds!;
        }
        else return null;

        var tripId = Guid.NewGuid();
        dbContext.FamilyTrips.Add(new FamilyTrip(tripId, familyGroupId, creator.Value,
            request.AudienceType!.Value, request.Title!, request.PrimaryDestination!,
            request.StartDate!.Value, request.EndDate!.Value, request.Description,
            request.TransportationNotes, request.AccommodationNotes,
            request.ExpectedArrivalDate, request.ExpectedArrivalTime,
            request.ExpectedArrivalTimeZone));
        dbContext.FamilyTripTravelers.AddRange(travelers.Select(id => new FamilyTripTraveler(tripId, id)));
        dbContext.FamilyTripAudienceMembers.AddRange(recipients.Select(id =>
            new FamilyTripAudienceMember(tripId, id, eligible[id])));
        dbContext.FamilyTripContacts.AddRange((request.Contacts ?? []).Select(contact =>
            new FamilyTripContact(Guid.NewGuid(), tripId, contact.Name!,
                contact.RelationshipRole, contact.Phone, contact.Email)));

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await GetViewAsync(creator.Value, tripId, cancellationToken);
    }

    public async Task<FamilyTripView?> UpdateAsync(Guid accountId, Guid tripId,
        UpdateFamilyTripRequest request, CancellationToken cancellationToken)
    {
        var profileId = await GetProfileIdAsync(accountId, cancellationToken);
        if (profileId is null) return null;
        var groupId = await dbContext.FamilyTrips.AsNoTracking()
            .Where(trip => trip.Id == tripId && trip.CreatorProfileId == profileId.Value)
            .Select(trip => (Guid?)trip.FamilyGroupId)
            .SingleOrDefaultAsync(cancellationToken);
        if (groupId is null) return null;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        if (!await LockFamilyGroupAsync(groupId.Value, cancellationToken)) return null;
        var trip = await dbContext.FamilyTrips.SingleOrDefaultAsync(
            item => item.Id == tripId && item.CreatorProfileId == profileId.Value,
            cancellationToken);
        if (trip is null) return null;

        var eligible = await GetEligibleMembersAsync(groupId.Value, cancellationToken);
        var existingTravelers = await dbContext.FamilyTripTravelers
            .Where(traveler => traveler.TripId == tripId).ToListAsync(cancellationToken);
        var existingIds = existingTravelers.Select(traveler => traveler.ProfileId).ToHashSet();
        // A traveler was eligible when added. Ending membership must not make unrelated
        // creator edits impossible or silently remove that historical traveler.
        if (request.TravelerProfileIds!.Count == 0
            || request.TravelerProfileIds.Distinct().Count() != request.TravelerProfileIds.Count
            || request.TravelerProfileIds.Any(id => !existingIds.Contains(id) && !eligible.ContainsKey(id)))
            return null;

        trip.Update(request.Title!, request.PrimaryDestination!, request.StartDate!.Value,
            request.EndDate!.Value, request.Description, request.TransportationNotes,
            request.AccommodationNotes, request.ExpectedArrivalDate,
            request.ExpectedArrivalTime, request.ExpectedArrivalTimeZone);

        var requestedIds = request.TravelerProfileIds.ToHashSet();
        dbContext.FamilyTripTravelers.RemoveRange(existingTravelers.Where(
            traveler => !requestedIds.Contains(traveler.ProfileId)));
        dbContext.FamilyTripContacts.RemoveRange(await dbContext.FamilyTripContacts
            .Where(contact => contact.TripId == tripId).ToListAsync(cancellationToken));
        dbContext.FamilyTripTravelers.AddRange(request.TravelerProfileIds.Where(id => !existingIds.Contains(id)).Select(id =>
            new FamilyTripTraveler(tripId, id)));
        dbContext.FamilyTripContacts.AddRange((request.Contacts ?? []).Select(contact =>
            new FamilyTripContact(Guid.NewGuid(), tripId, contact.Name!,
                contact.RelationshipRole, contact.Phone, contact.Email)));

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await GetViewAsync(profileId.Value, tripId, cancellationToken);
    }

    public async Task<TripAudiencePreview?> PreviewAudienceReplacementAsync(Guid accountId, Guid tripId,
        ReplaceTripAudienceRequest request, CancellationToken cancellationToken)
    {
        var profileId = await GetProfileIdAsync(accountId, cancellationToken);
        if (profileId is null) return null;
        var groupId = await dbContext.FamilyTrips.AsNoTracking()
            .Where(trip => trip.Id == tripId && trip.CreatorProfileId == profileId.Value)
            .Select(trip => (Guid?)trip.FamilyGroupId)
            .SingleOrDefaultAsync(cancellationToken);
        if (groupId is null) return null;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        if (!await LockFamilyGroupAsync(groupId.Value, cancellationToken)) return null;
        var state = await LoadAudienceReplacementStateAsync(profileId.Value, tripId, cancellationToken);
        if (state is null) return null;
        var outcome = await BuildAudienceOutcome(state, request, profileId.Value, cancellationToken);
        return outcome.Preview;
    }

    public async Task<FamilyTripView?> ReplaceAudienceAsync(Guid accountId, Guid tripId,
        ReplaceTripAudienceRequest request, string previewToken, CancellationToken cancellationToken)
    {
        var profileId = await GetProfileIdAsync(accountId, cancellationToken);
        if (profileId is null) return null;
        var groupId = await dbContext.FamilyTrips.AsNoTracking()
            .Where(trip => trip.Id == tripId && trip.CreatorProfileId == profileId.Value)
            .Select(trip => (Guid?)trip.FamilyGroupId)
            .SingleOrDefaultAsync(cancellationToken);
        if (groupId is null) return null;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        if (!await LockFamilyGroupAsync(groupId.Value, cancellationToken)) return null;
        var state = await LoadAudienceReplacementStateAsync(profileId.Value, tripId, cancellationToken);
        if (state is null) return null;
        AudienceReplacementOutcome outcome;
        try { outcome = await BuildAudienceOutcome(state, request, profileId.Value, cancellationToken); }
        catch (FamilyTripValidationException) { throw new FamilyTripAudienceConflictException(); }
        if (!CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(outcome.Preview.PreviewToken), Encoding.UTF8.GetBytes(previewToken)))
            throw new FamilyTripAudienceConflictException();

        var current = await dbContext.FamilyTripAudienceMembers
            .Where(recipient => recipient.TripId == tripId).ToListAsync(cancellationToken);
        var desired = outcome.Proposed.ToDictionary(pair => pair.Key, pair => pair.Value);
        dbContext.FamilyTripAudienceMembers.RemoveRange(current.Where(recipient =>
            !desired.ContainsKey(recipient.ProfileId)));
        foreach (var recipient in current.Where(recipient => desired.ContainsKey(recipient.ProfileId)))
        {
            var desiredVersion = desired[recipient.ProfileId];
            if (recipient.MembershipAccessVersion != desiredVersion)
                recipient.RefreshMembershipAccessVersion(desiredVersion);
            desired.Remove(recipient.ProfileId);
        }
        dbContext.FamilyTripAudienceMembers.AddRange(desired.Select(pair =>
            new FamilyTripAudienceMember(tripId, pair.Key, pair.Value)));
        var trip = await dbContext.FamilyTrips.SingleOrDefaultAsync(
            item => item.Id == tripId && item.CreatorProfileId == profileId.Value,
            cancellationToken);
        if (trip is null) return null;
        trip.ReplaceAudience(request.AudienceType!.Value);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await GetViewAsync(profileId.Value, tripId, cancellationToken);
    }

    private async Task<AudienceReplacementState?> LoadAudienceReplacementStateAsync(
        Guid creatorProfileId, Guid tripId, CancellationToken cancellationToken)
    {
        var trip = await dbContext.FamilyTrips.AsNoTracking().SingleOrDefaultAsync(
            item => item.Id == tripId && item.CreatorProfileId == creatorProfileId,
            cancellationToken);
        if (trip is null) return null;
        var saved = await dbContext.FamilyTripAudienceMembers.AsNoTracking()
            .Where(recipient => recipient.TripId == tripId)
            .ToDictionaryAsync(recipient => recipient.ProfileId,
                recipient => recipient.MembershipAccessVersion, cancellationToken);
        var eligible = await GetEligibleMembersAsync(trip.FamilyGroupId, cancellationToken);
        return new AudienceReplacementState(trip, saved, eligible);
    }

    private async Task<AudienceReplacementOutcome> BuildAudienceOutcome(
        AudienceReplacementState state, ReplaceTripAudienceRequest request,
        Guid creatorProfileId, CancellationToken cancellationToken)
    {
        if (request.AudienceType is null || !Enum.IsDefined(request.AudienceType.Value)
            || request.AudienceProfileIds is null)
            throw AudienceInvalid("Choose one complete audience.");
        var input = request.AudienceProfileIds;
        if (input.Any(id => id == Guid.Empty) || input.Distinct().Count() != input.Count)
            throw AudienceInvalid("Select each recipient once.");
        var groupActive = state.Eligible;
        var creatorEligible = groupActive.ContainsKey(creatorProfileId);
        Dictionary<Guid, int> proposed;
        if (request.AudienceType == TripAudienceType.FamilyGroup)
        {
            if (input.Count != 0) throw AudienceInvalid("Family Group mode has no individual overrides.");
            if (!creatorEligible) throw AudienceInvalid("A departed creator cannot refresh the Family Group audience.");
            proposed = groupActive;
        }
        else if (request.AudienceType == TripAudienceType.SelectedPeople)
        {
            if (input.Count == 0 || !input.Any(id => id != creatorProfileId))
                throw AudienceInvalid("Select at least one eligible person other than the creator.");
            if (input.Any(id => !groupActive.ContainsKey(id)))
                throw AudienceInvalid("Every recipient must be currently eligible in this Family Group.");
            proposed = input.ToDictionary(id => id, id => groupActive[id]);
            if (!creatorEligible)
            {
                var currentAccessible = state.Saved.Where(pair =>
                    groupActive.TryGetValue(pair.Key, out var version) && version == pair.Value)
                    .Select(pair => pair.Key).ToHashSet();
                if (input.Any(id => !currentAccessible.Contains(id)))
                    throw AudienceInvalid("A departed creator can only keep currently accessible saved recipients.");
            }
        }
        else throw AudienceInvalid("Choose one supported audience mode.");

        var currentAccess = state.Saved.Where(pair => pair.Key != creatorProfileId
                && groupActive.TryGetValue(pair.Key, out var version) && version == pair.Value)
            .Select(pair => pair.Key).ToHashSet();
        var proposedAccess = proposed.Keys.Where(id => id != creatorProfileId).ToHashSet();
        var gainingIds = proposedAccess.Except(currentAccess).Order().ToArray();
        var losingIds = currentAccess.Except(proposedAccess).Order().ToArray();
        var resultIds = proposed.Keys.Order().ToArray();

        // Only identities in the confirmed effective result are projected. For a departed
        // creator this set is bounded by the saved audience and cannot expose the directory.
        var visibleIds = resultIds.Concat(gainingIds).Concat(losingIds).Distinct().ToArray();
        var people = await dbContext.AdultProfiles.AsNoTracking()
            .Where(profile => visibleIds.Contains(profile.Id))
            .OrderBy(profile => profile.DisplayName).ThenBy(profile => profile.Id)
            .Select(profile => new TripPersonView(profile.Id, profile.DisplayName))
            .ToListAsync(cancellationToken);
        var byId = people.ToDictionary(person => person.ProfileId);
        var resultPeople = resultIds.Where(byId.ContainsKey).Select(id => byId[id]).ToArray();
        var gaining = gainingIds.Where(byId.ContainsKey).Select(id => byId[id]).ToArray();
        var losingPeople = losingIds.Where(byId.ContainsKey).Select(id => byId[id]).ToArray();
        var canonical = string.Join("|", state.Trip.AudienceType,
            string.Join(",", state.Saved.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key:N}:{pair.Value}")),
            creatorEligible ? "eligible" : "departed", request.AudienceType,
            string.Join(",", proposed.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key:N}:{pair.Value}")),
            string.Join(",", gainingIds.Select(id => id.ToString("N"))),
            string.Join(",", losingIds.Select(id => id.ToString("N"))));
        var token = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
        return new AudienceReplacementOutcome(proposed,
            new TripAudiencePreview(token, request.AudienceType.Value, resultPeople, gaining, losingPeople));
    }

    private static FamilyTripValidationException AudienceInvalid(string message) =>
        new(new Dictionary<string, string[]> { ["AudienceProfileIds"] = [message] });

    private sealed record AudienceReplacementState(
        FamilyTrip Trip, Dictionary<Guid, int> Saved, Dictionary<Guid, int> Eligible);
    private sealed record AudienceReplacementOutcome(
        Dictionary<Guid, int> Proposed, TripAudiencePreview Preview);

    private async Task<FamilyTripView?> GetViewAsync(
        Guid profileId, Guid tripId, CancellationToken cancellationToken)
    {
        var row = await (from familyTrip in dbContext.FamilyTrips.AsNoTracking()
                         join familyGroup in dbContext.FamilyGroups.AsNoTracking()
                             on familyTrip.FamilyGroupId equals familyGroup.Id
                         join creator in dbContext.AdultProfiles.AsNoTracking()
                             on familyTrip.CreatorProfileId equals creator.Id
                         where familyTrip.Id == tripId
                             && (familyTrip.CreatorProfileId == profileId
                                 || dbContext.FamilyTripAudienceMembers.Any(recipient =>
                                     recipient.TripId == familyTrip.Id && recipient.ProfileId == profileId
                                     && dbContext.FamilyGroupMemberships.Any(membership =>
                                         membership.AdultProfileId == recipient.ProfileId
                                         && membership.FamilyGroupId == familyTrip.FamilyGroupId
                                         && membership.IsActive
                                         && membership.AccessVersion == recipient.MembershipAccessVersion)))
                         select new { Trip = familyTrip, GroupName = familyGroup.Name, CreatorName = creator.DisplayName })
            .SingleOrDefaultAsync(cancellationToken);
        if (row is null) return null;

        var trip = row.Trip;
        var canEdit = trip.CreatorProfileId == profileId;
        var travelers = await (from traveler in dbContext.FamilyTripTravelers.AsNoTracking()
                               join profile in dbContext.AdultProfiles.AsNoTracking()
                                   on traveler.ProfileId equals profile.Id
                               where traveler.TripId == tripId
                               orderby profile.DisplayName, profile.Id
                               select new TripPersonView(profile.Id, profile.DisplayName))
            .ToListAsync(cancellationToken);
        var contacts = await dbContext.FamilyTripContacts.AsNoTracking()
            .Where(contact => contact.TripId == tripId)
            .OrderBy(contact => contact.Name)
            .Select(contact => new TripContactView(contact.Id, contact.Name,
                contact.RelationshipRole, contact.Phone, contact.Email))
            .ToListAsync(cancellationToken);

        IReadOnlyList<TripPersonView> audience = [];
        if (canEdit)
        {
            audience = await (from recipient in dbContext.FamilyTripAudienceMembers.AsNoTracking()
                              join membership in dbContext.FamilyGroupMemberships.AsNoTracking()
                                  on recipient.ProfileId equals membership.AdultProfileId
                              join profile in dbContext.AdultProfiles.AsNoTracking()
                                  on recipient.ProfileId equals profile.Id
                              where recipient.TripId == tripId
                                  && membership.FamilyGroupId == trip.FamilyGroupId
                                  && membership.IsActive
                                  && membership.AccessVersion == recipient.MembershipAccessVersion
                              orderby profile.DisplayName, profile.Id
                              select new TripPersonView(profile.Id, profile.DisplayName))
                .ToListAsync(cancellationToken);
        }

        return new FamilyTripView(trip.Id, trip.FamilyGroupId, row.GroupName,
            trip.CreatorProfileId, row.CreatorName, canEdit,
            trip.Title, trip.PrimaryDestination, trip.StartDate, trip.EndDate,
            trip.Description, trip.TransportationNotes, trip.AccommodationNotes,
            trip.ExpectedArrivalDate, trip.ExpectedArrivalTime, trip.ExpectedArrivalTimeZone,
            trip.AudienceType, travelers, contacts, audience);
    }

    private Task<Guid?> GetProfileIdAsync(Guid accountId, CancellationToken cancellationToken) =>
        dbContext.AdultProfiles.AsNoTracking()
            .Where(profile => profile.AccountId == accountId)
            .Select(profile => (Guid?)profile.Id)
            .SingleOrDefaultAsync(cancellationToken);

    private Task<Dictionary<Guid, int>> GetEligibleMembersAsync(
        Guid familyGroupId, CancellationToken cancellationToken) =>
        dbContext.FamilyGroupMemberships.AsNoTracking()
            .Where(membership => membership.FamilyGroupId == familyGroupId && membership.IsActive)
            .ToDictionaryAsync(membership => membership.AdultProfileId,
                membership => membership.AccessVersion, cancellationToken);

    private static bool AllEligible(IReadOnlyList<Guid> ids, IReadOnlyDictionary<Guid, int> eligible) =>
        ids.Count > 0 && ids.Distinct().Count() == ids.Count && ids.All(eligible.ContainsKey);

    private async Task<bool> LockFamilyGroupAsync(Guid familyGroupId, CancellationToken cancellationToken) =>
        await dbContext.FamilyGroups.Where(group => group.Id == familyGroupId)
            .ExecuteUpdateAsync(updates => updates.SetProperty(
                group => group.CapacityLimit, group => group.CapacityLimit), cancellationToken) == 1;
}
