using FamilySafety.Application.Directory;
using FamilySafety.Application.Profiles;

namespace FamilySafety.Api.Directory;

public static class FamilyDirectoryEndpoints
{
    public static IEndpointRouteBuilder MapFamilyDirectoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/family-groups");
        group.MapGet("/mine", GetActiveGroupsAsync);
        group.MapGet("/{familyGroupId:guid}/members", GetMembersAsync);
        return endpoints;
    }

    private static async Task<IResult> GetActiveGroupsAsync(
        IFamilyDirectoryService service,
        CancellationToken cancellationToken)
    {
        try
        {
            return Results.Ok(await service.GetActiveGroupsAsync(cancellationToken));
        }
        catch (CurrentAccountUnavailableException)
        {
            return Results.Unauthorized();
        }
    }

    private static async Task<IResult> GetMembersAsync(
        Guid familyGroupId,
        string? search,
        IFamilyDirectoryService service,
        CancellationToken cancellationToken)
    {
        try
        {
            return Results.Ok(await service.GetMembersAsync(familyGroupId, search, cancellationToken));
        }
        catch (CurrentAccountUnavailableException)
        {
            return Results.Unauthorized();
        }
        catch (FamilyDirectoryUnavailableException)
        {
            return Results.NotFound();
        }
    }
}
