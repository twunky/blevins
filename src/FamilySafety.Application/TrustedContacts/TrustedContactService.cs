using FamilySafety.Application.Profiles;

namespace FamilySafety.Application.TrustedContacts;

public sealed class TrustedContactService(ITrustedContactRepository repository, ICurrentAccountAccessor currentAccount)
{
    private Guid AccountId => currentAccount.Current?.AccountId ?? throw new CurrentAccountUnavailableException();
    public async Task<IReadOnlyList<TrustedContactView>> ListAsync(CancellationToken ct) =>
        await repository.ListAsync(AccountId, ct) ?? throw new TrustedContactUnavailableException();
    public async Task<TrustedContactView> ReadAsync(Guid contactId, CancellationToken ct) =>
        await repository.ReadAsync(AccountId, contactId, ct) ?? throw new TrustedContactUnavailableException();
    public async Task<TrustedContactView> CreateAsync(SaveTrustedContactRequest request, CancellationToken ct) =>
        await repository.CreateAsync(AccountId, request, ct) ?? throw new TrustedContactUnavailableException();
    public async Task<TrustedContactView> EditAsync(Guid contactId, EditTrustedContactRequest request, CancellationToken ct) =>
        await repository.EditAsync(AccountId, contactId, request, ct) ?? throw new TrustedContactUnavailableException();
    public async Task DeleteAsync(Guid contactId, Guid version, CancellationToken ct)
    {
        if (!await repository.DeleteAsync(AccountId, contactId, version, ct)) throw new TrustedContactUnavailableException();
    }
}
