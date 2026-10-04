using System.ComponentModel.DataAnnotations;
using FamilySafety.Application.Profiles;
using FamilySafety.Domain.Trips;

namespace FamilySafety.Application.Trips;

public sealed class FamilyTripService(
    ICurrentAccountAccessor currentAccountAccessor,
    IFamilyTripRepository repository) : IFamilyTripService
{
    public Task<IReadOnlyList<FamilyTripView>> GetVisibleAsync(CancellationToken cancellationToken) =>
        repository.GetVisibleAsync(RequireAccountId(), cancellationToken);

    public async Task<FamilyTripView> GetAsync(Guid tripId, CancellationToken cancellationToken) =>
        tripId == Guid.Empty
            ? throw new FamilyTripUnavailableException()
            : await repository.GetAsync(RequireAccountId(), tripId, cancellationToken)
                ?? throw new FamilyTripUnavailableException();

    public async Task<FamilyTripView> CreateAsync(Guid familyGroupId,
        CreateFamilyTripRequest request, CancellationToken cancellationToken)
    {
        if (familyGroupId == Guid.Empty) throw new FamilyTripUnavailableException();
        ArgumentNullException.ThrowIfNull(request);
        var errors = ValidateCore(request.Title, request.PrimaryDestination,
            request.StartDate, request.EndDate, request.Description,
            request.TransportationNotes, request.AccommodationNotes,
            request.ExpectedArrivalDate, request.ExpectedArrivalTime,
            request.ExpectedArrivalTimeZone, request.TravelerProfileIds, request.Contacts);

        if (request.AudienceType is null || !Enum.IsDefined(request.AudienceType.Value))
            errors[nameof(request.AudienceType)] = ["Choose who can see this trip."];
        if (request.AudienceProfileIds is null)
            errors[nameof(request.AudienceProfileIds)] = ["Choose a trip audience."];
        else if (request.AudienceProfileIds.Any(id => id == Guid.Empty)
            || request.AudienceProfileIds.Distinct().Count() != request.AudienceProfileIds.Count)
            errors[nameof(request.AudienceProfileIds)] = ["Select each recipient once."];
        else if (request.AudienceType == TripAudienceType.FamilyGroup
            && request.AudienceProfileIds.Count != 0)
            errors[nameof(request.AudienceProfileIds)] = ["A Family Group audience does not accept a people list."];
        else if (request.AudienceType == TripAudienceType.SelectedPeople
            && request.AudienceProfileIds.Count == 0)
            errors[nameof(request.AudienceProfileIds)] = ["Select at least one eligible person."];

        ThrowIfInvalid(errors);
        return await repository.CreateAsync(RequireAccountId(), familyGroupId, request, cancellationToken)
            ?? throw new FamilyTripUnavailableException();
    }

    public async Task<FamilyTripView> UpdateAsync(Guid tripId,
        UpdateFamilyTripRequest request, CancellationToken cancellationToken)
    {
        if (tripId == Guid.Empty) throw new FamilyTripUnavailableException();
        ArgumentNullException.ThrowIfNull(request);
        var errors = ValidateCore(request.Title, request.PrimaryDestination,
            request.StartDate, request.EndDate, request.Description,
            request.TransportationNotes, request.AccommodationNotes,
            request.ExpectedArrivalDate, request.ExpectedArrivalTime,
            request.ExpectedArrivalTimeZone, request.TravelerProfileIds, request.Contacts);
        ThrowIfInvalid(errors);
        return await repository.UpdateAsync(RequireAccountId(), tripId, request, cancellationToken)
            ?? throw new FamilyTripUnavailableException();
    }

    public async Task<TripAudiencePreview> PreviewAudienceReplacementAsync(Guid tripId,
        ReplaceTripAudienceRequest request, CancellationToken cancellationToken)
    {
        ValidateAudienceRequest(request);
        if (tripId == Guid.Empty) throw new FamilyTripUnavailableException();
        return await repository.PreviewAudienceReplacementAsync(
            RequireAccountId(), tripId, request, cancellationToken)
            ?? throw new FamilyTripUnavailableException();
    }

    public async Task<FamilyTripView> ReplaceAudienceAsync(Guid tripId,
        ReplaceTripAudienceRequest request, string previewToken, CancellationToken cancellationToken)
    {
        ValidateAudienceRequest(request);
        if (tripId == Guid.Empty || string.IsNullOrWhiteSpace(previewToken))
            throw new FamilyTripUnavailableException();
        return await repository.ReplaceAudienceAsync(
            RequireAccountId(), tripId, request, previewToken, cancellationToken)
            ?? throw new FamilyTripUnavailableException();
    }

    private static void ValidateAudienceRequest(ReplaceTripAudienceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (request.AudienceType is null || !Enum.IsDefined(request.AudienceType.Value))
            errors[nameof(request.AudienceType)] = ["Choose one audience mode."];
        if (request.AudienceProfileIds is null)
            errors[nameof(request.AudienceProfileIds)] = ["Choose the complete audience."];
        else if (request.AudienceProfileIds.Any(id => id == Guid.Empty)
            || request.AudienceProfileIds.Distinct().Count() != request.AudienceProfileIds.Count)
            errors[nameof(request.AudienceProfileIds)] = ["Select each recipient once."];
        else if (request.AudienceType == TripAudienceType.FamilyGroup
            && request.AudienceProfileIds.Count != 0)
            errors[nameof(request.AudienceProfileIds)] = ["Family Group mode does not accept individual overrides."];
        else if (request.AudienceType == TripAudienceType.SelectedPeople
            && request.AudienceProfileIds.Count == 0)
            errors[nameof(request.AudienceProfileIds)] = ["Select at least one eligible person."];
        ThrowIfInvalid(errors);
    }

    private Guid RequireAccountId() => currentAccountAccessor.Current?.AccountId
        ?? throw new CurrentAccountUnavailableException();

    private static void ThrowIfInvalid(Dictionary<string, string[]> errors)
    {
        if (errors.Count > 0) throw new FamilyTripValidationException(errors);
    }

    private static Dictionary<string, string[]> ValidateCore(
        string? title, string? destination, DateOnly? startDate, DateOnly? endDate,
        string? description, string? transportation, string? accommodation,
        DateOnly? arrivalDate, TimeOnly? arrivalTime, string? arrivalZone,
        IReadOnlyList<Guid>? travelers, IReadOnlyList<TripContactInput>? contacts)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        RequiredText(errors, "Title", title, 100);
        RequiredText(errors, "PrimaryDestination", destination, 200);
        OptionalText(errors, "Description", description, 1000);
        OptionalText(errors, "TransportationNotes", transportation, 1000);
        OptionalText(errors, "AccommodationNotes", accommodation, 1000);
        if (startDate is null || startDate == DateOnly.MinValue)
            errors["StartDate"] = ["Enter a start date."];
        if (endDate is null || endDate == DateOnly.MinValue)
            errors["EndDate"] = ["Enter an end date."];
        else if (startDate.HasValue && endDate < startDate)
            errors["EndDate"] = ["End date cannot precede start date."];

        if (arrivalDate.HasValue || arrivalTime.HasValue || !string.IsNullOrWhiteSpace(arrivalZone))
        {
            if (arrivalDate is null || arrivalDate == DateOnly.MinValue || arrivalTime is null
                || string.IsNullOrWhiteSpace(arrivalZone))
                errors["ExpectedArrivalDate"] = ["Enter expected arrival date, time, and time zone together."];
            else if (arrivalZone.Trim().Length > 128 || !IsTimeZoneValid(arrivalZone.Trim()))
                errors["ExpectedArrivalTimeZone"] = ["Enter a recognized time zone."];
        }

        if (travelers is null || travelers.Count == 0 || travelers.Any(id => id == Guid.Empty)
            || travelers.Distinct().Count() != travelers.Count)
            errors["TravelerProfileIds"] = ["Select at least one distinct eligible traveler."];

        if (contacts is not null)
        {
            for (var index = 0; index < contacts.Count; index++)
            {
                var contact = contacts[index];
                if (contact is null)
                {
                    errors[$"Contacts[{index}]"] = ["Enter contact details."];
                    continue;
                }
                RequiredText(errors, $"Contacts[{index}].Name", contact.Name, 100);
                OptionalText(errors, $"Contacts[{index}].RelationshipRole", contact.RelationshipRole, 100);
                OptionalText(errors, $"Contacts[{index}].Phone", contact.Phone, 32);
                var email = contact.Email?.Trim();
                if (!string.IsNullOrEmpty(email)
                    && (email.Length > 254 || !new EmailAddressAttribute().IsValid(email)))
                    errors[$"Contacts[{index}].Email"] = ["Enter a valid email address of at most 254 characters."];
            }
        }
        return errors;
    }

    private static bool IsTimeZoneValid(string value)
    {
        try { _ = TimeZoneInfo.FindSystemTimeZoneById(value); return true; }
        catch (TimeZoneNotFoundException) { return false; }
        catch (InvalidTimeZoneException) { return false; }
    }

    private static void RequiredText(Dictionary<string, string[]> errors,
        string key, string? value, int maximum)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length is 0 || trimmed.Length > maximum)
            errors[key] = [$"Enter 1 to {maximum} characters."];
    }

    private static void OptionalText(Dictionary<string, string[]> errors,
        string key, string? value, int maximum)
    {
        if (value?.Trim().Length > maximum)
            errors[key] = [$"Enter no more than {maximum} characters."];
    }
}
