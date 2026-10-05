using FamilySafety.Application.Profiles;

namespace FamilySafety.Application.Events;

public sealed class EventNeedService(IEventNeedRepository repository, ICurrentAccountAccessor currentAccount) : IEventNeedService
{
    private Guid AccountId => currentAccount.Current?.AccountId ?? throw new CurrentAccountUnavailableException();
    public async Task<EventNeedsView> GetAsync(Guid eventId, CancellationToken ct) =>
        await repository.GetAsync(AccountId, eventId, ct) ?? throw new FamilyEventUnavailableException();
    public async Task<EventNeedView> CreateAsync(Guid eventId, CreateEventNeedRequest request, CancellationToken ct) =>
        await repository.CreateAsync(AccountId, eventId, request, ct) ?? throw new FamilyEventUnavailableException();
    public async Task<EventNeedView> EditAsync(Guid eventId, Guid needId, EditEventNeedRequest request, CancellationToken ct) =>
        await repository.EditAsync(AccountId, eventId, needId, request, ct) ?? throw new FamilyEventUnavailableException();
    public async Task RemoveAsync(Guid eventId, Guid needId, Guid version, CancellationToken ct)
    {
        if (!await repository.RemoveAsync(AccountId, eventId, needId, version, ct)) throw new FamilyEventUnavailableException();
    }
    public async Task<EventNeedView> ActAsync(Guid eventId, Guid needId, Guid version, string action, Guid? assigneeId, CancellationToken ct) =>
        await repository.ActAsync(AccountId, eventId, needId, version, action, assigneeId, ct) ?? throw new FamilyEventUnavailableException();
}
