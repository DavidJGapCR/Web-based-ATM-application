using Atm.Application.Abstractions;
using Atm.Application.Accounts;
using Atm.Domain.Common;

namespace Atm.Application.Queries.GetAccounts;

public sealed class GetAccountsHandler(IAccountRepository accounts)
    : IQueryHandler<GetAccountsQuery, IReadOnlyList<AccountDto>>
{
    public async Task<Result<IReadOnlyList<AccountDto>>> HandleAsync(GetAccountsQuery query, CancellationToken ct)
    {
        var all = await accounts.GetAllAsync(ct);
        return all.OrderBy(a => a.Id).Select(a => a.ToDto()).ToList();
    }
}
