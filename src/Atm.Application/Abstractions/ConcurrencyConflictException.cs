namespace Atm.Application.Abstractions;

/// <summary>
/// Raised by <see cref="IUnitOfWork"/> when an optimistic concurrency check fails.
/// Keeps the application layer independent of the persistence technology's exception types.
/// </summary>
public sealed class ConcurrencyConflictException(Exception inner)
    : Exception("A concurrency conflict occurred while saving changes.", inner);
