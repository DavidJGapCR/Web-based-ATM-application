using Atm.Application.Abstractions;
using Atm.Application.Accounts;
using Atm.Domain.Accounts;
using Atm.Domain.Common;

namespace Atm.Application.Queries.GetAccountTransactions;

public sealed class GetAccountTransactionsHandler(IAccountRepository accounts, ITransactionRepository transactions)
    : IQueryHandler<GetAccountTransactionsQuery, IReadOnlyList<TransactionDto>>
{
    public async Task<Result<IReadOnlyList<TransactionDto>>> HandleAsync(GetAccountTransactionsQuery query, CancellationToken ct)
    {
        if (await accounts.GetByIdAsync(query.AccountId, ct) is null)
        {
            return AccountErrors.NotFound(query.AccountId);
        }

        var history = await transactions.GetByAccountAsync(query.AccountId, ct);
        return history.Select(t => t.ToDto()).ToList();
    }
}
