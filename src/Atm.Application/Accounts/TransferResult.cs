namespace Atm.Application.Accounts;

public sealed record TransferResult(Guid TransferId, AccountDto From, AccountDto To);
