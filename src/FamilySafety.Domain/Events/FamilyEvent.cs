namespace FamilySafety.Domain.Events;

public sealed class FamilyEvent
{
    private FamilyEvent()
    {
    }

    public FamilyEvent(
        Guid id,
        Guid familyGroupId,
        Guid creatorProfileId,
        string name,
        string description,
        DateOnly date,
        TimeOnly time,
        string location)
    {
        if (id == Guid.Empty || familyGroupId == Guid.Empty || creatorProfileId == Guid.Empty)
        {
            throw new ArgumentException("Event identifiers are required.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(location);

        Id = id;
        FamilyGroupId = familyGroupId;
        CreatorProfileId = creatorProfileId;
        Name = name.Trim();
        Description = description.Trim();
        Date = date;
        Time = time;
        Location = location.Trim();
    }

    public Guid Id { get; private set; }

    public Guid FamilyGroupId { get; private set; }

    public Guid CreatorProfileId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public DateOnly Date { get; private set; }

    public TimeOnly Time { get; private set; }

    public string Location { get; private set; } = string.Empty;

    public void UpdateDetails(string name, string description, DateOnly date, TimeOnly time, string location)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(location);

        Name = name.Trim();
        Description = description.Trim();
        Date = date;
        Time = time;
        Location = location.Trim();
    }
}
