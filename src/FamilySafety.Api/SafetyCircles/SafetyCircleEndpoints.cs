using FamilySafety.Application.Profiles;
using FamilySafety.Application.SafetyCircles;
namespace FamilySafety.Api.SafetyCircles;

public static class SafetyCircleEndpoints
{
    public static IEndpointRouteBuilder MapSafetyCircleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/family-groups/{familyId:guid}/safety-circles");
        group.AddEndpointFilter(async (context, next) => { context.HttpContext.Response.Headers.CacheControl = "no-store"; return await next(context); });
        group.MapGet("", (Guid familyId, SafetyCircleService s, CancellationToken ct) => Run(() => s.ListAsync(familyId, ct)));
        group.MapPost("", (Guid familyId, CreateSafetyCircleRequest r, SafetyCircleService s, CancellationToken ct) => Run(() => s.CreateAsync(familyId, r, ct)));
        group.MapGet("/{circleId:guid}", (Guid familyId, Guid circleId, SafetyCircleService s, CancellationToken ct) => Run(() => s.ReadAsync(familyId, circleId, ct)));
        group.MapPut("/{circleId:guid}", (Guid familyId, Guid circleId, RenameSafetyCircleRequest r, SafetyCircleService s, CancellationToken ct) => Run(() => s.RenameAsync(familyId, circleId, r, ct)));
        group.MapPost("/{circleId:guid}/members", (Guid familyId, Guid circleId, AddSafetyCircleMemberRequest r, SafetyCircleService s, CancellationToken ct) => Run(() => s.AddAsync(familyId, circleId, r, ct)));
        group.MapDelete("/{circleId:guid}/members/{profileId:guid}", (Guid familyId, Guid circleId, Guid profileId, Guid version, SafetyCircleService s, CancellationToken ct) => Run(() => s.RemoveAsync(familyId, circleId, profileId, version, ct)));
        group.MapDelete("/{circleId:guid}", (Guid familyId, Guid circleId, Guid version, SafetyCircleService s, CancellationToken ct) => Run(async () => { await s.DeleteAsync(familyId, circleId, version, ct); return Results.NoContent(); }));
        return endpoints;
    }
    private static async Task<IResult> Run<T>(Func<Task<T>> operation)
    {
        try { var result = await operation(); return result is IResult response ? response : Results.Ok(result); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (SafetyCircleUnavailableException) { return Results.NotFound(); }
        catch (SafetyCircleConflictException) { return Results.Conflict(new { code = "CircleChanged" }); }
        catch (SafetyCircleDuplicateException) { return Results.Conflict(new { code = "DuplicateCircleName" }); }
        catch (SafetyCircleValidationException) { return Results.ValidationProblem(new Dictionary<string, string[]> { ["Name"] = ["Choose a plain-text name of 1-100 characters."] }); }
    }
}
