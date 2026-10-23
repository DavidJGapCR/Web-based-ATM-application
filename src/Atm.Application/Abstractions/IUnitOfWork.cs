namespace Atm.Application.Abstractions;

/// <summary>
/// Commits all pending changes of the current operation atomically.
/// </summary>
public interface IUnitOfWork
{
    /// <exception cref="ConcurrencyConflictException">Another operation changed the same data first.</exception>
    Task SaveChangesAsync(CancellationToken ct);

    /// <summary>Forgets pending changes and loaded state so the operation can be retried from fresh data.</summary>
    void DiscardChanges();
}
