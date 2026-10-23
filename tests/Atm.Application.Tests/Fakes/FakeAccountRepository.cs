using Atm.Application.Abstractions;
using Atm.Domain.Accounts;

namespace Atm.Application.Tests.Fakes;

internal sealed class FakeAccountRepository : IAccountRepository
{
    private readonly Dictionary<int, Account> _accounts = new();

    public FakeAccountRepository(params Account[] accounts)
    {
        foreach (var account in accounts)
        {
            _accounts[account.Id] = account;
        }
    }

    public Task<Account?> GetByIdAsync(int id, CancellationToken ct) =>
        Task.FromResult(_accounts.GetValueOrDefault(id));

    public Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Account>>(_accounts.Values.ToList());
}
