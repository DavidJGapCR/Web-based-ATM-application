using Atm.Domain.Accounts;

namespace Atm.Domain.Tests.Accounts;

public class AccountTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Test]
    public void Deposit_IncreasesBalance_AndRecordsTransaction()
    {
        var account = new Account(1, "Checking", 100m);

        var result = account.Deposit(25.50m, Now);

        Assert.That(result.IsSuccess, Is.True);
        var tx = result.Value;
        Assert.Multiple(() =>
        {
            Assert.That(account.Balance, Is.EqualTo(125.50m));
            Assert.That(account.Version, Is.EqualTo(1));
            Assert.That(tx.AccountId, Is.EqualTo(1));
            Assert.That(tx.Type, Is.EqualTo(TransactionType.Deposit));
            Assert.That(tx.Amount, Is.EqualTo(25.50m));
            Assert.That(tx.BalanceAfter, Is.EqualTo(125.50m));
            Assert.That(tx.TransferId, Is.Null);
            Assert.That(tx.CreatedAt, Is.EqualTo(Now));
            Assert.That(tx.Id, Is.Not.EqualTo(Guid.Empty));
        });
    }

    [Test]
    public void Withdraw_DecreasesBalance()
    {
        var account = new Account(1, "Checking", 100m);

        var result = account.Withdraw(40m, Now);

        Assert.Multiple(() =>
        {
            Assert.That(account.Balance, Is.EqualTo(60m));
            Assert.That(result.Value.Type, Is.EqualTo(TransactionType.Withdrawal));
            Assert.That(result.Value.BalanceAfter, Is.EqualTo(60m));
        });
    }

    [Test]
    public void Withdraw_ExactBalance_Succeeds()
    {
        var account = new Account(1, "Checking", 100m);

        var result = account.Withdraw(100m, Now);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(account.Balance, Is.EqualTo(0m));
        });
    }

    [Test]
    public void Withdraw_MoreThanBalance_ReturnsInsufficientFunds()
    {
        var account = new Account(1, "Checking", 100m);

        var result = account.Withdraw(100.01m, Now);

        Assert.Multiple(() =>
        {
            Assert.That(result.Error, Is.EqualTo(AccountErrors.InsufficientFunds));
            Assert.That(account.Balance, Is.EqualTo(100m));
            Assert.That(account.Version, Is.EqualTo(0));
        });
    }

    [TestCase(0)]
    [TestCase(-5)]
    public void Deposit_NonPositive_ReturnsInvalidAmount(decimal amount)
    {
        var account = new Account(1, "Checking", 100m);

        var result = account.Deposit(amount, Now);

        Assert.Multiple(() =>
        {
            Assert.That(result.Error, Is.EqualTo(AccountErrors.InvalidAmount));
            Assert.That(account.Balance, Is.EqualTo(100m));
            Assert.That(account.Version, Is.EqualTo(0));
        });
    }

    [TestCase(0)]
    [TestCase(-5)]
    public void Withdraw_NonPositive_ReturnsInvalidAmount(decimal amount)
    {
        var account = new Account(1, "Checking", 100m);

        var result = account.Withdraw(amount, Now);

        Assert.That(result.Error, Is.EqualTo(AccountErrors.InvalidAmount));
    }

    [Test]
    public void Deposit_SubCent_ReturnsTooManyDecimals()
    {
        var account = new Account(1, "Checking");

        var result = account.Deposit(0.001m, Now);

        Assert.Multiple(() =>
        {
            Assert.That(result.Error, Is.EqualTo(AccountErrors.TooManyDecimals));
            Assert.That(account.Balance, Is.EqualTo(0m));
        });
    }

    [Test]
    public void Deposit_TrailingZeroScale_IsAccepted()
    {
        var account = new Account(1, "Checking");

        var result = account.Deposit(10.100m, Now);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(account.Balance, Is.EqualTo(10.1m));
        });
    }

    [Test]
    public void Deposit_Overflow_ReturnsAmountTooLarge()
    {
        var account = new Account(1, "Checking", decimal.MaxValue - 1m);

        var result = account.Deposit(2m, Now);

        Assert.Multiple(() =>
        {
            Assert.That(result.Error, Is.EqualTo(AccountErrors.AmountTooLarge));
            Assert.That(account.Balance, Is.EqualTo(decimal.MaxValue - 1m));
        });
    }
}
