using Atm.Application.Accounts;
using Atm.Application.Commands.Transfer;
using Atm.Application.Tests.Fakes;
using Atm.Domain.Accounts;
using Atm.Domain.Common;

namespace Atm.Application.Tests.Commands;

public class TransferHandlerTests
{
    private Account _checking = null!;
    private FakeTransactionRepository _transactions = null!;
    private FakeUnitOfWork _uow = null!;
    private TransferHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _checking = new Account(1, "Checking", 100m);
        _transactions = new FakeTransactionRepository();
        _uow = new FakeUnitOfWork();
        _handler = new TransferHandler(
            new FakeAccountRepository(_checking, new Account(2, "Savings")), _transactions, _uow, new FixedTimeProvider());
    }

    [Test]
    public async Task Transfer_Success_AddsTwoLinkedTransactions_SavesOnce()
    {
        var result = await _handler.HandleAsync(new TransferCommand(1, 2, 40m), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value.From, Is.EqualTo(new AccountDto(1, "Checking", 60m)));
            Assert.That(result.Value.To, Is.EqualTo(new AccountDto(2, "Savings", 40m)));
            Assert.That(_transactions.Added, Has.Count.EqualTo(2));
            Assert.That(_transactions.Added.Select(t => t.TransferId), Is.All.EqualTo(result.Value.TransferId));
            Assert.That(_uow.SaveCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Transfer_SameAccount_ReturnsValidation()
    {
        var result = await _handler.HandleAsync(new TransferCommand(1, 1, 40m), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Error, Is.EqualTo(AccountErrors.SameAccountTransfer));
            Assert.That(_uow.SaveCount, Is.EqualTo(0));
        });
    }

    [TestCase(999, 2)]
    [TestCase(1, 999)]
    public async Task Transfer_UnknownAccount_ReturnsNotFound_NoSave(int from, int to)
    {
        var result = await _handler.HandleAsync(new TransferCommand(from, to, 40m), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Error.Kind, Is.EqualTo(ErrorKind.NotFound));
            Assert.That(result.Error.Message, Does.Contain("999"));
            Assert.That(_checking.Balance, Is.EqualTo(100m));
            Assert.That(_uow.SaveCount, Is.EqualTo(0));
        });
    }
}
