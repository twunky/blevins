using FamilySafety.Application.Concerns;
using FamilySafety.Application.Profiles;

namespace FamilySafety.Api.Concerns;

public static class ConcernEndpoints
{
    public static IEndpointRouteBuilder MapConcernEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/concerns", ListAsync);
        endpoints.MapGet("/api/concerns/{concernId:guid}", GetAsync);
        endpoints.MapPost("/api/family-groups/{familyGroupId:guid}/concerns", CreateAsync);
        endpoints.MapPut("/api/concerns/{concernId:guid}/participants", ReplaceParticipantsAsync);
        endpoints.MapPost("/api/concerns/{concernId:guid}/entries", AddEntryAsync);
        endpoints.MapPost("/api/concerns/{concernId:guid}/resolve", ResolveAsync);
        return endpoints;
    }

    private static async Task<IResult> ListAsync(IConcernService service, CancellationToken ct)
    {
        try { return Results.Ok(await service.GetVisibleAsync(ct)); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
    }

    private static async Task<IResult> GetAsync(Guid concernId, IConcernService service, CancellationToken ct)
    {
        try { return Results.Ok(await service.GetAsync(concernId, ct)); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (ConcernUnavailableException) { return Results.NotFound(); }
    }

    private static async Task<IResult> CreateAsync(Guid familyGroupId, CreateConcernRequest request,
        IConcernService service, CancellationToken ct)
    {
        try
        {
            var created = await service.CreateAsync(familyGroupId, request, ct);
            return Results.Created($"/api/concerns/{created.ConcernId:D}", created);
        }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (ConcernUnavailableException) { return Results.NotFound(); }
        catch (ConcernValidationException exception) { return Validation(exception); }
    }

    private static async Task<IResult> ReplaceParticipantsAsync(Guid concernId,
        ReplaceConcernParticipantsRequest request, IConcernService service, CancellationToken ct)
    {
        try { return Results.Ok(await service.ReplaceParticipantsAsync(concernId, request, ct)); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (ConcernUnavailableException) { return Results.NotFound(); }
        catch (ConcernValidationException exception) { return Validation(exception); }
    }

    private static async Task<IResult> AddEntryAsync(Guid concernId, AddConcernEntryRequest request,
        IConcernService service, CancellationToken ct)
    {
        try { return Results.Created($"/api/concerns/{concernId:D}", await service.AddEntryAsync(concernId, request, ct)); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (ConcernUnavailableException) { return Results.NotFound(); }
        catch (ConcernValidationException exception) { return Validation(exception); }
    }

    private static async Task<IResult> ResolveAsync(Guid concernId, ResolveConcernRequest request,
        IConcernService service, CancellationToken ct)
    {
        try { return Results.Ok(await service.ResolveAsync(concernId, request, ct)); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (ConcernUnavailableException) { return Results.NotFound(); }
        catch (ConcernValidationException exception) { return Validation(exception); }
    }

    private static IResult Validation(ConcernValidationException exception) =>
        Results.ValidationProblem(exception.Errors.ToDictionary(pair => pair.Key, pair => pair.Value));
}
