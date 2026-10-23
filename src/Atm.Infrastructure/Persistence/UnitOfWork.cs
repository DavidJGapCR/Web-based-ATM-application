using Atm.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Atm.Infrastructure.Persistence;

internal sealed class UnitOfWork(AtmDbContext db) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            // SaveChanges wraps all pending inserts/updates in one database transaction.
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException(ex);
        }
    }

    public void DiscardChanges() => db.ChangeTracker.Clear();
}
