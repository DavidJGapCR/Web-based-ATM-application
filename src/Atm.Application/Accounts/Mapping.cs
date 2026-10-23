using Atm.Domain.Accounts;

namespace Atm.Application.Accounts;

public static class Mapping
{
    public static AccountDto ToDto(this Account account) =>
        new(account.Id, account.Name, account.Balance);

    public static TransactionDto ToDto(this AccountTransaction tx) =>
        new(tx.Id, tx.AccountId, tx.Type.ToString(), tx.Amount, tx.BalanceAfter, tx.TransferId, tx.CreatedAt);
}
