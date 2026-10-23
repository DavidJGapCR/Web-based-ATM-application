using Atm.Domain.Common;

namespace Atm.Domain.Accounts;

/// <summary>
/// A bank account. All balance changes go through this entity so its rules cannot be bypassed.
/// </summary>
public sealed class Account
{
    // Required by EF Core.
    private Account()
    {
        Name = string.Empty;
    }

    public Account(int id, string name, decimal balance = 0m)
    {
        Id = id;
        Name = name;
        Balance = balance;
    }

    public int Id { get; private set; }

    public string Name { get; private set; }

    public decimal Balance { get; private set; }

    /// <summary>Optimistic concurrency token; incremented on every balance change.</summary>
    public int Version { get; private set; }

    public Result<AccountTransaction> Deposit(decimal amount, DateTime utcNow) =>
        Credit(amount, TransactionType.Deposit, transferId: null, utcNow);

    public Result<AccountTransaction> Withdraw(decimal amount, DateTime utcNow) =>
        Debit(amount, TransactionType.Withdrawal, transferId: null, utcNow);

    internal Result CanCredit(decimal amount)
    {
        var validation = AmountRules.Validate(amount);
        if (validation.IsFailure)
        {
            return validation;
        }

        return amount > decimal.MaxValue - Balance
            ? AccountErrors.AmountTooLarge
            : Result.Success();
    }

    internal Result CanDebit(decimal amount)
    {
        var validation = AmountRules.Validate(amount);
        if (validation.IsFailure)
        {
            return validation;
        }

        return Balance < amount
            ? AccountErrors.InsufficientFunds
            : Result.Success();
    }

    internal Result<AccountTransaction> Credit(decimal amount, TransactionType type, Guid? transferId, DateTime utcNow)
    {
        var check = CanCredit(amount);
        if (check.IsFailure)
        {
            return check.Error;
        }

        return Apply(Balance + amount, amount, type, transferId, utcNow);
    }

    internal Result<AccountTransaction> Debit(decimal amount, TransactionType type, Guid? transferId, DateTime utcNow)
    {
        var check = CanDebit(amount);
        if (check.IsFailure)
        {
            return check.Error;
        }

        return Apply(Balance - amount, amount, type, transferId, utcNow);
    }

    private AccountTransaction Apply(decimal newBalance, decimal amount, TransactionType type, Guid? transferId, DateTime utcNow)
    {
        Balance = newBalance;
        Version++;
        return AccountTransaction.Create(Id, type, amount, Balance, transferId, utcNow);
    }
}
