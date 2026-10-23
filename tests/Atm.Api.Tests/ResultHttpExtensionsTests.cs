using Atm.Api.Errors;
using Atm.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Atm.Api.Tests;

public class ResultHttpExtensionsTests
{
    [TestCase(ErrorKind.Validation, StatusCodes.Status400BadRequest)]
    [TestCase(ErrorKind.NotFound, StatusCodes.Status404NotFound)]
    [TestCase(ErrorKind.InsufficientFunds, StatusCodes.Status422UnprocessableEntity)]
    [TestCase(ErrorKind.Conflict, StatusCodes.Status409Conflict)]
    public void ToProblem_MapsErrorKindToStatusAndCode(ErrorKind kind, int expectedStatus)
    {
        var result = (ProblemHttpResult)new Error("some.code", "Some message.", kind).ToProblem();

        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(expectedStatus));
            Assert.That(result.ProblemDetails.Title, Is.EqualTo("Some message."));
            Assert.That(result.ProblemDetails.Extensions["code"], Is.EqualTo("some.code"));
        });
    }
}
