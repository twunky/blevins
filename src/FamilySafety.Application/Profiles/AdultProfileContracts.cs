using FamilySafety.Domain.Profiles;

namespace FamilySafety.Application.Profiles;

public sealed record AdultProfileOwnerView(
    Guid ProfileId,
    string DisplayName,
    string? ContactEmail,
    ProfileFieldVisibility ContactEmailVisibility,
    string? ContactPhone,
    ProfileFieldVisibility ContactPhoneVisibility,
    byte? BirthdayMonth,
    byte? BirthdayDay,
    ProfileFieldVisibility BirthdayVisibility);

public sealed record AdultProfileSharedView(
    Guid ProfileId,
    string DisplayName,
    string? ContactEmail,
    string? ContactPhone,
    byte? BirthdayMonth,
    byte? BirthdayDay);

public sealed record UpdateAdultProfileRequest(
    string? DisplayName,
    string? ContactEmail,
    ProfileFieldVisibility ContactEmailVisibility,
    string? ContactPhone,
    ProfileFieldVisibility ContactPhoneVisibility,
    byte? BirthdayMonth,
    byte? BirthdayDay,
    ProfileFieldVisibility BirthdayVisibility);
