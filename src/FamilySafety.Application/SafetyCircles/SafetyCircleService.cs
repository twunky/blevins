using FamilySafety.Application.Profiles;
namespace FamilySafety.Application.SafetyCircles;

public sealed class SafetyCircleService(ISafetyCircleRepository repository, ICurrentAccountAccessor actor)
{
    private Guid Account => actor.Current?.AccountId ?? throw new CurrentAccountUnavailableException();
    public async Task<IReadOnlyList<SafetyCircleView>> ListAsync(Guid family, CancellationToken ct) =>
        await repository.ListAsync(Account, family, ct) ?? throw new SafetyCircleUnavailableException();
    public async Task<SafetyCircleView> ReadAsync(Guid family, Guid circle, CancellationToken ct) =>
        await repository.ReadAsync(Account, family, circle, ct) ?? throw new SafetyCircleUnavailableException();
    public async Task<SafetyCircleView> CreateAsync(Guid family, CreateSafetyCircleRequest request, CancellationToken ct) =>
        await repository.CreateAsync(Account, family, request.Name, ct) ?? throw new SafetyCircleUnavailableException();
    public async Task<SafetyCircleView> RenameAsync(Guid family, Guid circle, RenameSafetyCircleRequest request, CancellationToken ct) =>
        await repository.RenameAsync(Account, family, circle, request, ct) ?? throw new SafetyCircleUnavailableException();
    public async Task<SafetyCircleView> AddAsync(Guid family, Guid circle, AddSafetyCircleMemberRequest request, CancellationToken ct) =>
        await repository.AddAsync(Account, family, circle, request, ct) ?? throw new SafetyCircleUnavailableException();
    public async Task<SafetyCircleView> RemoveAsync(Guid family, Guid circle, Guid profile, Guid version, CancellationToken ct) =>
        await repository.RemoveAsync(Account, family, circle, profile, version, ct) ?? throw new SafetyCircleUnavailableException();
    public async Task DeleteAsync(Guid family, Guid circle, Guid version, CancellationToken ct)
    { if (!await repository.DeleteAsync(Account, family, circle, version, ct)) throw new SafetyCircleUnavailableException(); }
}
