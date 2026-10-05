namespace FamilySafety.Domain.TrustedContacts;

/// <summary>Owner-entered information only. There is deliberately no contact identity or Family link.</summary>
public sealed class TrustedContact
{
    public const int NameLimit = 100;
    public const int PhoneLimit = 32;
    public const int EmailLimit = 254;
    public const int LabelLimit = 100;

    private TrustedContact() { }

    public TrustedContact(Guid id, Guid ownerProfileId, string displayName, string? phoneNumber,
        string? normalizedPhone, string? emailAddress, string? normalizedEmail, string? relationshipLabel)
    {
        if (id == Guid.Empty || ownerProfileId == Guid.Empty) throw new ArgumentException("Contact and owner IDs are required.");
        Id = id;
        OwnerProfileId = ownerProfileId;
        Edit(displayName, phoneNumber, normalizedPhone, emailAddress, normalizedEmail, relationshipLabel);
    }

    public Guid Id { get; private set; }
    public Guid OwnerProfileId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public string? NormalizedPhone { get; private set; }
    public string? EmailAddress { get; private set; }
    public string? NormalizedEmail { get; private set; }
    public string? RelationshipLabel { get; private set; }
    public Guid Version { get; private set; }

    public void Edit(string displayName, string? phoneNumber, string? normalizedPhone,
        string? emailAddress, string? normalizedEmail, string? relationshipLabel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        if (displayName.Length > NameLimit || phoneNumber?.Length > PhoneLimit || emailAddress?.Length > EmailLimit
            || relationshipLabel?.Length > LabelLimit || (normalizedPhone is null && normalizedEmail is null))
            throw new ArgumentException("Valid name and contact information are required.");
        DisplayName = displayName;
        PhoneNumber = phoneNumber;
        NormalizedPhone = normalizedPhone;
        EmailAddress = emailAddress;
        NormalizedEmail = normalizedEmail;
        RelationshipLabel = relationshipLabel;
        Version = Guid.NewGuid();
    }
}
