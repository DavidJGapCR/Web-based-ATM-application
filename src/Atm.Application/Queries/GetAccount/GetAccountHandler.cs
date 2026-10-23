using Atm.Application.Abstractions;
using Atm.Application.Accounts;
using Atm.Domain.Accounts;
using Atm.Domain.Common;

namespace Atm.Application.Queries.GetAccount;

public sealed class GetAccountHandler(IAccountRepository accounts)
    : IQueryHandler<GetAccountQuery, AccountDto>
{
    public async Task<Result<AccountDto>> HandleAsync(GetAccountQuery query, CancellationToken ct)
    {
        var account = await accounts.GetByIdAsync(query.AccountId, ct);
        return account is null
            ? AccountErrors.NotFound(query.AccountId)
            : account.ToDto();
    }
}
