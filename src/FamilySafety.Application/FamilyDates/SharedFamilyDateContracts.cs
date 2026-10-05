namespace FamilySafety.Application.FamilyDates;

public sealed record FamilyDatePerson(Guid ProfileId, string DisplayName);
public sealed record SharedFamilyDateView(Guid DateId, string Title, string Description, DateOnly Date,
    Guid CreatorProfileId, IReadOnlyList<FamilyDatePerson> AssociatedPeople, bool CanEdit,
    bool CanManageAssociations, bool CanDelete, Guid Version);
public sealed record SharedFamilyDatesView(bool CanCreate, IReadOnlyList<SharedFamilyDateView> Dates);
public sealed record CreateSharedFamilyDateRequest(string Title, string Description, DateOnly Date, Guid[] AssociatedProfileIds);
// Null means preserve associations: participants cannot submit any association change.
public sealed record EditSharedFamilyDateRequest(Guid Version, string Title, string Description, DateOnly Date, Guid[]? AssociatedProfileIds);
public sealed class SharedFamilyDateUnavailableException : Exception;
public sealed class SharedFamilyDateConflictException : Exception;
public sealed class SharedFamilyDateValidationException(IDictionary<string, string[]> errors) : Exception
{ public IDictionary<string, string[]> Errors { get; } = errors; }

public interface ISharedFamilyDateRepository
{
    Task<SharedFamilyDatesView?> ListAsync(Guid accountId, Guid familyGroupId, CancellationToken ct);
    Task<SharedFamilyDateView?> CreateAsync(Guid accountId, Guid familyGroupId, CreateSharedFamilyDateRequest request, CancellationToken ct);
    Task<SharedFamilyDateView?> EditAsync(Guid accountId, Guid familyGroupId, Guid dateId, EditSharedFamilyDateRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid accountId, Guid familyGroupId, Guid dateId, Guid version, CancellationToken ct);
    Task ReconcileAsync(CancellationToken ct);
}
public interface ISharedFamilyDateService
{
    Task<SharedFamilyDatesView> ListAsync(Guid familyGroupId, CancellationToken ct);
    Task<SharedFamilyDateView> CreateAsync(Guid familyGroupId, CreateSharedFamilyDateRequest request, CancellationToken ct);
    Task<SharedFamilyDateView> EditAsync(Guid familyGroupId, Guid dateId, EditSharedFamilyDateRequest request, CancellationToken ct);
    Task DeleteAsync(Guid familyGroupId, Guid dateId, Guid version, CancellationToken ct);
}
