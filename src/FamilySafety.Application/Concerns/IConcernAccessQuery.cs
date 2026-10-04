namespace FamilySafety.Application.Concerns;

/// <summary>
/// Read-only evaluation of current Concern access under REQ-PRIV-025, for Incident lineage only.
/// It applies the Concern's effective lifecycle and expiry exactly as Concern reads do, but persists
/// nothing and writes no Concern audit or denied summary. It changes no Concern permission.
/// </summary>
public interface IConcernAccessQuery
{
    /// <summary>True when the caller could currently read the Concern (creator or valid participant grant).</summary>
    Task<bool> CanReadAsync(Guid accountId, Guid concernId, CancellationToken cancellationToken);

    /// <summary>
    /// True only when the caller is the Concern's creator with current creator access, the Concern is
    /// in the given Family Group, and it has not effectively expired. Used for create-from-Concern.
    /// </summary>
    Task<bool> IsCurrentCreatorAsync(Guid accountId, Guid concernId, Guid familyGroupId,
        CancellationToken cancellationToken);
}
