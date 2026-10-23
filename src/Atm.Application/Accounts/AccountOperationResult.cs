namespace Atm.Application.Accounts;

public sealed record AccountOperationResult(AccountDto Account, TransactionDto Transaction);
