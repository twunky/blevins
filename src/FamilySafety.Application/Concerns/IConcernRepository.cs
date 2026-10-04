namespace FamilySafety.Application.Concerns;

public interface IConcernRepository
{
    Task<IReadOnlyList<ConcernView>> GetVisibleAsync(Guid accountId, CancellationToken cancellationToken);
    Task<ConcernView?> GetAsync(Guid accountId, Guid concernId, CancellationToken cancellationToken);
    Task<ConcernView?> CreateAsync(Guid accountId, Guid familyGroupId,
        CreateConcernRequest request, CancellationToken cancellationToken);
    Task<ConcernView?> ReplaceParticipantsAsync(Guid accountId, Guid concernId,
        ReplaceConcernParticipantsRequest request, CancellationToken cancellationToken);
    Task<ConcernView?> AddEntryAsync(Guid accountId, Guid concernId,
        AddConcernEntryRequest request, CancellationToken cancellationToken);
    Task<ConcernView?> ResolveAsync(Guid accountId, Guid concernId,
        ResolveConcernRequest request, CancellationToken cancellationToken);
    Task ReconcileAsync(CancellationToken cancellationToken);
}
