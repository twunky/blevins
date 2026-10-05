using FamilySafety.Application.FamilyDates;
using FamilySafety.Application.Profiles;

namespace FamilySafety.Api.FamilyDates;

public static class SharedFamilyDateEndpoints
{
    public static IEndpointRouteBuilder MapSharedFamilyDateEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/family-groups/{familyGroupId:guid}/dates");
        group.MapGet("", (Guid familyGroupId, ISharedFamilyDateService service, CancellationToken ct) =>
            ExecuteAsync(() => service.ListAsync(familyGroupId, ct)));
        group.MapPost("", (Guid familyGroupId, CreateSharedFamilyDateRequest request, ISharedFamilyDateService service, CancellationToken ct) =>
            ExecuteAsync(() => service.CreateAsync(familyGroupId, request, ct)));
        group.MapPut("/{dateId:guid}", (Guid familyGroupId, Guid dateId, EditSharedFamilyDateRequest request, ISharedFamilyDateService service, CancellationToken ct) =>
            ExecuteAsync(() => service.EditAsync(familyGroupId, dateId, request, ct)));
        group.MapDelete("/{dateId:guid}", (Guid familyGroupId, Guid dateId, Guid version, ISharedFamilyDateService service, CancellationToken ct) =>
            ExecuteAsync(async () => { await service.DeleteAsync(familyGroupId, dateId, version, ct); return Results.NoContent(); }));
        return endpoints;
    }
    private static async Task<IResult> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        try { var result = await operation(); return result is IResult response ? response : Results.Ok(result); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (SharedFamilyDateUnavailableException) { return Results.NotFound(); }
        catch (SharedFamilyDateConflictException) { return Results.Conflict(); }
        catch (SharedFamilyDateValidationException error) { return Results.ValidationProblem(error.Errors); }
    }
}
