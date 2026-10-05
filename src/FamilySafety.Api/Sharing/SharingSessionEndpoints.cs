using FamilySafety.Application.Profiles;
using FamilySafety.Application.Sharing;

namespace FamilySafety.Api.Sharing;

public static class SharingSessionEndpoints
{
    public static IEndpointRouteBuilder MapSharingSessionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/sharing-sessions");
        group.AddEndpointFilter(async (context, next) => {
            var result = await next(context);
            context.HttpContext.Response.Headers.CacheControl = "no-store";
            return result;
        });
        group.MapPost("", (CreateSharingSessionRequest request, SharingSessionService service, CancellationToken ct) =>
            ExecuteAsync(async () => Results.Ok(await service.CreateAsync(request, ct))));
        group.MapGet("/mine", (SharingSessionService service, CancellationToken ct) => ExecuteAsync(async () => Results.Ok(await service.ListAsync(false, ct))));
        group.MapGet("/received", (SharingSessionService service, CancellationToken ct) => ExecuteAsync(async () => Results.Ok(await service.ListAsync(true, ct))));
        group.MapGet("/{id:guid}", (Guid id, SharingSessionService service, CancellationToken ct) => ExecuteAsync(async () => Results.Ok(await service.GetAsync(id, ct))));
        group.MapPost("/{id:guid}/stop", (Guid id, StopSharingSessionRequest request, SharingSessionService service, CancellationToken ct) =>
            ExecuteAsync(async () => { await service.StopAsync(id, request.Version, ct); return Results.NoContent(); }));
        group.MapPut("/{id:guid}/location", (Guid id, UpdateSharingLocationRequest request, SharingSessionService service, CancellationToken ct) =>
            ExecuteAsync(async () => Results.Ok(await service.UpdateLocationAsync(id, request, ct))));
        return endpoints;
    }
    private static async Task<IResult> ExecuteAsync(Func<Task<IResult>> operation)
    {
        try { return await operation(); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (SharingSessionUnavailableException) { return Results.NotFound(); }
        catch (SharingSessionConflictException) { return Results.Conflict(); }
        catch (SharingSessionValidationException error) { return Results.ValidationProblem(error.Errors); }
    }
}
