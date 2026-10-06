namespace FamilySafety.Domain.Plans;

public sealed class FamilyPlan
{
    private FamilyPlan()
    {
    }

    public FamilyPlan(
        Guid id,
        Guid familyGroupId,
        Guid creatorProfileId,
        string activityOrDestination,
        DateOnly date,
        TimeOnly time,
        DateOnly? expectedReturnDate,
        TimeOnly? expectedReturnTime,
        DateOnly? expectedCheckInDate,
        TimeOnly? expectedCheckInTime,
        PlanAudienceType audienceType,
        string? notes = null)
    {
        if (id == Guid.Empty || familyGroupId == Guid.Empty || creatorProfileId == Guid.Empty)
        {
            throw new ArgumentException("Plan identifiers are required.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(activityOrDestination);
        if (!Enum.IsDefined(audienceType))
        {
            throw new ArgumentOutOfRangeException(nameof(audienceType));
        }

        Id = id;
        FamilyGroupId = familyGroupId;
        CreatorProfileId = creatorProfileId;
        CorrectActivityTiming(activityOrDestination, date, time, expectedReturnDate,
            expectedReturnTime, expectedCheckInDate, expectedCheckInTime);
        AudienceType = audienceType;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes;
    }

    public Guid Id { get; private set; }
    public Guid FamilyGroupId { get; private set; }
    public Guid CreatorProfileId { get; private set; }
    public string ActivityOrDestination { get; private set; } = string.Empty;
    public DateOnly Date { get; private set; }
    public TimeOnly Time { get; private set; }
    public DateOnly? ExpectedReturnDate { get; private set; }
    public TimeOnly? ExpectedReturnTime { get; private set; }
    public DateOnly? ExpectedCheckInDate { get; private set; }
    public TimeOnly? ExpectedCheckInTime { get; private set; }
    public PlanAudienceType AudienceType { get; private set; }
    public string? Notes { get; private set; }
    public Guid Version { get; private set; } = Guid.NewGuid();

    public void CorrectActivityTiming(
        string activityOrDestination, DateOnly date, TimeOnly time,
        DateOnly? expectedReturnDate, TimeOnly? expectedReturnTime,
        DateOnly? expectedCheckInDate, TimeOnly? expectedCheckInTime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(activityOrDestination);
        if (activityOrDestination.Length > 200) throw new ArgumentOutOfRangeException(nameof(activityOrDestination));
        ArgumentOutOfRangeException.ThrowIfEqual(date, DateOnly.MinValue);
        if (expectedReturnDate.HasValue != expectedReturnTime.HasValue)
            throw new ArgumentException("Expected return requires both date and time.");
        if (expectedCheckInDate.HasValue != expectedCheckInTime.HasValue)
            throw new ArgumentException("Expected check-in requires both date and time.");

        ActivityOrDestination = activityOrDestination.Trim();
        Date = date;
        Time = time;
        ExpectedReturnDate = expectedReturnDate;
        ExpectedReturnTime = expectedReturnTime;
        ExpectedCheckInDate = expectedCheckInDate;
        ExpectedCheckInTime = expectedCheckInTime;
        Version = Guid.NewGuid();
    }

    public void UpdateNotes(string? notes)
    {
        if (notes?.Length > 1000)
        {
            throw new ArgumentOutOfRangeException(nameof(notes));
        }

        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes;
    }
}
