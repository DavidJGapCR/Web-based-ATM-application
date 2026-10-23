using Atm.Application.Queries.GetAccount;
using Atm.Application.Queries.GetAccounts;
using Atm.Application.Queries.GetAccountTransactions;
using Atm.Application.Tests.Fakes;
using Atm.Domain.Accounts;

namespace Atm.Application.Tests.Queries;

public class QueryHandlerTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Test]
    public async Task GetAccounts_ReturnsAllOrderedById()
    {
        var accounts = new FakeAccountRepository(new Account(2, "Savings", 5m), new Account(1, "Checking", 10m));
        var handler = new GetAccountsHandler(accounts);

        var result = await handler.HandleAsync(new GetAccountsQuery(), CancellationToken.None);

        Assert.That(result.Value.Select(a => (a.Id, a.Name, a.Balance)), Is.EqualTo(new[]
        {
            (1, "Checking", 10m),
            (2, "Savings", 5m)
        }));
    }

    [Test]
    public async Task GetAccount_Known_ReturnsDto()
    {
        var handler = new GetAccountHandler(new FakeAccountRepository(new Account(1, "Checking", 10m)));

        var result = await handler.HandleAsync(new GetAccountQuery(1), CancellationToken.None);

        Assert.That(result.Value, Is.EqualTo(new Accounts.AccountDto(1, "Checking", 10m)));
    }

    [Test]
    public async Task GetAccount_Unknown_ReturnsNotFound()
    {
        var handler = new GetAccountHandler(new FakeAccountRepository());

        var result = await handler.HandleAsync(new GetAccountQuery(999), CancellationToken.None);

        Assert.That(result.Error.Code, Is.EqualTo("account.not_found"));
    }

    [Test]
    public async Task GetAccountTransactions_Unknown_ReturnsNotFound()
    {
        var handler = new GetAccountTransactionsHandler(new FakeAccountRepository(), new FakeTransactionRepository());

        var result = await handler.HandleAsync(new GetAccountTransactionsQuery(999), CancellationToken.None);

        Assert.That(result.Error.Code, Is.EqualTo("account.not_found"));
    }

    [Test]
    public async Task GetAccountTransactions_ReturnsDtos()
    {
        var account = new Account(1, "Checking");
        var transactions = new FakeTransactionRepository();
        var tx = account.Deposit(10m, Now).Value;
        transactions.Add(tx);
        var handler = new GetAccountTransactionsHandler(new FakeAccountRepository(account), transactions);

        var result = await handler.HandleAsync(new GetAccountTransactionsQuery(1), CancellationToken.None);

        Assert.That(result.Value, Is.EqualTo(new[]
        {
            new Accounts.TransactionDto(tx.Id, 1, "Deposit", 10m, 10m, null, Now)
        }));
    }
}
