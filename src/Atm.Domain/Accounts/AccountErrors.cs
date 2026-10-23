using Atm.Domain.Common;

namespace Atm.Domain.Accounts;

/// <summary>
/// Catalog of the expected business failures for accounts and transfers.
/// </summary>
public static class AccountErrors
{
    public static readonly Error InvalidAmount =
        new("amount.invalid", "Amount must be greater than zero.", ErrorKind.Validation);

    public static readonly Error TooManyDecimals =
        new("amount.too_many_decimals", "Amount cannot have more than 2 decimal places.", ErrorKind.Validation);

    public static readonly Error AmountTooLarge =
        new("amount.too_large", "Amount is too large.", ErrorKind.Validation);

    public static readonly Error InsufficientFunds =
        new("account.insufficient_funds", "Insufficient funds.", ErrorKind.InsufficientFunds);

    public static readonly Error SameAccountTransfer =
        new("transfer.same_account", "Cannot transfer to the same account.", ErrorKind.Validation);

    public static readonly Error ConcurrencyConflict =
        new("account.concurrency_conflict", "The account was modified by another operation. Please try again.", ErrorKind.Conflict);

    public static Error NotFound(int accountId) =>
        new("account.not_found", $"Account {accountId} was not found.", ErrorKind.NotFound);
}
