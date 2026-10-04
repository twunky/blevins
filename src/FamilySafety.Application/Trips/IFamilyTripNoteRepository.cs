namespace FamilySafety.Application.Trips;

public interface IFamilyTripNoteRepository
{
    Task<IReadOnlyList<TripNoteView>?> ListAsync(Guid accountId, Guid tripId, CancellationToken cancellationToken);
    Task<TripNoteView?> AddAsync(Guid accountId, Guid tripId, string content, CancellationToken cancellationToken);
    Task<TripNoteView?> EditAsync(Guid accountId, Guid tripId, Guid noteId, string content, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid accountId, Guid tripId, Guid noteId, CancellationToken cancellationToken);
    Task<bool> HideAsync(Guid accountId, Guid tripId, Guid noteId, CancellationToken cancellationToken);
}
