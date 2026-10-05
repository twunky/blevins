namespace FamilySafety.Application.SafetyCircles;

public sealed record SafetyCircleMemberView(Guid ProfileId, string DisplayName);
public sealed record SafetyCircleView(Guid SafetyCircleId, Guid FamilyGroupId, string Name, Guid Version, IReadOnlyList<SafetyCircleMemberView> Members);
public sealed record CreateSafetyCircleRequest(string Name);
public sealed record RenameSafetyCircleRequest(Guid Version, string Name);
public sealed record AddSafetyCircleMemberRequest(Guid Version, Guid MemberProfileId);
public sealed class SafetyCircleUnavailableException : Exception;
public sealed class SafetyCircleConflictException : Exception;
public sealed class SafetyCircleDuplicateException : Exception;
public sealed class SafetyCircleValidationException : Exception;
public interface ISafetyCircleRepository
{
    Task<IReadOnlyList<SafetyCircleView>?> ListAsync(Guid accountId, Guid familyId, CancellationToken ct);
    Task<SafetyCircleView?> ReadAsync(Guid accountId, Guid familyId, Guid circleId, CancellationToken ct);
    Task<SafetyCircleView?> CreateAsync(Guid accountId, Guid familyId, string name, CancellationToken ct);
    Task<SafetyCircleView?> RenameAsync(Guid accountId, Guid familyId, Guid circleId, RenameSafetyCircleRequest request, CancellationToken ct);
    Task<SafetyCircleView?> AddAsync(Guid accountId, Guid familyId, Guid circleId, AddSafetyCircleMemberRequest request, CancellationToken ct);
    Task<SafetyCircleView?> RemoveAsync(Guid accountId, Guid familyId, Guid circleId, Guid profileId, Guid version, CancellationToken ct);
    Task<bool> DeleteAsync(Guid accountId, Guid familyId, Guid circleId, Guid version, CancellationToken ct);
}
