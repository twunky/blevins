namespace FamilySafety.Domain.Events;

public enum EventNeedStatus { Open, Claimed, Assigned, Completed }
public enum EventNeedCategory { Food, Supplies, Help, Transport, Other }

public sealed class FamilyEventNeed
{
    private FamilyEventNeed() { }
    public FamilyEventNeed(Guid id, Guid eventId, string title, string? detail, EventNeedCategory? category, DateTimeOffset now)
    {
        if (id == Guid.Empty || eventId == Guid.Empty) throw new ArgumentException("Need identifiers are required.");
        Id = id; EventId = eventId; CreatedAtUtc = now;
        Edit(title, detail, category, now);
    }
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public string Title { get; private set; } = "";
    public string? Detail { get; private set; }
    public EventNeedCategory? Category { get; private set; }
    public EventNeedStatus Status { get; private set; }
    public Guid? ResponsibleProfileId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public Guid Version { get; private set; }

    public void Edit(string title, string? detail, EventNeedCategory? category, DateTimeOffset now)
    {
        Title = title.Trim(); Detail = string.IsNullOrWhiteSpace(detail) ? null : detail.Trim(); Category = category;
        Touch(now);
    }
    public void Take(Guid profileId, bool assigned, DateTimeOffset now)
    {
        if (Status != EventNeedStatus.Open || profileId == Guid.Empty) throw new InvalidOperationException("Only an Open Need can be taken.");
        ResponsibleProfileId = profileId; Status = assigned ? EventNeedStatus.Assigned : EventNeedStatus.Claimed; Touch(now);
    }
    public void Accept(DateTimeOffset now)
    {
        if (Status != EventNeedStatus.Assigned) throw new InvalidOperationException("Only an assignment can be accepted.");
        Status = EventNeedStatus.Claimed; Touch(now);
    }
    public void Release(DateTimeOffset now)
    {
        if (Status is not (EventNeedStatus.Assigned or EventNeedStatus.Claimed)) throw new InvalidOperationException("This Need has no active responsibility.");
        Status = EventNeedStatus.Open; ResponsibleProfileId = null; Touch(now);
    }
    public void Complete(DateTimeOffset now)
    {
        if (Status is not (EventNeedStatus.Assigned or EventNeedStatus.Claimed)) throw new InvalidOperationException("Only a held Need can be completed.");
        Status = EventNeedStatus.Completed; Touch(now);
    }
    private void Touch(DateTimeOffset now) { UpdatedAtUtc = now; Version = Guid.NewGuid(); }
}

// Responsibility-specific provenance, never a general Event activity feed.
public sealed class EventNeedResponsibilityChange
{
    private EventNeedResponsibilityChange() { }
    public EventNeedResponsibilityChange(Guid needId, Guid profileId, EventNeedStatus priorStatus,
        EventNeedStatus status, string reason, DateTimeOffset now)
    {
        Id = Guid.NewGuid(); NeedId = needId; ProfileId = profileId; PriorStatus = priorStatus;
        Status = status; Reason = reason; OccurredAtUtc = now;
    }
    public Guid Id { get; private set; }
    public Guid NeedId { get; private set; }
    public Guid ProfileId { get; private set; }
    public EventNeedStatus PriorStatus { get; private set; }
    public EventNeedStatus Status { get; private set; }
    public string Reason { get; private set; } = "";
    public DateTimeOffset OccurredAtUtc { get; private set; }
}
