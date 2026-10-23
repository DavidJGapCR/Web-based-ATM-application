using System.Net.Http.Json;
using System.Text.Json;
using Atm.Application.Accounts;

namespace Atm.Api.Tests;

/// <summary>
/// Gives each test a fresh API instance and database.
/// </summary>
public abstract class ApiTestBase
{
    protected AtmApiFactory Factory { get; private set; } = null!;

    protected HttpClient Client { get; private set; } = null!;

    [SetUp]
    public void CreateApi()
    {
        Factory = new AtmApiFactory();
        Client = Factory.CreateClient();
    }

    [TearDown]
    public void DisposeApi()
    {
        Client.Dispose();
        Factory.Dispose();
    }

    protected async Task<AccountDto> GetAccountAsync(int id) =>
        (await Client.GetFromJsonAsync<AccountDto>($"/api/accounts/{id}"))!;

    protected async Task<List<TransactionDto>> GetTransactionsAsync(int id) =>
        (await Client.GetFromJsonAsync<List<TransactionDto>>($"/api/accounts/{id}/transactions"))!;

    protected Task<HttpResponseMessage> PostRawJsonAsync(string url, string json) =>
        Client.PostAsync(url, new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

    protected static async Task<string?> ReadErrorCodeAsync(HttpResponseMessage response)
    {
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.TryGetProperty("code", out var code) ? code.GetString() : null;
    }
}
