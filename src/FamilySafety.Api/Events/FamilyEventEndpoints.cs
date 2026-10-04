using FamilySafety.Application.Events;
using FamilySafety.Application.Profiles;

namespace FamilySafety.Api.Events;

public static class FamilyEventEndpoints
{
    public static IEndpointRouteBuilder MapFamilyEventEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/family-groups/{familyGroupId:guid}/events");
        group.MapGet(string.Empty, GetVisibleAsync);
        group.MapPost(string.Empty, CreateAsync);
        group.MapPut("/{eventId:guid}", UpdateAsync);

        // Item read by ID alone: the server finds and authorizes the Event before its
        // Family is disclosed (REQ-PRIV-027). Missing and unauthorized are both 404.
        endpoints.MapGet("/api/events/{eventId:guid}", GetAsync);
        return endpoints;
    }

    private static async Task<IResult> GetAsync(
        Guid eventId,
        IFamilyEventService service,
        CancellationToken cancellationToken)
    {
        try
        {
            return Results.Ok(await service.GetAsync(eventId, cancellationToken));
        }
        catch (CurrentAccountUnavailableException)
        {
            return Results.Unauthorized();
        }
        catch (FamilyEventUnavailableException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> GetVisibleAsync(
        Guid familyGroupId,
        IFamilyEventService service,
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
        catch (FamilyEventUnavailableException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> CreateAsync(
        Guid familyGroupId,
        CreateFamilyEventRequest request,
        IFamilyEventService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await service.CreateAsync(familyGroupId, request, cancellationToken);
            return Results.Created($"/api/family-groups/{familyGroupId:D}/events", created);
        }
        catch (CurrentAccountUnavailableException)
        {
            return Results.Unauthorized();
        }
        catch (FamilyEventUnavailableException)
        {
            return Results.NotFound();
        }
        catch (FamilyEventValidationException exception)
        {
            return Results.ValidationProblem(exception.Errors.ToDictionary(
                error => error.Key, error => error.Value));
        }
    }

    private static async Task<IResult> UpdateAsync(
        Guid familyGroupId,
        Guid eventId,
        UpdateFamilyEventRequest request,
        IFamilyEventService service,
        CancellationToken cancellationToken)
    {
        try
        {
            return Results.Ok(await service.UpdateAsync(
                familyGroupId, eventId, request, cancellationToken));
        }
        catch (CurrentAccountUnavailableException)
        {
            return Results.Unauthorized();
        }
        catch (FamilyEventUnavailableException)
        {
            return Results.NotFound();
        }
        catch (FamilyEventValidationException exception)
        {
            return Results.ValidationProblem(exception.Errors.ToDictionary(
                error => error.Key, error => error.Value));
        }
    }
}
