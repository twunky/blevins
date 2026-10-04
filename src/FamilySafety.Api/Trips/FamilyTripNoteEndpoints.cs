using FamilySafety.Application.Profiles;
using FamilySafety.Application.Trips;

namespace FamilySafety.Api.Trips;

public static class FamilyTripNoteEndpoints
{
    public static IEndpointRouteBuilder MapFamilyTripNoteEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/trips/{tripId:guid}/notes", ListAsync);
        endpoints.MapPost("/api/trips/{tripId:guid}/notes", AddAsync);
        endpoints.MapPut("/api/trips/{tripId:guid}/notes/{noteId:guid}", EditAsync);
        endpoints.MapDelete("/api/trips/{tripId:guid}/notes/{noteId:guid}", DeleteAsync);
        endpoints.MapPost("/api/trips/{tripId:guid}/notes/{noteId:guid}/hide", HideAsync);
        return endpoints;
    }

    private static async Task<IResult> ListAsync(Guid tripId, IFamilyTripNoteService service,
        CancellationToken cancellationToken)
    {
        try { return Results.Ok(await service.ListAsync(tripId, cancellationToken)); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (FamilyTripNoteUnavailableException) { return Results.NotFound(); }
    }

    private static async Task<IResult> AddAsync(Guid tripId, TripNoteContentRequest request,
        IFamilyTripNoteService service, CancellationToken cancellationToken)
    {
        try
        {
            var note = await service.AddAsync(tripId, request, cancellationToken);
            return Results.Created($"/api/trips/{tripId:D}/notes", note);
        }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (FamilyTripNoteUnavailableException) { return Results.NotFound(); }
        catch (FamilyTripNoteValidationException)
        { return Results.ValidationProblem(new Dictionary<string, string[]> { ["Content"] = ["Enter 1 to 1,000 characters."] }); }
    }

    private static async Task<IResult> EditAsync(Guid tripId, Guid noteId, TripNoteContentRequest request,
        IFamilyTripNoteService service, CancellationToken cancellationToken)
    {
        try { return Results.Ok(await service.EditAsync(tripId, noteId, request, cancellationToken)); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (FamilyTripNoteUnavailableException) { return Results.NotFound(); }
        catch (FamilyTripNoteValidationException)
        { return Results.ValidationProblem(new Dictionary<string, string[]> { ["Content"] = ["Enter 1 to 1,000 characters."] }); }
    }

    private static async Task<IResult> DeleteAsync(Guid tripId, Guid noteId, IFamilyTripNoteService service,
        CancellationToken cancellationToken)
    {
        try { await service.DeleteAsync(tripId, noteId, cancellationToken); return Results.NoContent(); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (FamilyTripNoteUnavailableException) { return Results.NotFound(); }
    }

    private static async Task<IResult> HideAsync(Guid tripId, Guid noteId, IFamilyTripNoteService service,
        CancellationToken cancellationToken)
    {
        try { await service.HideAsync(tripId, noteId, cancellationToken); return Results.NoContent(); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (FamilyTripNoteUnavailableException) { return Results.NotFound(); }
    }
}
