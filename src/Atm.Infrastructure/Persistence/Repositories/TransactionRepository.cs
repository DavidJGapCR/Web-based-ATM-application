using Atm.Application.Abstractions;
using Atm.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Atm.Infrastructure.Persistence.Repositories;

internal sealed class TransactionRepository(AtmDbContext db) : ITransactionRepository
{
    public void Add(AccountTransaction tx) => db.Transactions.Add(tx);

    public async Task<IReadOnlyList<AccountTransaction>> GetByAccountAsync(int accountId, CancellationToken ct) =>
        await db.Transactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
}
