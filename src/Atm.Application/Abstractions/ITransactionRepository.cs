using Atm.Domain.Accounts;

namespace Atm.Application.Abstractions;

public interface ITransactionRepository
{
    void Add(AccountTransaction tx);

    /// <summary>Returns the account's transactions, newest first.</summary>
    Task<IReadOnlyList<AccountTransaction>> GetByAccountAsync(int accountId, CancellationToken ct);
}
