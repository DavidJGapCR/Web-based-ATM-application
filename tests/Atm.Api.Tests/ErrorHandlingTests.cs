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
