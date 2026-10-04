using FamilySafety.Application.Invitations;
using FamilySafety.Application.Profiles;

namespace FamilySafety.Api.Invitations;

public static class MembershipInvitationEndpoints
{
    public static IEndpointRouteBuilder MapMembershipInvitationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api");
        group.MapGet("/family-groups/administered", GetAdministeredAsync);
        group.MapPost("/membership-invitations", IssueAsync);
        group.MapPost("/membership-invitations/revoke", RevokeAsync);
        group.MapPost("/membership-invitations/preview", PreviewAsync);
        group.MapPost("/membership-invitations/accept", AcceptAsync);
        group.MapPost("/membership-invitations/decline", DeclineAsync);
        return endpoints;
    }

    private static async Task<IResult> GetAdministeredAsync(
        IMembershipInvitationService service, CancellationToken cancellationToken)
    {
        try { return Results.Ok(await service.GetAdministeredGroupsAsync(cancellationToken)); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
    }

    private static async Task<IResult> IssueAsync(
        IssueMembershipInvitationRequest request,
        IMembershipInvitationService service,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(() => service.IssueAsync(request, cancellationToken), accepted: true);

    private static async Task<IResult> RevokeAsync(
        RevokeMembershipInvitationRequest request,
        IMembershipInvitationService service,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(() => service.RevokeAsync(request, cancellationToken));

    private static async Task<IResult> PreviewAsync(
        InvitationTokenRequest request,
        IMembershipInvitationService service,
        CancellationToken cancellationToken)
    {
        try { return Results.Ok(await service.PreviewAsync(request, cancellationToken)); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (MembershipInvitationUnavailableException) { return Results.NotFound(); }
    }

    private static async Task<IResult> AcceptAsync(
        InvitationTokenRequest request,
        IMembershipInvitationService service,
        CancellationToken cancellationToken)
    {
        try { return Results.Ok(await service.AcceptAsync(request, cancellationToken)); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (MembershipInvitationUnavailableException) { return Results.NotFound(); }
        catch (MembershipInvitationCapacityException)
        {
            return Results.Conflict(new { message = "The family group has reached its capacity." });
        }
    }

    private static async Task<IResult> DeclineAsync(
        InvitationTokenRequest request,
        IMembershipInvitationService service,
        CancellationToken cancellationToken) =>
        await ExecuteAsync(() => service.DeclineAsync(request, cancellationToken));

    private static async Task<IResult> ExecuteAsync(Func<Task> action, bool accepted = false)
    {
        try
        {
            await action();
            return accepted ? Results.Accepted() : Results.NoContent();
        }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (MembershipInvitationValidationException exception)
        {
            return Results.ValidationProblem(exception.Errors);
        }
        catch (MembershipInvitationUnavailableException) { return Results.NotFound(); }
        catch (MembershipInvitationCapacityException)
        {
            return Results.Conflict(new { message = "The family group has reached its capacity." });
        }
        catch (MembershipInvitationDeliveryUnavailableException)
        {
            return Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}
