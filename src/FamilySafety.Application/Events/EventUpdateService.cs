using FamilySafety.Application.Profiles;

namespace FamilySafety.Application.Events;

public sealed class EventUpdateService(IEventUpdateRepository repository, ICurrentAccountAccessor currentAccount) : IEventUpdateService
{
    private Guid AccountId => currentAccount.Current?.AccountId ?? throw new CurrentAccountUnavailableException();
    public async Task<EventUpdatesView> GetAsync(Guid eventId, CancellationToken ct) =>
        await repository.GetAsync(AccountId, eventId, ct) ?? throw new FamilyEventUnavailableException();
    public async Task<EventUpdateView> CreateAsync(Guid eventId, CreateEventUpdateRequest request, CancellationToken ct) =>
        await repository.CreateAsync(AccountId, eventId, request, ct) ?? throw new FamilyEventUnavailableException();
    public async Task<EventUpdateView> EditAsync(Guid eventId, Guid updateId, EditEventUpdateRequest request, CancellationToken ct) =>
        await repository.EditAsync(AccountId, eventId, updateId, request, ct) ?? throw new FamilyEventUnavailableException();
    public async Task HideAsync(Guid eventId, Guid updateId, Guid version, CancellationToken ct)
    {
        if (!await repository.HideAsync(AccountId, eventId, updateId, version, ct)) throw new FamilyEventUnavailableException();
    }
}
