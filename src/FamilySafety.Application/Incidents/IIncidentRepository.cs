namespace FamilySafety.Application.Incidents;

/// <summary>
/// Persistence and commit-time authorization for Incidents. Every method returns null for the
/// uniform unavailable result; the service maps that to <see cref="IncidentUnavailableException"/>.
/// </summary>
public interface IIncidentRepository
{
    Task<IReadOnlyList<IncidentSummaryView>> GetVisibleAsync(Guid accountId, CancellationToken cancellationToken);
    Task<IncidentView?> GetAsync(Guid accountId, Guid incidentId, CancellationToken cancellationToken);
    Task<IncidentView?> CreateAsync(Guid accountId, Guid familyGroupId, CreateIncidentRequest request,
        CancellationToken cancellationToken);
    Task<IncidentView?> ReplaceParticipantsAsync(Guid accountId, Guid incidentId,
        ReplaceIncidentParticipantsRequest request, CancellationToken cancellationToken);
    Task<IncidentView?> TransferCoordinatorAsync(Guid accountId, Guid incidentId,
        TransferIncidentCoordinatorRequest request, CancellationToken cancellationToken);
    Task<IncidentView?> AddEntryAsync(Guid accountId, Guid incidentId, AddIncidentEntryRequest request,
        CancellationToken cancellationToken);
    Task<IncidentView?> CreateTaskAsync(Guid accountId, Guid incidentId, CreateIncidentTaskRequest request,
        CancellationToken cancellationToken);
    Task<IncidentView?> CompleteTaskAsync(Guid accountId, Guid incidentId, Guid taskId,
        CompleteIncidentTaskRequest request, CancellationToken cancellationToken);
    Task<IncidentView?> CancelTaskAsync(Guid accountId, Guid incidentId, Guid taskId,
        CancelIncidentTaskRequest request, CancellationToken cancellationToken);
    Task<IncidentView?> ResolveAsync(Guid accountId, Guid incidentId, ResolveIncidentRequest request,
        CancellationToken cancellationToken);
    Task ReconcileAsync(CancellationToken cancellationToken);
}
