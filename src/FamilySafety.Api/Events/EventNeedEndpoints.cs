using FamilySafety.Application.Events;
using FamilySafety.Application.Profiles;

namespace FamilySafety.Api.Events;

public static class EventNeedEndpoints
{
    public static IEndpointRouteBuilder MapEventNeedEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/events/{eventId:guid}/needs");
        group.MapGet("", (Guid eventId, IEventNeedService service, CancellationToken ct) =>
            ExecuteAsync(() => service.GetAsync(eventId, ct)));
        group.MapPost("", (Guid eventId, CreateEventNeedRequest request, IEventNeedService service, CancellationToken ct) =>
            ExecuteAsync(() => service.CreateAsync(eventId, request, ct)));
        group.MapPut("/{needId:guid}", (Guid eventId, Guid needId, EditEventNeedRequest request, IEventNeedService service, CancellationToken ct) =>
            ExecuteAsync(() => service.EditAsync(eventId, needId, request, ct)));
        group.MapPost("/{needId:guid}/remove", (Guid eventId, Guid needId, EventNeedActionRequest request, IEventNeedService service, CancellationToken ct) =>
            ExecuteAsync(async () => { await service.RemoveAsync(eventId, needId, request.Version, ct); return Results.NoContent(); }));
        group.MapPost("/{needId:guid}/assign", (Guid eventId, Guid needId, AssignEventNeedRequest request, IEventNeedService service, CancellationToken ct) =>
            ExecuteAsync(() => service.ActAsync(eventId, needId, request.Version, "assign", request.ProfileId, ct)));
        foreach (var action in new[] { "claim", "release", "accept", "decline", "complete" })
            group.MapPost($"/{{needId:guid}}/{action}", (Guid eventId, Guid needId, EventNeedActionRequest request, IEventNeedService service, CancellationToken ct) =>
                ExecuteAsync(() => service.ActAsync(eventId, needId, request.Version, action, null, ct)));
        return endpoints;
    }
    private static async Task<IResult> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        try { var result = await operation(); return result is IResult response ? response : Results.Ok(result); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (FamilyEventUnavailableException) { return Results.NotFound(); }
        catch (EventNeedConflictException) { return Results.Conflict(); }
        catch (FamilyEventValidationException error) { return Results.ValidationProblem(error.Errors.ToDictionary(e => e.Key, e => e.Value)); }
    }
}
