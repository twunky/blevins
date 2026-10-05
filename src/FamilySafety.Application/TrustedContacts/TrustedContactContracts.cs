namespace FamilySafety.Application.TrustedContacts;

public sealed record TrustedContactView(Guid TrustedContactId, string DisplayName, string? PhoneNumber,
    string? EmailAddress, string? RelationshipLabel, Guid Version);
public sealed record SaveTrustedContactRequest(string DisplayName, string? PhoneNumber, string? EmailAddress, string? RelationshipLabel);
public sealed record EditTrustedContactRequest(Guid Version, string DisplayName, string? PhoneNumber, string? EmailAddress, string? RelationshipLabel);
public sealed class TrustedContactUnavailableException : Exception;
public sealed class TrustedContactConflictException : Exception;
public sealed class TrustedContactDuplicateException : Exception;
public sealed class TrustedContactValidationException(IDictionary<string, string[]> errors) : Exception
{ public IDictionary<string, string[]> Errors { get; } = errors; }

public interface ITrustedContactRepository
{
    Task<IReadOnlyList<TrustedContactView>?> ListAsync(Guid accountId, CancellationToken ct);
    Task<TrustedContactView?> ReadAsync(Guid accountId, Guid contactId, CancellationToken ct);
    Task<TrustedContactView?> CreateAsync(Guid accountId, SaveTrustedContactRequest request, CancellationToken ct);
    Task<TrustedContactView?> EditAsync(Guid accountId, Guid contactId, EditTrustedContactRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid accountId, Guid contactId, Guid version, CancellationToken ct);
}
