using FamilySafety.Application.Plans;
using FamilySafety.Application.Profiles;

namespace FamilySafety.Api.Plans;

public static class FamilyPlanEndpoints
{
    public static IEndpointRouteBuilder MapFamilyPlanEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/family-groups/{familyGroupId:guid}/plans");
        group.MapGet(string.Empty, GetVisibleAsync);
        group.MapPost(string.Empty, CreateAsync);

        // Item read by ID alone: the server finds and authorizes the Plan before its
        // Family is disclosed (REQ-PRIV-027). Missing and unauthorized are both 404.
        endpoints.MapGet("/api/plans/{planId:guid}", GetAsync);
        return endpoints;
    }

    private static async Task<IResult> GetAsync(
        Guid planId,
        IFamilyPlanService service,
        CancellationToken cancellationToken)
    {
        try
        {
            return Results.Ok(await service.GetAsync(planId, cancellationToken));
        }
        catch (CurrentAccountUnavailableException)
        {
            return Results.Unauthorized();
        }
        catch (FamilyPlanUnavailableException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> GetVisibleAsync(
        Guid familyGroupId,
        IFamilyPlanService service,
        CancellationToken cancellationToken)
    {
        try
        {
            return Results.Ok(await service.GetVisibleAsync(familyGroupId, cancellationToken));
        }
        catch (CurrentAccountUnavailableException)
        {
            return Results.Unauthorized();
        }
        catch (FamilyPlanUnavailableException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> CreateAsync(
        Guid familyGroupId,
        CreateFamilyPlanRequest request,
        IFamilyPlanService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await service.CreateAsync(familyGroupId, request, cancellationToken);
            return Results.Created($"/api/family-groups/{familyGroupId:D}/plans/{created.PlanId:D}", created);
        }
        catch (CurrentAccountUnavailableException)
        {
            return Results.Unauthorized();
        }
        catch (FamilyPlanUnavailableException)
        {
            return Results.NotFound();
        }
        catch (FamilyPlanValidationException exception)
        {
            return Results.ValidationProblem(exception.Errors.ToDictionary(
                error => error.Key, error => error.Value));
        }
    }
}
