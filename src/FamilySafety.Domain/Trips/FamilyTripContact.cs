namespace FamilySafety.Domain.Trips;

public sealed class FamilyTripContact
{
    private FamilyTripContact() { }
    public FamilyTripContact(Guid id, Guid tripId, string name, string? relationshipRole,
        string? phone, string? email)
    {
        if (id == Guid.Empty || tripId == Guid.Empty) throw new ArgumentException("Contact identifiers are required.");
        Id = id;
        TripId = tripId;
        Update(name, relationshipRole, phone, email);
    }
    public Guid Id { get; private set; }
    public Guid TripId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? RelationshipRole { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }

    public void Update(string name, string? relationshipRole, string? phone, string? email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        RelationshipRole = NormalizeOptional(relationshipRole);
        Phone = NormalizeOptional(phone);
        Email = NormalizeOptional(email);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
