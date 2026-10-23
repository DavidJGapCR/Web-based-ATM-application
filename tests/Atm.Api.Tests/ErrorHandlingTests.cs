using System.Net;

namespace Atm.Api.Tests;

public class ErrorHandlingTests : ApiTestBase
{
    [TestCase("""{"amount":"abc"}""")]
    [TestCase("""{"amount":""")]
    [TestCase("")]
    public async Task MalformedBody_Returns400ProblemDetails(string body)
    {
        var response = await PostRawJsonAsync("/api/accounts/1/deposits", body);

        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
            Assert.That(await ReadErrorCodeAsync(response), Is.EqualTo("request.invalid"));
        });
    }

    [TestCase("/api/accounts/1/deposits", "{}")]
    [TestCase("/api/transfers", """{"amount":5}""")]
    [TestCase("/api/transfers", """{"fromAccountId":1,"amount":5}""")]
    public async Task MissingRequiredFields_Returns400RequestInvalid(string url, string body)
    {
        var response = await PostRawJsonAsync(url, body);

        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(await ReadErrorCodeAsync(response), Is.EqualTo("request.invalid"));
        });
    }

    [Test]
    public async Task UnsupportedContentType_Returns415ProblemDetails()
    {
        var response = await Client.PostAsync("/api/accounts/1/deposits",
            new StringContent("""{"amount":5}""", System.Text.Encoding.UTF8, "text/plain"));

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnsupportedMediaType));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
        });
    }

    [Test]
    public async Task UnmatchedRoute_Returns404ProblemDetails()
    {
        var response = await Client.GetAsync("/api/accounts/abc");

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
        });
    }

    [Test]
    public async Task MalformedTransferBody_Returns400ProblemDetails()
    {
        var response = await PostRawJsonAsync("/api/transfers", """{"fromAccountId":"one"}""");

        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(await ReadErrorCodeAsync(response), Is.EqualTo("request.invalid"));
        });
    }
}
