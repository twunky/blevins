namespace FamilySafety.Domain.Events;

public enum EventRsvpStatus { Pending, Going, Maybe, NotGoing }

public sealed class FamilyEventInvitation
{
    private FamilyEventInvitation() { }

    public FamilyEventInvitation(Guid id, Guid eventId, Guid inviteeProfileId, DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty || eventId == Guid.Empty || inviteeProfileId == Guid.Empty)
            throw new ArgumentException("Invitation identifiers are required.");
        Id = id;
        EventId = eventId;
        InviteeProfileId = inviteeProfileId;
        CreatedAtUtc = createdAtUtc;
        Version = Guid.NewGuid();
    }

    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public Guid InviteeProfileId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? RevokedAtUtc { get; private set; }
    public EventRsvpStatus RsvpStatus { get; private set; }
    public DateTimeOffset? RsvpUpdatedAtUtc { get; private set; }
    public Guid Version { get; private set; }

    public void Respond(EventRsvpStatus status, DateTimeOffset now)
    {
        if (RevokedAtUtc is not null) throw new InvalidOperationException("The invitation is inactive.");
        if (!Enum.IsDefined(status)) throw new ArgumentOutOfRangeException(nameof(status));
        RsvpStatus = status;
        RsvpUpdatedAtUtc = now;
        Version = Guid.NewGuid();
    }

    public void Revoke(DateTimeOffset now)
    {
        if (RevokedAtUtc is not null) throw new InvalidOperationException("The invitation is inactive.");
        RevokedAtUtc = now;
        Version = Guid.NewGuid();
    }
}
