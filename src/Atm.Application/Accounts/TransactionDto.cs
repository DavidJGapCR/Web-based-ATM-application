namespace Atm.Application.Accounts;

public sealed record TransactionDto(
    Guid Id,
    int AccountId,
    string Type,
    decimal Amount,
    decimal BalanceAfter,
    Guid? TransferId,
    DateTime CreatedAt);
