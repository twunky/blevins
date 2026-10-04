namespace FamilySafety.Domain.Profiles;

public sealed class AdultProfile
{
    private AdultProfile()
    {
    }

    public AdultProfile(Guid id, Guid accountId, string displayName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A profile ID is required.", nameof(id));
        }

        if (accountId == Guid.Empty)
        {
            throw new ArgumentException("An account ID is required.", nameof(accountId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        Id = id;
        AccountId = accountId;
        DisplayName = displayName;
        ContactEmailVisibility = ProfileFieldVisibility.OnlyMe;
        ContactPhoneVisibility = ProfileFieldVisibility.OnlyMe;
        BirthdayVisibility = ProfileFieldVisibility.OnlyMe;
    }

    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public string? ContactEmail { get; private set; }

    public ProfileFieldVisibility ContactEmailVisibility { get; private set; }

    public string? ContactPhone { get; private set; }

    public ProfileFieldVisibility ContactPhoneVisibility { get; private set; }

    public byte? BirthdayMonth { get; private set; }

    public byte? BirthdayDay { get; private set; }

    public ProfileFieldVisibility BirthdayVisibility { get; private set; }

    public void Update(
        string displayName,
        string? contactEmail,
        ProfileFieldVisibility contactEmailVisibility,
        string? contactPhone,
        ProfileFieldVisibility contactPhoneVisibility,
        byte? birthdayMonth,
        byte? birthdayDay,
        ProfileFieldVisibility birthdayVisibility)
    {
        DisplayName = displayName;
        ContactEmail = contactEmail;
        ContactEmailVisibility = contactEmailVisibility;
        ContactPhone = contactPhone;
        ContactPhoneVisibility = contactPhoneVisibility;
        BirthdayMonth = birthdayMonth;
        BirthdayDay = birthdayDay;
        BirthdayVisibility = birthdayVisibility;
    }
}
