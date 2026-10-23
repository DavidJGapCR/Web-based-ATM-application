using Atm.Application.Commands.Deposit;
using Atm.Application.Tests.Fakes;
using Atm.Domain.Accounts;
using Atm.Domain.Common;

namespace Atm.Application.Tests.Commands;

public class DepositHandlerTests
{
    private FakeTransactionRepository _transactions = null!;
    private FakeUnitOfWork _uow = null!;
    private DepositHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _transactions = new FakeTransactionRepository();
        _uow = new FakeUnitOfWork();
        _handler = new DepositHandler(
            new FakeAccountRepository(new Account(1, "Checking", 100m)), _transactions, _uow, new FixedTimeProvider());
    }

    [Test]
    public async Task Deposit_Success_SavesOnce_AndAddsTransaction()
    {
        var result = await _handler.HandleAsync(new DepositCommand(1, 25m), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value.Account.Balance, Is.EqualTo(125m));
            Assert.That(result.Value.Transaction.Type, Is.EqualTo("Deposit"));
            Assert.That(result.Value.Transaction.CreatedAt, Is.EqualTo(FixedTimeProvider.Default.UtcDateTime));
            Assert.That(_transactions.Added, Has.Count.EqualTo(1));
            Assert.That(_uow.SaveCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Deposit_UnknownAccount_ReturnsNotFound_AndDoesNotSave()
    {
        var result = await _handler.HandleAsync(new DepositCommand(999, 25m), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Error.Kind, Is.EqualTo(ErrorKind.NotFound));
            Assert.That(_uow.SaveCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task Deposit_InvalidAmount_DoesNotSave()
    {
        var result = await _handler.HandleAsync(new DepositCommand(1, -1m), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Error, Is.EqualTo(AccountErrors.InvalidAmount));
            Assert.That(_transactions.Added, Is.Empty);
            Assert.That(_uow.SaveCount, Is.EqualTo(0));
        });
    }
}
