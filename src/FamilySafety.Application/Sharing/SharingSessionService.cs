using FamilySafety.Application.Profiles;

namespace FamilySafety.Application.Sharing;

public sealed class SharingSessionService(ISharingSessionRepository repository, ICurrentAccountAccessor currentAccount)
{
    private Guid AccountId => currentAccount.Current?.AccountId ?? throw new CurrentAccountUnavailableException();
    public async Task<SharingSessionView> CreateAsync(CreateSharingSessionRequest request, CancellationToken ct) =>
        await repository.CreateAsync(AccountId, request, ct) ?? throw new SharingSessionUnavailableException();
    public Task<IReadOnlyList<SharingSessionView>> ListAsync(bool received, CancellationToken ct) => repository.ListAsync(AccountId, received, ct);
    public async Task<SharingSessionView> GetAsync(Guid id, CancellationToken ct) =>
        await repository.GetAsync(AccountId, id, ct) ?? throw new SharingSessionUnavailableException();
    public async Task StopAsync(Guid id, int version, CancellationToken ct)
    { if (!await repository.StopAsync(AccountId, id, version, ct)) throw new SharingSessionUnavailableException(); }
    public async Task<SharingSessionView> UpdateLocationAsync(Guid id, UpdateSharingLocationRequest request, CancellationToken ct) =>
        await repository.UpdateLocationAsync(AccountId, id, request, ct) ?? throw new SharingSessionUnavailableException();
}
