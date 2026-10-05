using FamilySafety.Application.CheckIns;
using FamilySafety.Application.Profiles;

namespace FamilySafety.Api.CheckIns;

public static class CheckInEndpoints
{
    public static IEndpointRouteBuilder MapCheckInEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/check-ins");
        group.MapPost("", (CreateCheckInRequest request, ICheckInService service, CancellationToken ct) =>
            ExecuteAsync(() => service.CreateAsync(request, ct)));
        group.MapGet("/mine", (ICheckInService service, CancellationToken ct) => ExecuteAsync(() => service.ListAsync(false, ct)));
        group.MapGet("/received", (ICheckInService service, CancellationToken ct) => ExecuteAsync(() => service.ListAsync(true, ct)));
        group.MapGet("/{checkInId:guid}", (Guid checkInId, ICheckInService service, CancellationToken ct) =>
            ExecuteAsync(() => service.GetAsync(checkInId, ct)));
        return endpoints;
    }
    private static async Task<IResult> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        try { return Results.Ok(await operation()); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (CheckInUnavailableException) { return Results.NotFound(); }
        catch (CheckInValidationException error) { return Results.ValidationProblem(error.Errors); }
    }
}
