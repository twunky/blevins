using FamilySafety.Application.TrustedContacts;
using FamilySafety.Domain.TrustedContacts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Infrastructure.Persistence;

public sealed class EfTrustedContactRepository(FamilySafetyDbContext db) : ITrustedContactRepository
{
    // Authorization stays in the SQL predicate, before contact content is loaded. No Family/admin queries.
    private IQueryable<TrustedContact> Owned(Guid accountId) => db.TrustedContacts.Where(c =>
        db.AdultProfiles.Any(p => p.Id == c.OwnerProfileId && p.AccountId == accountId));

    private static IQueryable<TrustedContactView> Projection(IQueryable<TrustedContact> rows) => rows.AsNoTracking()
        .OrderBy(c => c.DisplayName).ThenBy(c => c.Id)
        .Select(c => new TrustedContactView(c.Id, c.DisplayName, c.PhoneNumber, c.EmailAddress, c.RelationshipLabel, c.Version));

    public async Task<IReadOnlyList<TrustedContactView>?> ListAsync(Guid accountId, CancellationToken ct)
    {
        if (!await db.AdultProfiles.AnyAsync(p => p.AccountId == accountId, ct)) return null;
        return await Projection(Owned(accountId)).ToListAsync(ct);
    }
    public Task<TrustedContactView?> ReadAsync(Guid accountId, Guid contactId, CancellationToken ct) =>
        Projection(Owned(accountId).Where(c => c.Id == contactId)).SingleOrDefaultAsync(ct);

    public async Task<TrustedContactView?> CreateAsync(Guid accountId, SaveTrustedContactRequest request, CancellationToken ct)
    {
        var owner = await db.AdultProfiles.Where(p => p.AccountId == accountId).Select(p => (Guid?)p.Id).SingleOrDefaultAsync(ct);
        if (owner is null) return null;
        var fields = TrustedContactFields.Validate(request.DisplayName, request.PhoneNumber, request.EmailAddress, request.RelationshipLabel);
        var row = new TrustedContact(Guid.NewGuid(), owner.Value, fields.DisplayName, fields.PhoneNumber,
            fields.NormalizedPhone, fields.EmailAddress, fields.NormalizedEmail, fields.RelationshipLabel);
        db.TrustedContacts.Add(row);
        await SaveAsync(ct);
        return View(row);
    }

    public async Task<TrustedContactView?> EditAsync(Guid accountId, Guid contactId, EditTrustedContactRequest request, CancellationToken ct)
    {
        Detach(contactId);
        var row = await Owned(accountId).SingleOrDefaultAsync(c => c.Id == contactId, ct);
        if (row is null) return null;
        if (row.Version != request.Version) throw new TrustedContactConflictException();
        var fields = TrustedContactFields.Validate(request.DisplayName, request.PhoneNumber, request.EmailAddress, request.RelationshipLabel);
        row.Edit(fields.DisplayName, fields.PhoneNumber, fields.NormalizedPhone, fields.EmailAddress, fields.NormalizedEmail, fields.RelationshipLabel);
        await SaveAsync(ct);
        return View(row);
    }

    public async Task<bool> DeleteAsync(Guid accountId, Guid contactId, Guid version, CancellationToken ct)
    {
        Detach(contactId);
        var row = await Owned(accountId).SingleOrDefaultAsync(c => c.Id == contactId, ct);
        if (row is null) return false;
        if (row.Version != version) throw new TrustedContactConflictException();
        db.TrustedContacts.Remove(row);
        await SaveAsync(ct);
        return true;
    }

    private void Detach(Guid id)
    {
        var tracked = db.ChangeTracker.Entries<TrustedContact>().FirstOrDefault(e => e.Entity.Id == id);
        if (tracked is not null) tracked.State = EntityState.Detached;
    }
    private async Task SaveAsync(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { throw new TrustedContactConflictException(); }
        catch (DbUpdateException e) when (e.InnerException is SqlException { Number: 2601 or 2627 })
        { throw new TrustedContactDuplicateException(); }
    }
    private static TrustedContactView View(TrustedContact row) =>
        new(row.Id, row.DisplayName, row.PhoneNumber, row.EmailAddress, row.RelationshipLabel, row.Version);
}
