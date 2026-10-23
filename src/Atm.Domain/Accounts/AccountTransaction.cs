namespace Atm.Domain.Accounts;

/// <summary>
/// An immutable entry in an account's history.
/// </summary>
public sealed class AccountTransaction
{
    // Required by EF Core.
    private AccountTransaction()
    {
    }

    public Guid Id { get; private set; }

    public int AccountId { get; private set; }

    public TransactionType Type { get; private set; }

    /// <summary>Always positive; <see cref="Type"/> gives the direction.</summary>
    public decimal Amount { get; private set; }

    public decimal BalanceAfter { get; private set; }

    /// <summary>Shared by both legs of a transfer; null otherwise.</summary>
    public Guid? TransferId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    internal static AccountTransaction Create(
        int accountId,
        TransactionType type,
        decimal amount,
        decimal balanceAfter,
        Guid? transferId,
        DateTime createdAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Type = type,
            Amount = amount,
            BalanceAfter = balanceAfter,
            TransferId = transferId,
            CreatedAt = createdAt
        };
}
