using Atm.Application.Abstractions;

namespace Atm.Application.Tests.Fakes;

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    /// <summary>Counts every SaveChangesAsync call, including ones that throw.</summary>
    public int SaveCount { get; private set; }

    public int DiscardCount { get; private set; }

    /// <summary>Number of upcoming SaveChangesAsync calls that throw a concurrency conflict.</summary>
    public int ConflictsToThrow { get; set; }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        SaveCount++;
        if (ConflictsToThrow > 0)
        {
            ConflictsToThrow--;
            throw new ConcurrencyConflictException(new InvalidOperationException("simulated conflict"));
        }

        return Task.CompletedTask;
    }

    public void DiscardChanges() => DiscardCount++;
}
