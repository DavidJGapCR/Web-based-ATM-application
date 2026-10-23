using System.Net;
using System.Net.Http.Json;
using Atm.Application.Accounts;

namespace Atm.Api.Tests;

public class TransferEndpointTests : ApiTestBase
{
    [Test]
    public async Task Transfer_Returns200_AndBothHistoriesShareTransferId()
    {
        await Client.PostAsJsonAsync("/api/accounts/1/deposits", new { amount = 100m });

        var response = await Client.PostAsJsonAsync("/api/transfers",
            new { fromAccountId = 1, toAccountId = 2, amount = 40m });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<TransferResult>();
        var checkingTop = (await GetTransactionsAsync(1))[0];
        var savingsTop = (await GetTransactionsAsync(2))[0];
        Assert.Multiple(() =>
        {
            Assert.That(body!.From, Is.EqualTo(new AccountDto(1, "Checking", 60m)));
            Assert.That(body.To, Is.EqualTo(new AccountDto(2, "Savings", 40m)));
            Assert.That(checkingTop.Type, Is.EqualTo("TransferOut"));
            Assert.That(savingsTop.Type, Is.EqualTo("TransferIn"));
            Assert.That(checkingTop.TransferId, Is.EqualTo(body.TransferId));
            Assert.That(savingsTop.TransferId, Is.EqualTo(body.TransferId));
        });
    }

    [Test]
    public async Task Transfer_SameAccount_Returns400()
    {
        var response = await Client.PostAsJsonAsync("/api/transfers",
            new { fromAccountId = 1, toAccountId = 1, amount = 10m });

        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(await ReadErrorCodeAsync(response), Is.EqualTo("transfer.same_account"));
        });
    }

    [Test]
    public async Task Transfer_Insufficient_Returns422_NoBalanceChange()
    {
        var response = await Client.PostAsJsonAsync("/api/transfers",
            new { fromAccountId = 1, toAccountId = 2, amount = 10m });

        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
            Assert.That((await GetAccountAsync(2)).Balance, Is.EqualTo(0m));
            Assert.That(await GetTransactionsAsync(2), Is.Empty);
        });
    }

    [TestCase(999, 2)]
    [TestCase(1, 999)]
    public async Task Transfer_UnknownAccount_Returns404_NoBalanceChange(int from, int to)
    {
        await Client.PostAsJsonAsync("/api/accounts/1/deposits", new { amount = 100m });

        var response = await Client.PostAsJsonAsync("/api/transfers",
            new { fromAccountId = from, toAccountId = to, amount = 10m });

        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That((await GetAccountAsync(1)).Balance, Is.EqualTo(100m));
            Assert.That((await GetAccountAsync(2)).Balance, Is.EqualTo(0m));
        });
    }
}
