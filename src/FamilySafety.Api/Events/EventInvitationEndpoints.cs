using FamilySafety.Application.Events;
using FamilySafety.Application.Profiles;

namespace FamilySafety.Api.Events;

public static class EventInvitationEndpoints
{
    public static IEndpointRouteBuilder MapEventInvitationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/events/{eventId:guid}");
        group.MapGet("/invitations", (Guid eventId, IEventInvitationService service, CancellationToken ct) =>
            ReadAsync(() => service.GetOrganizerAsync(eventId, ct)));
        group.MapPost("/invitations", (Guid eventId, IssueEventInvitationRequest request, IEventInvitationService service, CancellationToken ct) =>
            ReadAsync(() => service.IssueAsync(eventId, request, ct)));
        group.MapPost("/invitations/{invitationId:guid}/revoke", (Guid eventId, Guid invitationId, RevokeEventInvitationRequest request, IEventInvitationService service, CancellationToken ct) =>
            ReadAsync(() => service.RevokeAsync(eventId, invitationId, request, ct)));
        group.MapGet("/invitation/mine", (Guid eventId, IEventInvitationService service, CancellationToken ct) =>
            ReadAsync(() => service.GetOwnAsync(eventId, ct)));
        group.MapPut("/invitation/mine/rsvp", (Guid eventId, UpdateEventRsvpRequest request, IEventInvitationService service, CancellationToken ct) =>
            ReadAsync(() => service.RespondAsync(eventId, request, ct)));
        endpoints.MapGet("/api/family-groups/{familyGroupId:guid}/event-invitations/mine",
            (Guid familyGroupId, IEventInvitationService service, CancellationToken ct) =>
                ReadAsync(() => service.GetPendingAsync(familyGroupId, ct)));
        return endpoints;
    }

    private static async Task<IResult> ReadAsync<T>(Func<Task<T>> operation)
    {
        try { return Results.Ok(await operation()); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (FamilyEventUnavailableException) { return Results.NotFound(); }
        catch (EventInvitationConflictException) { return Results.Conflict(); }
        catch (FamilyEventValidationException exception)
        {
            return Results.ValidationProblem(exception.Errors.ToDictionary(error => error.Key, error => error.Value));
        }
    }
}
