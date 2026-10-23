using Atm.Application.Commands.Deposit;
using Atm.Application.Tests.Fakes;
using Atm.Domain.Accounts;
using Atm.Domain.Common;

namespace Atm.Application.Tests.Commands;

public class ConcurrencyRetryTests
{
    private static (DepositHandler Handler, FakeUnitOfWork Uow) CreateHandler(int conflicts)
    {
        var uow = new FakeUnitOfWork { ConflictsToThrow = conflicts };
        var handler = new DepositHandler(
            new FakeAccountRepository(new Account(1, "Checking")), new FakeTransactionRepository(), uow, new FixedTimeProvider());
        return (handler, uow);
    }

    [Test]
    public async Task OneConflict_RetriesAndSucceeds()
    {
        var (handler, uow) = CreateHandler(conflicts: 1);

        var result = await handler.HandleAsync(new DepositCommand(1, 10m), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(uow.SaveCount, Is.EqualTo(2));
            Assert.That(uow.DiscardCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task TwoConflicts_ReturnsConflict()
    {
        var (handler, uow) = CreateHandler(conflicts: 2);

        var result = await handler.HandleAsync(new DepositCommand(1, 10m), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Error.Kind, Is.EqualTo(ErrorKind.Conflict));
            Assert.That(uow.SaveCount, Is.EqualTo(2));
            Assert.That(uow.DiscardCount, Is.EqualTo(2));
        });
    }
}
