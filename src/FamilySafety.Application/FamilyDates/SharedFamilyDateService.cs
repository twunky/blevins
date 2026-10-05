using FamilySafety.Application.Profiles;

namespace FamilySafety.Application.FamilyDates;

public sealed class SharedFamilyDateService(ISharedFamilyDateRepository repository, ICurrentAccountAccessor currentAccount) : ISharedFamilyDateService
{
    private Guid AccountId => currentAccount.Current?.AccountId ?? throw new CurrentAccountUnavailableException();
    public async Task<SharedFamilyDatesView> ListAsync(Guid familyGroupId, CancellationToken ct) =>
        await repository.ListAsync(AccountId, familyGroupId, ct) ?? throw new SharedFamilyDateUnavailableException();
    public async Task<SharedFamilyDateView> CreateAsync(Guid familyGroupId, CreateSharedFamilyDateRequest request, CancellationToken ct) =>
        await repository.CreateAsync(AccountId, familyGroupId, request, ct) ?? throw new SharedFamilyDateUnavailableException();
    public async Task<SharedFamilyDateView> EditAsync(Guid familyGroupId, Guid dateId, EditSharedFamilyDateRequest request, CancellationToken ct) =>
        await repository.EditAsync(AccountId, familyGroupId, dateId, request, ct) ?? throw new SharedFamilyDateUnavailableException();
    public async Task DeleteAsync(Guid familyGroupId, Guid dateId, Guid version, CancellationToken ct)
    {
        if (!await repository.DeleteAsync(AccountId, familyGroupId, dateId, version, ct)) throw new SharedFamilyDateUnavailableException();
    }
}
