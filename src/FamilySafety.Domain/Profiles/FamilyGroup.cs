namespace FamilySafety.Domain.Profiles;

public sealed class FamilyGroup
{
    private FamilyGroup()
    {
    }

    public FamilyGroup(Guid id, string name, int capacityLimit)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A family group ID is required.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var normalizedName = name.Trim();
        if (normalizedName.Length > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(name), "A family group name cannot exceed 100 characters.");
        }

        if (capacityLimit is < 1 or > 250)
        {
            throw new ArgumentOutOfRangeException(nameof(capacityLimit), "Capacity must be between 1 and 250.");
        }

        Id = id;
        Name = normalizedName;
        CapacityLimit = capacityLimit;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public int CapacityLimit { get; private set; }

    public void Configure(string name, int capacityLimit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var normalizedName = name.Trim();
        if (normalizedName.Length > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(name), "A family group name cannot exceed 100 characters.");
        }

        if (capacityLimit is < 1 or > 250)
        {
            throw new ArgumentOutOfRangeException(nameof(capacityLimit), "Capacity must be between 1 and 250.");
        }

        Name = normalizedName;
        CapacityLimit = capacityLimit;
    }
}
