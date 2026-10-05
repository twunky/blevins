using FamilySafety.Domain.CheckIns;

namespace FamilySafety.Application.CheckIns;

// No caller-supplied author, location or contextual association fields.
public sealed record CreateCheckInRequest(Guid FamilyGroupId, CheckInStatus? Status, string? Note, Guid[] RecipientProfileIds);
public sealed record CheckInView(Guid CheckInId, Guid FamilyGroupId, Guid AuthorProfileId,
    string AuthorDisplayName, DateTimeOffset CreatedAtUtc, CheckInStatus Status, string? Note,
    IReadOnlyList<Guid>? RecipientProfileIds);
public sealed class CheckInUnavailableException : Exception;
public sealed class CheckInValidationException(IDictionary<string, string[]> errors) : Exception
{ public IDictionary<string, string[]> Errors { get; } = errors; }
public interface ICheckInRepository
{
    Task<CheckInView?> CreateAsync(Guid accountId, CreateCheckInRequest request, CancellationToken ct);
    Task<IReadOnlyList<CheckInView>> ListAsync(Guid accountId, bool received, CancellationToken ct);
    Task<CheckInView?> GetAsync(Guid accountId, Guid checkInId, CancellationToken ct);
}
public interface ICheckInService
{
    Task<CheckInView> CreateAsync(CreateCheckInRequest request, CancellationToken ct);
    Task<IReadOnlyList<CheckInView>> ListAsync(bool received, CancellationToken ct);
    Task<CheckInView> GetAsync(Guid checkInId, CancellationToken ct);
}
