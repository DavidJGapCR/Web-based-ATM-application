using System.Net;
using System.Net.Http.Json;

namespace Atm.Api.Tests;

public class ConcurrencyTests : ApiTestBase
{
    [Test]
    public async Task ConcurrentWithdrawals_NeverOverdraw()
    {
        await Client.PostAsJsonAsync("/api/accounts/1/deposits", new { amount = 100m });

        var responses = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ =>
            Client.PostAsJsonAsync("/api/accounts/1/withdrawals", new { amount = 30m })));

        var statuses = responses.Select(r => r.StatusCode).ToList();
        var succeeded = statuses.Count(s => s == HttpStatusCode.OK);
        var balance = (await GetAccountAsync(1)).Balance;
        var withdrawals = (await GetTransactionsAsync(1)).Count(t => t.Type == "Withdrawal");

        Assert.Multiple(() =>
        {
            Assert.That(statuses, Is.All.AnyOf(HttpStatusCode.OK, HttpStatusCode.Conflict, HttpStatusCode.UnprocessableEntity));
            Assert.That(succeeded, Is.InRange(1, 3));
            Assert.That(balance, Is.EqualTo(100m - 30m * succeeded));
            Assert.That(balance, Is.GreaterThanOrEqualTo(0m));
            Assert.That(withdrawals, Is.EqualTo(succeeded));
        });
    }
}
