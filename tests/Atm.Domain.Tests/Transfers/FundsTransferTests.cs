using Atm.Domain.Accounts;
using Atm.Domain.Transfers;

namespace Atm.Domain.Tests.Transfers;

public class FundsTransferTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Test]
    public void Execute_MovesMoney_AndLinksLegs()
    {
        var from = new Account(1, "Checking", 100m);
        var to = new Account(2, "Savings");

        var result = FundsTransfer.Execute(from, to, 30m, Now);

        Assert.That(result.IsSuccess, Is.True);
        var legs = result.Value;
        Assert.Multiple(() =>
        {
            Assert.That(from.Balance, Is.EqualTo(70m));
            Assert.That(to.Balance, Is.EqualTo(30m));
            Assert.That(legs.TransferId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(legs.Debit.Type, Is.EqualTo(TransactionType.TransferOut));
            Assert.That(legs.Credit.Type, Is.EqualTo(TransactionType.TransferIn));
            Assert.That(legs.Debit.TransferId, Is.EqualTo(legs.TransferId));
            Assert.That(legs.Credit.TransferId, Is.EqualTo(legs.TransferId));
            Assert.That(legs.Debit.AccountId, Is.EqualTo(1));
            Assert.That(legs.Credit.AccountId, Is.EqualTo(2));
            Assert.That(legs.Debit.BalanceAfter, Is.EqualTo(70m));
            Assert.That(legs.Credit.BalanceAfter, Is.EqualTo(30m));
        });
    }

    [Test]
    public void Execute_SameAccount_ReturnsSameAccountTransfer()
    {
        var account = new Account(1, "Checking", 100m);

        var result = FundsTransfer.Execute(account, account, 30m, Now);

        Assert.Multiple(() =>
        {
            Assert.That(result.Error, Is.EqualTo(AccountErrors.SameAccountTransfer));
            Assert.That(account.Balance, Is.EqualTo(100m));
        });
    }

    [Test]
    public void Execute_InsufficientFunds_LeavesBothAccountsUnchanged()
    {
        var from = new Account(1, "Checking", 10m);
        var to = new Account(2, "Savings");

        var result = FundsTransfer.Execute(from, to, 20m, Now);

        Assert.Multiple(() =>
        {
            Assert.That(result.Error, Is.EqualTo(AccountErrors.InsufficientFunds));
            Assert.That(from.Balance, Is.EqualTo(10m));
            Assert.That(to.Balance, Is.EqualTo(0m));
            Assert.That(from.Version, Is.EqualTo(0));
            Assert.That(to.Version, Is.EqualTo(0));
        });
    }

    [Test]
    public void Execute_DestinationOverflow_LeavesSourceUnchanged()
    {
        var from = new Account(1, "Checking", 10m);
        var to = new Account(2, "Savings", decimal.MaxValue - 1m);

        var result = FundsTransfer.Execute(from, to, 5m, Now);

        Assert.Multiple(() =>
        {
            Assert.That(result.Error, Is.EqualTo(AccountErrors.AmountTooLarge));
            Assert.That(from.Balance, Is.EqualTo(10m));
            Assert.That(from.Version, Is.EqualTo(0));
        });
    }

    [Test]
    public void Execute_InvalidAmount_ReturnsInvalidAmount()
    {
        var from = new Account(1, "Checking", 100m);
        var to = new Account(2, "Savings");

        var result = FundsTransfer.Execute(from, to, 0m, Now);

        Assert.That(result.Error, Is.EqualTo(AccountErrors.InvalidAmount));
    }
}
