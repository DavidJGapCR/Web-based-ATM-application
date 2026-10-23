using System.Net;
using System.Net.Http.Json;
using Atm.Application.Accounts;

namespace Atm.Api.Tests;

public class AccountQueryEndpointTests : ApiTestBase
{
    [Test]
    public async Task GetAccounts_ReturnsSeededAccounts()
    {
        var accounts = await Client.GetFromJsonAsync<List<AccountDto>>("/api/accounts");

        Assert.That(accounts, Is.EqualTo(new[]
        {
            new AccountDto(1, "Checking", 0m),
            new AccountDto(2, "Savings", 0m)
        }));
    }

    [Test]
    public async Task GetAccount_Known_ReturnsAccount()
    {
        var account = await GetAccountAsync(2);

        Assert.That(account, Is.EqualTo(new AccountDto(2, "Savings", 0m)));
    }

    [Test]
    public async Task GetAccount_Unknown_Returns404ProblemDetails()
    {
        var response = await Client.GetAsync("/api/accounts/999");

        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
            Assert.That(await ReadErrorCodeAsync(response), Is.EqualTo("account.not_found"));
        });
    }

    [Test]
    public async Task GetTransactions_NewAccount_ReturnsEmptyList()
    {
        var transactions = await GetTransactionsAsync(1);

        Assert.That(transactions, Is.Empty);
    }

    [Test]
    public async Task GetTransactions_Unknown_Returns404()
    {
        var response = await Client.GetAsync("/api/accounts/999/transactions");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
