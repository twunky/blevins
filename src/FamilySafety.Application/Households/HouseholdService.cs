using FamilySafety.Application.Profiles;

namespace FamilySafety.Application.Households;

public sealed class HouseholdService(IHouseholdRepository repository, ICurrentAccountAccessor actor)
{
    private Guid Account => actor.Current?.AccountId ?? throw new CurrentAccountUnavailableException();

    public async Task<HouseholdStructureView> ListAsync(Guid family, CancellationToken ct) =>
        await repository.ListAsync(Account, family, ct) ?? throw new HouseholdUnavailableException();

    public async Task<HouseholdView> CreateAsync(Guid family, CreateHouseholdRequest request, CancellationToken ct) =>
        await repository.CreateAsync(Account, family, request.Name, ct) ?? throw new HouseholdUnavailableException();

    public async Task<HouseholdView> RenameAsync(Guid family, Guid household, RenameHouseholdRequest request, CancellationToken ct) =>
        await repository.RenameAsync(Account, family, household, request, ct) ?? throw new HouseholdUnavailableException();

    public async Task AssignAsync(Guid family, Guid household, AssignHouseholdMemberRequest request, CancellationToken ct)
    {
        if (!await repository.AssignAsync(Account, family, household, request, ct)) throw new HouseholdUnavailableException();
    }

    public async Task UnassignAsync(Guid family, Guid household, Guid profile, Guid version, CancellationToken ct)
    {
        if (!await repository.UnassignAsync(Account, family, household, profile, version, ct)) throw new HouseholdUnavailableException();
    }

    public async Task DeleteAsync(Guid family, Guid household, Guid version, CancellationToken ct)
    {
        if (!await repository.DeleteAsync(Account, family, household, version, ct)) throw new HouseholdUnavailableException();
    }
}
