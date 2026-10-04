using FamilySafety.Application.Profiles;
using FamilySafety.Application.Relationships;

namespace FamilySafety.Api.Relationships;

public static class FamilyRelationshipEndpoints
{
    public static IEndpointRouteBuilder MapFamilyRelationshipEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/family-groups");
        group.MapGet("/{familyGroupId:guid}/relationships", GetForGroupAsync);
        group.MapPost("/{familyGroupId:guid}/relationship-proposals", ProposeAsync);
        group.MapPost("/relationships/{relationshipId:guid}/accept", AcceptAsync);
        group.MapPost("/relationships/{relationshipId:guid}/decline", DeclineAsync);
        group.MapPost("/relationships/{relationshipId:guid}/withdraw", WithdrawAsync);
        group.MapDelete("/relationships/{relationshipId:guid}", RemoveAsync);
        return endpoints;
    }

    private static async Task<IResult> GetForGroupAsync(
        Guid familyGroupId,
        IFamilyRelationshipService service,
        CancellationToken cancellationToken)
    {
        try
        {
            return Results.Ok(await service.GetForGroupAsync(familyGroupId, cancellationToken));
        }
        catch (CurrentAccountUnavailableException)
        {
            return Results.Unauthorized();
        }
        catch (FamilyRelationshipUnavailableException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> ProposeAsync(
        Guid familyGroupId,
        ProposeFamilyRelationshipRequest request,
        IFamilyRelationshipService service,
        CancellationToken cancellationToken)
    {
        try
        {
            await service.ProposeAsync(familyGroupId, request, cancellationToken);
            return Results.Accepted();
        }
        catch (CurrentAccountUnavailableException)
        {
            return Results.Unauthorized();
        }
        catch (FamilyRelationshipUnavailableException)
        {
            return Results.NotFound();
        }
        catch (FamilyRelationshipConflictException)
        {
            return Results.Conflict();
        }
        catch (FamilyRelationshipValidationException exception)
        {
            return Results.ValidationProblem(exception.Errors.ToDictionary(error => error.Key, error => error.Value));
        }
    }

    private static Task<IResult> AcceptAsync(
        Guid relationshipId,
        IFamilyRelationshipService service,
        CancellationToken cancellationToken) =>
        RespondAsync(relationshipId, FamilyRelationshipAction.Accept, service, cancellationToken);

    private static Task<IResult> DeclineAsync(
        Guid relationshipId,
        IFamilyRelationshipService service,
        CancellationToken cancellationToken) =>
        RespondAsync(relationshipId, FamilyRelationshipAction.Decline, service, cancellationToken);

    private static Task<IResult> WithdrawAsync(
        Guid relationshipId,
        IFamilyRelationshipService service,
        CancellationToken cancellationToken) =>
        RespondAsync(relationshipId, FamilyRelationshipAction.Withdraw, service, cancellationToken);

    private static Task<IResult> RemoveAsync(
        Guid relationshipId,
        IFamilyRelationshipService service,
        CancellationToken cancellationToken) =>
        RespondAsync(relationshipId, FamilyRelationshipAction.Remove, service, cancellationToken);

    private static async Task<IResult> RespondAsync(
        Guid relationshipId,
        FamilyRelationshipAction action,
        IFamilyRelationshipService service,
        CancellationToken cancellationToken)
    {
        try
        {
            await service.RespondAsync(relationshipId, action, cancellationToken);
            return Results.NoContent();
        }
        catch (CurrentAccountUnavailableException)
        {
            return Results.Unauthorized();
        }
        catch (FamilyRelationshipUnavailableException)
        {
            return Results.NotFound();
        }
    }
}
