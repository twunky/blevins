using FamilySafety.Application.Events;
using FamilySafety.Application.Profiles;

namespace FamilySafety.Api.Events;

public static class EventUpdateEndpoints
{
    public static IEndpointRouteBuilder MapEventUpdateEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/events/{eventId:guid}/updates");
        group.MapGet("", (Guid eventId, IEventUpdateService service, CancellationToken ct) =>
            ExecuteAsync(() => service.GetAsync(eventId, ct)));
        group.MapPost("", (Guid eventId, CreateEventUpdateRequest request, IEventUpdateService service, CancellationToken ct) =>
            ExecuteAsync(() => service.CreateAsync(eventId, request, ct)));
        group.MapPut("/{updateId:guid}", (Guid eventId, Guid updateId, EditEventUpdateRequest request, IEventUpdateService service, CancellationToken ct) =>
            ExecuteAsync(() => service.EditAsync(eventId, updateId, request, ct)));
        group.MapPost("/{updateId:guid}/hide", (Guid eventId, Guid updateId, HideEventUpdateRequest request, IEventUpdateService service, CancellationToken ct) =>
            ExecuteAsync(async () => { await service.HideAsync(eventId, updateId, request.Version, ct); return Results.NoContent(); }));
        return endpoints;
    }
    private static async Task<IResult> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        try { var result = await operation(); return result is IResult response ? response : Results.Ok(result); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (FamilyEventUnavailableException) { return Results.NotFound(); }
        catch (EventUpdateConflictException) { return Results.Conflict(); }
        catch (FamilyEventValidationException error) { return Results.ValidationProblem(error.Errors.ToDictionary(e => e.Key, e => e.Value)); }
    }
}
