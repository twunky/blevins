namespace FamilySafety.Application.Concerns;

public interface IConcernService
{
    Task<IReadOnlyList<ConcernView>> GetVisibleAsync(CancellationToken cancellationToken);
    Task<ConcernView> GetAsync(Guid concernId, CancellationToken cancellationToken);
    Task<ConcernView> CreateAsync(Guid familyGroupId, CreateConcernRequest request,
        CancellationToken cancellationToken);
    Task<ConcernView> ReplaceParticipantsAsync(Guid concernId,
        ReplaceConcernParticipantsRequest request, CancellationToken cancellationToken);
    Task<ConcernView> AddEntryAsync(Guid concernId, AddConcernEntryRequest request,
        CancellationToken cancellationToken);
    Task<ConcernView> ResolveAsync(Guid concernId, ResolveConcernRequest request,
        CancellationToken cancellationToken);
}
