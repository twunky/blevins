namespace FamilySafety.Application.Incidents;

public interface IIncidentService
{
    Task<IReadOnlyList<IncidentSummaryView>> GetVisibleAsync(CancellationToken cancellationToken);
    Task<IncidentView> GetAsync(Guid incidentId, CancellationToken cancellationToken);
    Task<IncidentView> CreateAsync(Guid familyGroupId, CreateIncidentRequest request,
        CancellationToken cancellationToken);
    Task<IncidentView> ReplaceParticipantsAsync(Guid incidentId, ReplaceIncidentParticipantsRequest request,
        CancellationToken cancellationToken);
    Task<IncidentView> TransferCoordinatorAsync(Guid incidentId, TransferIncidentCoordinatorRequest request,
        CancellationToken cancellationToken);
    Task<IncidentView> AddEntryAsync(Guid incidentId, AddIncidentEntryRequest request,
        CancellationToken cancellationToken);
    Task<IncidentView> CreateTaskAsync(Guid incidentId, CreateIncidentTaskRequest request,
        CancellationToken cancellationToken);
    Task<IncidentView> CompleteTaskAsync(Guid incidentId, Guid taskId, CompleteIncidentTaskRequest request,
        CancellationToken cancellationToken);
    Task<IncidentView> CancelTaskAsync(Guid incidentId, Guid taskId, CancelIncidentTaskRequest request,
        CancellationToken cancellationToken);
    Task<IncidentView> ResolveAsync(Guid incidentId, ResolveIncidentRequest request,
        CancellationToken cancellationToken);
}
