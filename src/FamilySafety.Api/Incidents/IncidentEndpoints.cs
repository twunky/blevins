using FamilySafety.Application.Incidents;
using FamilySafety.Application.Profiles;

namespace FamilySafety.Api.Incidents;

/// <summary>
/// Private Incident API. Every unavailable or unauthorized target maps to the same 404; a missing
/// current account maps to 401. There are no audit, reopen, edit, delete, or reassignment routes.
/// </summary>
public static class IncidentEndpoints
{
    public static IEndpointRouteBuilder MapIncidentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/incidents", ListAsync);
        endpoints.MapGet("/api/incidents/{incidentId:guid}", GetAsync);
        endpoints.MapPost("/api/family-groups/{familyGroupId:guid}/incidents", CreateAsync);
        endpoints.MapPut("/api/incidents/{incidentId:guid}/participants", ReplaceParticipantsAsync);
        endpoints.MapPost("/api/incidents/{incidentId:guid}/coordinator", TransferCoordinatorAsync);
        endpoints.MapPost("/api/incidents/{incidentId:guid}/entries", AddEntryAsync);
        endpoints.MapPost("/api/incidents/{incidentId:guid}/tasks", CreateTaskAsync);
        endpoints.MapPost("/api/incidents/{incidentId:guid}/tasks/{taskId:guid}/complete", CompleteTaskAsync);
        endpoints.MapPost("/api/incidents/{incidentId:guid}/tasks/{taskId:guid}/cancel", CancelTaskAsync);
        endpoints.MapPost("/api/incidents/{incidentId:guid}/resolve", ResolveAsync);
        return endpoints;
    }

    private static async Task<IResult> ListAsync(IIncidentService service, CancellationToken ct)
    {
        try { return Results.Ok(await service.GetVisibleAsync(ct)); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
    }

    private static Task<IResult> GetAsync(Guid incidentId, IIncidentService service, CancellationToken ct) =>
        Handle(async () => Results.Ok(await service.GetAsync(incidentId, ct)));

    private static Task<IResult> CreateAsync(Guid familyGroupId, CreateIncidentRequest request,
        IIncidentService service, CancellationToken ct) =>
        Handle(async () =>
        {
            var created = await service.CreateAsync(familyGroupId, request, ct);
            return Results.Created($"/api/incidents/{created.IncidentId:D}", created);
        });

    private static Task<IResult> ReplaceParticipantsAsync(Guid incidentId,
        ReplaceIncidentParticipantsRequest request, IIncidentService service, CancellationToken ct) =>
        Handle(async () => Results.Ok(await service.ReplaceParticipantsAsync(incidentId, request, ct)));

    private static Task<IResult> TransferCoordinatorAsync(Guid incidentId,
        TransferIncidentCoordinatorRequest request, IIncidentService service, CancellationToken ct) =>
        Handle(async () => Results.Ok(await service.TransferCoordinatorAsync(incidentId, request, ct)));

    private static Task<IResult> AddEntryAsync(Guid incidentId, AddIncidentEntryRequest request,
        IIncidentService service, CancellationToken ct) =>
        Handle(async () => Results.Created($"/api/incidents/{incidentId:D}",
            await service.AddEntryAsync(incidentId, request, ct)));

    private static Task<IResult> CreateTaskAsync(Guid incidentId, CreateIncidentTaskRequest request,
        IIncidentService service, CancellationToken ct) =>
        Handle(async () => Results.Created($"/api/incidents/{incidentId:D}",
            await service.CreateTaskAsync(incidentId, request, ct)));

    private static Task<IResult> CompleteTaskAsync(Guid incidentId, Guid taskId,
        CompleteIncidentTaskRequest request, IIncidentService service, CancellationToken ct) =>
        Handle(async () => Results.Ok(await service.CompleteTaskAsync(incidentId, taskId, request, ct)));

    private static Task<IResult> CancelTaskAsync(Guid incidentId, Guid taskId,
        CancelIncidentTaskRequest request, IIncidentService service, CancellationToken ct) =>
        Handle(async () => Results.Ok(await service.CancelTaskAsync(incidentId, taskId, request, ct)));

    private static Task<IResult> ResolveAsync(Guid incidentId, ResolveIncidentRequest request,
        IIncidentService service, CancellationToken ct) =>
        Handle(async () => Results.Ok(await service.ResolveAsync(incidentId, request, ct)));

    private static async Task<IResult> Handle(Func<Task<IResult>> action)
    {
        try { return await action(); }
        catch (CurrentAccountUnavailableException) { return Results.Unauthorized(); }
        catch (IncidentUnavailableException) { return Results.NotFound(); }
        catch (IncidentValidationException exception)
        {
            return Results.ValidationProblem(exception.Errors.ToDictionary(pair => pair.Key, pair => pair.Value));
        }
    }
}
