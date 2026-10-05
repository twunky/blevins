namespace FamilySafety.Application.Sharing;

// Identity, start time, state and audience mutation are never supplied by a caller.
public sealed record CreateSharingSessionRequest(Guid FamilyGroupId, Guid[] RecipientProfileIds, DateTimeOffset? EndsAtUtc);
public sealed record UpdateSharingLocationRequest(double? Latitude, double? Longitude, double? Accuracy, DateTimeOffset? CapturedAtUtc, int Version);
public sealed record StopSharingSessionRequest(int Version);
public sealed record SharingLocationView(double Latitude, double Longitude, double Accuracy, DateTimeOffset CapturedAtUtc, DateTimeOffset ReceivedAtUtc);
public sealed record SharingRecipientView(Guid ProfileId, string DisplayName, bool HasAccess);
public sealed record SharingSessionView(Guid SharingSessionId, Guid FamilyGroupId, Guid AuthorProfileId, string AuthorDisplayName,
    DateTimeOffset StartedAtUtc, DateTimeOffset EndsAtUtc, string State, int Version,
    IReadOnlyList<SharingRecipientView>? Recipients, SharingLocationView? Location);
public sealed class SharingSessionUnavailableException : Exception;
public sealed class SharingSessionConflictException : Exception;
public sealed class SharingSessionValidationException(IDictionary<string, string[]> errors) : Exception
{ public IDictionary<string, string[]> Errors { get; } = errors; }
public interface ISharingSessionRepository
{
    Task<SharingSessionView?> CreateAsync(Guid accountId, CreateSharingSessionRequest request, CancellationToken ct);
    Task<IReadOnlyList<SharingSessionView>> ListAsync(Guid accountId, bool received, CancellationToken ct);
    Task<SharingSessionView?> GetAsync(Guid accountId, Guid sessionId, CancellationToken ct);
    Task<bool> StopAsync(Guid accountId, Guid sessionId, int version, CancellationToken ct);
    Task<SharingSessionView?> UpdateLocationAsync(Guid accountId, Guid sessionId, UpdateSharingLocationRequest request, CancellationToken ct);
    Task CleanupAsync(CancellationToken ct);
}
