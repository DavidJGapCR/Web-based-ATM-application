using Atm.Application.Abstractions;
using Atm.Domain.Accounts;

namespace Atm.Application.Tests.Fakes;

internal sealed class FakeTransactionRepository : ITransactionRepository
{
    public List<AccountTransaction> Added { get; } = new();

    public void Add(AccountTransaction tx) => Added.Add(tx);

    public Task<IReadOnlyList<AccountTransaction>> GetByAccountAsync(int accountId, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<AccountTransaction>>(
            Added.Where(t => t.AccountId == accountId).OrderByDescending(t => t.CreatedAt).ToList());
}
