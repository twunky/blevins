using FamilySafety.Application.Households;
using FamilySafety.Application.Profiles;

namespace FamilySafety.Api.Households;

public static class HouseholdEndpoints
{
    public static IEndpointRouteBuilder MapHouseholdEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/family-groups/{familyId:guid}/households");
        group.AddEndpointFilter(async (context, next) =>
        {
            context.HttpContext.Response.Headers.CacheControl = "no-store";
            return await next(context);
        });
        group.MapGet("", (Guid familyId, HouseholdService s, CancellationToken ct) => Run(() => s.ListAsync(familyId, ct)));
        group.MapPost("", (Guid familyId, CreateHouseholdRequest r, HouseholdService s, CancellationToken ct) => Run(() => s.CreateAsync(familyId, r, ct)));
        group.MapPut("/{householdId:guid}", (Guid familyId, Guid householdId, RenameHouseholdRequest r, HouseholdService s, CancellationToken ct) => Run(() => s.RenameAsync(familyId, householdId, r, ct)));
        group.MapPost("/{householdId:guid}/members", (Guid familyId, Guid householdId, AssignHouseholdMemberRequest r, HouseholdService s, CancellationToken ct) => Run(async () =>
        {
            await s.AssignAsync(familyId, householdId, r, ct);
            return Results.NoContent();
        }));
        group.MapDelete("/{householdId:guid}/members/{profileId:guid}", (Guid familyId, Guid householdId, Guid profileId, Guid version, HouseholdService s, CancellationToken ct) => Run(async () =>
        {
            await s.UnassignAsync(familyId, householdId, profileId, version, ct);
            return Results.NoContent();
        }));
        group.MapDelete("/{householdId:guid}", (Guid familyId, Guid householdId, Guid version, HouseholdService s, CancellationToken ct) => Run(async () =>
        {
            await s.DeleteAsync(familyId, householdId, version, ct);
            return Results.NoContent();
        }));
        return endpoints;
    }

    private static async Task<IResult> Run<T>(Func<Task<T>> operation)
    {
        try { var result = await operation(); return result is IResult response ? response : Results.Ok(result); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (HouseholdUnavailableException) { return Results.NotFound(); }
        catch (HouseholdConflictException) { return Results.Conflict(new { code = "HouseholdChanged" }); }
        catch (HouseholdDuplicateException) { return Results.Conflict(new { code = "DuplicateHouseholdName" }); }
        catch (HouseholdValidationException) { return Results.ValidationProblem(new Dictionary<string, string[]> { ["Name"] = ["Choose a plain-text name of 1-100 characters."] }); }
    }
}
