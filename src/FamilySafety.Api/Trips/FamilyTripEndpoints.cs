using FamilySafety.Application.Profiles;
using FamilySafety.Application.Trips;

namespace FamilySafety.Api.Trips;

public static class FamilyTripEndpoints
{
    public static IEndpointRouteBuilder MapFamilyTripEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/trips", GetVisibleAsync);
        endpoints.MapGet("/api/trips/{tripId:guid}", GetAsync);
        endpoints.MapPut("/api/trips/{tripId:guid}", UpdateAsync);
        endpoints.MapPost("/api/trips/{tripId:guid}/complete", CompleteAsync);
        endpoints.MapPost("/api/trips/{tripId:guid}/audience/preview", PreviewAudienceAsync);
        endpoints.MapPost("/api/trips/{tripId:guid}/audience/confirm", ConfirmAudienceAsync);
        endpoints.MapPost("/api/family-groups/{familyGroupId:guid}/trips", CreateAsync);
        return endpoints;
    }

    private static async Task<IResult> CompleteAsync(Guid tripId,
        CompleteFamilyTripRequest request, IFamilyTripService service, CancellationToken cancellationToken)
    {
        try { return Results.Ok(await service.CompleteAsync(tripId, request, cancellationToken)); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (FamilyTripUnavailableException) { return Results.NotFound(); }
        catch (FamilyTripCompletionConflictException)
        {
            return Results.Conflict(new { message = "The Trip changed or is already completed. Refresh the Trip before trying again." });
        }
        catch (FamilyTripValidationException exception)
        {
            return Results.ValidationProblem(exception.Errors.ToDictionary(error => error.Key, error => error.Value));
        }
    }

    private static async Task<IResult> GetVisibleAsync(
        IFamilyTripService service, CancellationToken cancellationToken)
    {
        try { return Results.Ok(await service.GetVisibleAsync(cancellationToken)); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
    }

    private static async Task<IResult> GetAsync(Guid tripId,
        IFamilyTripService service, CancellationToken cancellationToken)
    {
        try { return Results.Ok(await service.GetAsync(tripId, cancellationToken)); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (FamilyTripUnavailableException) { return Results.NotFound(); }
    }

    private static async Task<IResult> CreateAsync(Guid familyGroupId,
        CreateFamilyTripRequest request, IFamilyTripService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await service.CreateAsync(familyGroupId, request, cancellationToken);
            return Results.Created($"/api/trips/{created.TripId:D}", created);
        }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (FamilyTripUnavailableException) { return Results.NotFound(); }
        catch (FamilyTripValidationException exception)
        {
            return Results.ValidationProblem(exception.Errors.ToDictionary(
                error => error.Key, error => error.Value));
        }
    }

    private static async Task<IResult> UpdateAsync(Guid tripId,
        UpdateFamilyTripRequest request, IFamilyTripService service,
        CancellationToken cancellationToken)
    {
        try { return Results.Ok(await service.UpdateAsync(tripId, request, cancellationToken)); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (FamilyTripUnavailableException) { return Results.NotFound(); }
        catch (FamilyTripValidationException exception)
        {
            return Results.ValidationProblem(exception.Errors.ToDictionary(
                error => error.Key, error => error.Value));
        }
    }

    private static async Task<IResult> PreviewAudienceAsync(Guid tripId,
        ReplaceTripAudienceRequest request, IFamilyTripService service,
        CancellationToken cancellationToken)
    {
        try { return Results.Ok(await service.PreviewAudienceReplacementAsync(tripId, request, cancellationToken)); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (FamilyTripUnavailableException) { return Results.NotFound(); }
        catch (FamilyTripValidationException exception)
        {
            return Results.ValidationProblem(exception.Errors.ToDictionary(error => error.Key, error => error.Value));
        }
    }

    private static async Task<IResult> ConfirmAudienceAsync(Guid tripId,
        ConfirmTripAudienceRequest request, IFamilyTripService service,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PreviewToken))
            return Results.ValidationProblem(new Dictionary<string, string[]>
                { ["PreviewToken"] = ["Preview and confirm the proposed audience first."] });
        try
        {
            var replaced = await service.ReplaceAudienceAsync(tripId,
                new ReplaceTripAudienceRequest(request.AudienceType, request.AudienceProfileIds),
                request.PreviewToken, cancellationToken);
            return Results.Ok(replaced);
        }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (FamilyTripUnavailableException) { return Results.NotFound(); }
        catch (FamilyTripAudienceConflictException)
        {
            return Results.Conflict(new { message = "The audience changed since preview. Refresh the preview and confirm again." });
        }
        catch (FamilyTripValidationException exception)
        {
            return Results.ValidationProblem(exception.Errors.ToDictionary(error => error.Key, error => error.Value));
        }
    }
}
