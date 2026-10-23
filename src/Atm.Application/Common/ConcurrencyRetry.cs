using Atm.Application.Abstractions;
using Atm.Domain.Accounts;
using Atm.Domain.Common;

namespace Atm.Application.Common;

/// <summary>
/// Runs a state-changing operation, retrying it once from fresh data if another
/// operation changed the same account first. A second conflict becomes a Conflict error.
/// </summary>
public static class ConcurrencyRetry
{
    private const int MaxAttempts = 2;

    public static async Task<Result<T>> ExecuteAsync<T>(
        IUnitOfWork unitOfWork,
        Func<CancellationToken, Task<Result<T>>> attempt,
        CancellationToken ct)
    {
        for (var attemptNumber = 1; attemptNumber <= MaxAttempts; attemptNumber++)
        {
            try
            {
                return await attempt(ct);
            }
            catch (ConcurrencyConflictException)
            {
                unitOfWork.DiscardChanges();
            }
        }

        return AccountErrors.ConcurrencyConflict;
    }
}
