using System.ComponentModel.DataAnnotations;
using FamilySafety.Domain.Profiles;

namespace FamilySafety.Application.Profiles;

public sealed class AdultProfileService(
    ICurrentAccountAccessor currentAccountAccessor,
    IAdultProfileRepository repository) : IAdultProfileService
{
    public async Task<AdultProfileOwnerView> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var account = RequireCurrentAccount();
        var profile = await repository.FindByAccountIdAsync(account.AccountId, cancellationToken)
            ?? throw new AdultProfileNotFoundException();

        return ToOwnerView(profile);
    }

    public async Task<AdultProfileOwnerView> UpdateCurrentAsync(
        UpdateAdultProfileRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var account = RequireCurrentAccount();
        var profile = await repository.FindByAccountIdAsync(account.AccountId, cancellationToken)
            ?? throw new AdultProfileNotFoundException();
        var normalized = ValidateAndNormalize(request);

        profile.Update(
            normalized.DisplayName,
            normalized.ContactEmail,
            request.ContactEmailVisibility,
            normalized.ContactPhone,
            request.ContactPhoneVisibility,
            request.BirthdayMonth,
            request.BirthdayDay,
            request.BirthdayVisibility);

        await repository.SaveChangesAsync(cancellationToken);
        return ToOwnerView(profile);
    }

    public async Task<AdultProfileSharedView?> GetSharedAsync(
        Guid profileId,
        CancellationToken cancellationToken)
    {
        if (profileId == Guid.Empty)
        {
            return null;
        }

        var account = RequireCurrentAccount();
        var profile = await repository.FindByIdAsync(profileId, cancellationToken);
        if (profile is null)
        {
            return null;
        }

        if (profile.AccountId == account.AccountId)
        {
            return ToSharedView(profile, includePrivateFields: true);
        }

        var sharesActiveGroup = await repository.AreActiveMembersOfSameGroupAsync(
            account.AccountId,
            profile.AccountId,
            cancellationToken);

        return sharesActiveGroup ? ToSharedView(profile, includePrivateFields: false) : null;
    }

    private CurrentAccount RequireCurrentAccount() =>
        currentAccountAccessor.Current ?? throw new CurrentAccountUnavailableException();

    private static NormalizedAdultProfile ValidateAndNormalize(UpdateAdultProfileRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var displayName = request.DisplayName?.Trim() ?? string.Empty;
        var contactEmail = NormalizeOptional(request.ContactEmail);
        var contactPhone = NormalizeOptional(request.ContactPhone);

        if (displayName.Length is < 1 or > 100)
        {
            errors[nameof(request.DisplayName)] = ["Display name must contain 1 to 100 characters."];
        }

        if (contactEmail?.Length > 254)
        {
            errors[nameof(request.ContactEmail)] = ["Contact email must contain no more than 254 characters."];
        }
        else if (contactEmail is not null && !new EmailAddressAttribute().IsValid(contactEmail))
        {
            errors[nameof(request.ContactEmail)] = ["Contact email must be a valid email address."];
        }

        if (contactPhone?.Length > 32)
        {
            errors[nameof(request.ContactPhone)] = ["Contact phone must contain no more than 32 characters."];
        }

        ValidateVisibility(request.ContactEmailVisibility, nameof(request.ContactEmailVisibility), errors);
        ValidateVisibility(request.ContactPhoneVisibility, nameof(request.ContactPhoneVisibility), errors);
        ValidateVisibility(request.BirthdayVisibility, nameof(request.BirthdayVisibility), errors);

        if (request.BirthdayMonth.HasValue != request.BirthdayDay.HasValue)
        {
            errors[nameof(request.BirthdayMonth)] = ["Birthday month and day must both be provided or both be empty."];
            errors[nameof(request.BirthdayDay)] = ["Birthday month and day must both be provided or both be empty."];
        }
        else if (request.BirthdayMonth.HasValue &&
                 !DateOnly.TryParseExact(
                     $"2000-{request.BirthdayMonth.Value:00}-{request.BirthdayDay!.Value:00}",
                     "yyyy-MM-dd",
                     out _))
        {
            errors[nameof(request.BirthdayDay)] = ["Birthday month and day must form a valid calendar date."];
        }

        if (errors.Count > 0)
        {
            throw new AdultProfileValidationException(errors);
        }

        return new NormalizedAdultProfile(displayName, contactEmail, contactPhone);
    }

    private static void ValidateVisibility(
        ProfileFieldVisibility visibility,
        string field,
        Dictionary<string, string[]> errors)
    {
        if (visibility is not ProfileFieldVisibility.OnlyMe and not ProfileFieldVisibility.ActiveFamilyGroup)
        {
            errors[field] = ["Visibility must be OnlyMe or ActiveFamilyGroup."];
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrEmpty(normalized) ? null : normalized;
    }

    private static AdultProfileOwnerView ToOwnerView(AdultProfile profile) =>
        new(
            profile.Id,
            profile.DisplayName,
            profile.ContactEmail,
            profile.ContactEmailVisibility,
            profile.ContactPhone,
            profile.ContactPhoneVisibility,
            profile.BirthdayMonth,
            profile.BirthdayDay,
            profile.BirthdayVisibility);

    private static AdultProfileSharedView ToSharedView(AdultProfile profile, bool includePrivateFields) =>
        new(
            profile.Id,
            profile.DisplayName,
            Include(profile.ContactEmail, profile.ContactEmailVisibility, includePrivateFields),
            Include(profile.ContactPhone, profile.ContactPhoneVisibility, includePrivateFields),
            Include(profile.BirthdayMonth, profile.BirthdayVisibility, includePrivateFields),
            Include(profile.BirthdayDay, profile.BirthdayVisibility, includePrivateFields));

    private static T? Include<T>(T? value, ProfileFieldVisibility visibility, bool includePrivateFields) =>
        includePrivateFields || visibility == ProfileFieldVisibility.ActiveFamilyGroup ? value : default;

    private sealed record NormalizedAdultProfile(string DisplayName, string? ContactEmail, string? ContactPhone);
}
