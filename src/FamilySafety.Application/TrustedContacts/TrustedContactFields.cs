using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FamilySafety.Domain.TrustedContacts;

namespace FamilySafety.Application.TrustedContacts;

public sealed record TrustedContactFields(string DisplayName, string? PhoneNumber, string? NormalizedPhone,
    string? EmailAddress, string? NormalizedEmail, string? RelationshipLabel)
{
    public static TrustedContactFields Validate(string? name, string? phone, string? email, string? label)
    {
        var errors = new Dictionary<string, string[]>();
        name = name?.Trim() ?? "";
        phone = Optional(phone); email = Optional(email); label = Optional(label);
        if (name.Length is < 1 or > TrustedContact.NameLimit || name.Any(char.IsControl))
            errors["DisplayName"] = ["Enter a plain-text name of 1 to 100 characters."];
        if (label?.Length > TrustedContact.LabelLimit || label?.Any(char.IsControl) == true)
            errors["RelationshipLabel"] = ["Enter a plain-text label of up to 100 characters."];
        if (email is not null && (email.Length > TrustedContact.EmailLimit || !new EmailAddressAttribute().IsValid(email)))
            errors["EmailAddress"] = ["Enter a valid email address of up to 254 characters."];
        var normalizedPhone = phone is null ? null : NormalizePhone(phone);
        if (phone is not null && (phone.Length > TrustedContact.PhoneLimit || normalizedPhone is null))
            errors["PhoneNumber"] = ["Enter a phone number with 7 to 15 digits, optionally starting with + or 00 and using spaces, parentheses, periods or hyphens (up to 32 characters)."];
        if (phone is null && email is null)
            errors["ContactMethod"] = ["Enter at least one phone number or email address."];
        if (errors.Count > 0) throw new TrustedContactValidationException(errors);
        return new(name, phone, normalizedPhone, email, email?.ToUpperInvariant(), label);
    }

    private static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string? NormalizePhone(string phone)
    {
        if (!Regex.IsMatch(phone, @"^\+?[0-9 ().-]+$")) return null;
        var digits = string.Concat(phone.Where(c => c is >= '0' and <= '9'));
        var international = phone.StartsWith('+') || digits.StartsWith("00", StringComparison.Ordinal);
        if (!phone.StartsWith('+') && international) digits = digits[2..];
        if (digits.Length is < 7 or > 15) return null;
        // Never guess a country for a local number. International + and 00 prefixes are equivalent.
        return international ? "+" + digits : digits;
    }
}
