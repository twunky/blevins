using FamilySafety.Application.Profiles;
using FamilySafety.Application.TrustedContacts;

namespace FamilySafety.Api.TrustedContacts;

public static class TrustedContactEndpoints
{
    public static IEndpointRouteBuilder MapTrustedContactEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/trusted-contacts");
        group.AddEndpointFilter(async (context, next) => {
            context.HttpContext.Response.Headers.CacheControl = "no-store";
            return await next(context);
        });
        group.MapGet("", (TrustedContactService service, CancellationToken ct) => ExecuteAsync(() => service.ListAsync(ct)));
        group.MapGet("/{contactId:guid}", (Guid contactId, TrustedContactService service, CancellationToken ct) =>
            ExecuteAsync(() => service.ReadAsync(contactId, ct)));
        group.MapPost("", (SaveTrustedContactRequest request, TrustedContactService service, CancellationToken ct) =>
            ExecuteAsync(() => service.CreateAsync(request, ct)));
        group.MapPut("/{contactId:guid}", (Guid contactId, EditTrustedContactRequest request, TrustedContactService service, CancellationToken ct) =>
            ExecuteAsync(() => service.EditAsync(contactId, request, ct)));
        group.MapDelete("/{contactId:guid}", (Guid contactId, Guid version, TrustedContactService service, CancellationToken ct) =>
            ExecuteAsync(async () => { await service.DeleteAsync(contactId, version, ct); return Results.NoContent(); }));
        return endpoints;
    }
    private static async Task<IResult> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        try { var result = await operation(); return result is IResult response ? response : Results.Ok(result); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (TrustedContactUnavailableException) { return Results.NotFound(); }
        catch (TrustedContactDuplicateException) { return Results.Conflict(new { code = "DuplicateContact" }); }
        catch (TrustedContactConflictException) { return Results.Conflict(new { code = "ContactChanged" }); }
        catch (TrustedContactValidationException error) { return Results.ValidationProblem(error.Errors); }
    }
}
