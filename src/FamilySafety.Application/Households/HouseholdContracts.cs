namespace FamilySafety.Application.Households;

public sealed record HouseholdMemberView(Guid ProfileId, string DisplayName);
public sealed record HouseholdView(Guid HouseholdId, string Name, Guid Version, IReadOnlyList<HouseholdMemberView> Members);
public sealed record HouseholdStructureView(bool CanManage, IReadOnlyList<HouseholdView> Households, IReadOnlyList<HouseholdMemberView> UnassignedMembers);
public sealed record CreateHouseholdRequest(string Name);
public sealed record RenameHouseholdRequest(Guid Version, string Name);
// Both the observed source and target versions protect moves; null source means observed unassigned.
public sealed record AssignHouseholdMemberRequest(Guid Version, Guid ProfileId, Guid? CurrentHouseholdId, Guid? SourceVersion);
public sealed class HouseholdUnavailableException : Exception;
public sealed class HouseholdConflictException : Exception;
public sealed class HouseholdDuplicateException : Exception;
public sealed class HouseholdValidationException : Exception;

public interface IHouseholdRepository
{
    Task<HouseholdStructureView?> ListAsync(Guid accountId, Guid familyId, CancellationToken ct);
    Task<HouseholdView?> CreateAsync(Guid accountId, Guid familyId, string name, CancellationToken ct);
    Task<HouseholdView?> RenameAsync(Guid accountId, Guid familyId, Guid householdId, RenameHouseholdRequest request, CancellationToken ct);
    Task<bool> AssignAsync(Guid accountId, Guid familyId, Guid householdId, AssignHouseholdMemberRequest request, CancellationToken ct);
    Task<bool> UnassignAsync(Guid accountId, Guid familyId, Guid householdId, Guid profileId, Guid version, CancellationToken ct);
    Task<bool> DeleteAsync(Guid accountId, Guid familyId, Guid householdId, Guid version, CancellationToken ct);
}
