using FamilySafety.Application.Profiles;

namespace FamilySafety.Api.Profiles;

public static class AdultProfileEndpoints
{
    public static IEndpointRouteBuilder MapAdultProfileEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api");

        group.MapGet("/profile", GetCurrentAsync);
        group.MapPut("/profile", UpdateCurrentAsync);
        group.MapGet("/family-members/{profileId:guid}/profile", GetSharedAsync);

        return endpoints;
    }

    private static async Task<IResult> GetCurrentAsync(
        IAdultProfileService service,
        CancellationToken cancellationToken)
    {
        try
        {
            return Results.Ok(await service.GetCurrentAsync(cancellationToken));
        }
        catch (CurrentAccountUnavailableException)
        {
            return Results.Unauthorized();
        }
        catch (AdultProfileNotFoundException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> UpdateCurrentAsync(
        UpdateAdultProfileRequest request,
        IAdultProfileService service,
        CancellationToken cancellationToken)
    {
        try
        {
            return Results.Ok(await service.UpdateCurrentAsync(request, cancellationToken));
        }
        catch (CurrentAccountUnavailableException)
        {
            return Results.Unauthorized();
        }
        catch (AdultProfileNotFoundException)
        {
            return Results.NotFound();
        }
        catch (AdultProfileValidationException exception)
        {
            return Results.ValidationProblem(exception.Errors);
        }
    }

    private static async Task<IResult> GetSharedAsync(
        Guid profileId,
        IAdultProfileService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var profile = await service.GetSharedAsync(profileId, cancellationToken);
            return profile is null ? Results.NotFound() : Results.Ok(profile);
        }
        catch (CurrentAccountUnavailableException)
        {
            return Results.Unauthorized();
        }
    }
}
