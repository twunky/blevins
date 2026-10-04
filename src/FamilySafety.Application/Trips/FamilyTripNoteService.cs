using FamilySafety.Application.Profiles;

namespace FamilySafety.Application.Trips;

public interface IFamilyTripNoteService
{
    Task<IReadOnlyList<TripNoteView>> ListAsync(Guid tripId, CancellationToken cancellationToken);
    Task<TripNoteView> AddAsync(Guid tripId, TripNoteContentRequest request, CancellationToken cancellationToken);
    Task<TripNoteView> EditAsync(Guid tripId, Guid noteId, TripNoteContentRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid tripId, Guid noteId, CancellationToken cancellationToken);
    Task HideAsync(Guid tripId, Guid noteId, CancellationToken cancellationToken);
}

public sealed class FamilyTripNoteService(ICurrentAccountAccessor accounts, IFamilyTripNoteRepository repository)
    : IFamilyTripNoteService
{
    public async Task<IReadOnlyList<TripNoteView>> ListAsync(Guid tripId, CancellationToken cancellationToken) =>
        await repository.ListAsync(AccountId(), RequireId(tripId), cancellationToken)
        ?? throw new FamilyTripNoteUnavailableException();

    public async Task<TripNoteView> AddAsync(Guid tripId, TripNoteContentRequest request, CancellationToken cancellationToken) =>
        await repository.AddAsync(AccountId(), RequireId(tripId), ValidateContent(request), cancellationToken)
        ?? throw new FamilyTripNoteUnavailableException();

    public async Task<TripNoteView> EditAsync(Guid tripId, Guid noteId, TripNoteContentRequest request,
        CancellationToken cancellationToken) =>
        await repository.EditAsync(AccountId(), RequireId(tripId), RequireId(noteId), ValidateContent(request), cancellationToken)
        ?? throw new FamilyTripNoteUnavailableException();

    public async Task DeleteAsync(Guid tripId, Guid noteId, CancellationToken cancellationToken)
    {
        if (!await repository.DeleteAsync(AccountId(), RequireId(tripId), RequireId(noteId), cancellationToken))
            throw new FamilyTripNoteUnavailableException();
    }

    public async Task HideAsync(Guid tripId, Guid noteId, CancellationToken cancellationToken)
    {
        if (!await repository.HideAsync(AccountId(), RequireId(tripId), RequireId(noteId), cancellationToken))
            throw new FamilyTripNoteUnavailableException();
    }

    private Guid AccountId() => accounts.Current?.AccountId ?? throw new CurrentAccountUnavailableException();
    private static Guid RequireId(Guid id) => id == Guid.Empty ? throw new FamilyTripNoteUnavailableException() : id;
    private static string ValidateContent(TripNoteContentRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Content) || request.Content.Trim().Length > 1000)
            throw new FamilyTripNoteValidationException();
        return request.Content.Trim();
    }
}
