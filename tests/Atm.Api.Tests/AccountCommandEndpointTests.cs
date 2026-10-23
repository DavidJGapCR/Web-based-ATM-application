using System.Net;
using System.Net.Http.Json;
using Atm.Application.Accounts;

namespace Atm.Api.Tests;

public class AccountCommandEndpointTests : ApiTestBase
{
    [Test]
    public async Task Deposit_Returns200_AndUpdatesBalanceAndHistory()
    {
        var response = await Client.PostAsJsonAsync("/api/accounts/1/deposits", new { amount = 100m });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<AccountOperationResult>();
        var history = await GetTransactionsAsync(1);
        Assert.Multiple(() =>
        {
            Assert.That(body!.Account.Balance, Is.EqualTo(100m));
            Assert.That(body.Transaction.Type, Is.EqualTo("Deposit"));
            Assert.That(history.Select(t => (t.Type, t.Amount, t.BalanceAfter)),
                Is.EqualTo(new[] { ("Deposit", 100m, 100m) }));
        });
    }

    [Test]
    public async Task Withdraw_Returns200_AndUpdatesBalance()
    {
        await Client.PostAsJsonAsync("/api/accounts/1/deposits", new { amount = 100m });

        var response = await Client.PostAsJsonAsync("/api/accounts/1/withdrawals", new { amount = 30.25m });

        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That((await GetAccountAsync(1)).Balance, Is.EqualTo(69.75m));
        });
    }

    [Test]
    public async Task Withdraw_Insufficient_Returns422()
    {
        var response = await Client.PostAsJsonAsync("/api/accounts/1/withdrawals", new { amount = 10m });

        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
            Assert.That(await ReadErrorCodeAsync(response), Is.EqualTo("account.insufficient_funds"));
            Assert.That((await GetAccountAsync(1)).Balance, Is.EqualTo(0m));
        });
    }

    [TestCase("0", "amount.invalid")]
    [TestCase("-1", "amount.invalid")]
    [TestCase("0.001", "amount.too_many_decimals")]
    public async Task Deposit_InvalidAmount_Returns400(string amount, string expectedCode)
    {
        var response = await PostRawJsonAsync("/api/accounts/1/deposits", $$"""{"amount":{{amount}}}""");

        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(await ReadErrorCodeAsync(response), Is.EqualTo(expectedCode));
        });
    }

    [Test]
    public async Task Deposit_UnknownAccount_Returns404()
    {
        var response = await Client.PostAsJsonAsync("/api/accounts/999/deposits", new { amount = 10m });

        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(await ReadErrorCodeAsync(response), Is.EqualTo("account.not_found"));
        });
    }

    [Test]
    public async Task Withdraw_UnknownAccount_Returns404()
    {
        var response = await Client.PostAsJsonAsync("/api/accounts/999/withdrawals", new { amount = 10m });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
