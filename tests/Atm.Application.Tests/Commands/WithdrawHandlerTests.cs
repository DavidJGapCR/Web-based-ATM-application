using Atm.Application.Commands.Withdraw;
using Atm.Application.Tests.Fakes;
using Atm.Domain.Accounts;

namespace Atm.Application.Tests.Commands;

public class WithdrawHandlerTests
{
    [Test]
    public async Task Withdraw_Success_SavesOnce()
    {
        var uow = new FakeUnitOfWork();
        var handler = new WithdrawHandler(
            new FakeAccountRepository(new Account(1, "Checking", 100m)), new FakeTransactionRepository(), uow, new FixedTimeProvider());

        var result = await handler.HandleAsync(new WithdrawCommand(1, 40m), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value.Account.Balance, Is.EqualTo(60m));
            Assert.That(result.Value.Transaction.Type, Is.EqualTo("Withdrawal"));
            Assert.That(uow.SaveCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Withdraw_InsufficientFunds_DoesNotSave()
    {
        var uow = new FakeUnitOfWork();
        var transactions = new FakeTransactionRepository();
        var handler = new WithdrawHandler(
            new FakeAccountRepository(new Account(1, "Checking", 10m)), transactions, uow, new FixedTimeProvider());

        var result = await handler.HandleAsync(new WithdrawCommand(1, 40m), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Error, Is.EqualTo(AccountErrors.InsufficientFunds));
            Assert.That(transactions.Added, Is.Empty);
            Assert.That(uow.SaveCount, Is.EqualTo(0));
        });
    }
}
