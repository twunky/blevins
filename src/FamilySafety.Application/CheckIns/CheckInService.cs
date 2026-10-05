using FamilySafety.Application.Profiles;

namespace FamilySafety.Application.CheckIns;

public sealed class CheckInService(ICheckInRepository repository, ICurrentAccountAccessor currentAccount) : ICheckInService
{
    private Guid AccountId => currentAccount.Current?.AccountId ?? throw new CurrentAccountUnavailableException();
    public async Task<CheckInView> CreateAsync(CreateCheckInRequest request, CancellationToken ct) =>
        await repository.CreateAsync(AccountId, request, ct) ?? throw new CheckInUnavailableException();
    public Task<IReadOnlyList<CheckInView>> ListAsync(bool received, CancellationToken ct) => repository.ListAsync(AccountId, received, ct);
    public async Task<CheckInView> GetAsync(Guid checkInId, CancellationToken ct) =>
        await repository.GetAsync(AccountId, checkInId, ct) ?? throw new CheckInUnavailableException();
}
