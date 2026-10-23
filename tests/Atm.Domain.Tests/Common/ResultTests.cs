using Atm.Domain.Common;

namespace Atm.Domain.Tests.Common;

public class ResultTests
{
    private static readonly Error SampleError = new("x", "m", ErrorKind.Validation);

    [Test]
    public void Success_HasValue()
    {
        Result<int> result = 5;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Value, Is.EqualTo(5));
        });
    }

    [Test]
    public void Failure_FromError()
    {
        Result<int> result = SampleError;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("x"));
        });
    }

    [Test]
    public void Value_OnFailure_Throws()
    {
        Result<int> result = SampleError;

        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Test]
    public void Error_OnSuccess_Throws()
    {
        Result<int> result = 5;

        Assert.Throws<InvalidOperationException>(() => _ = result.Error);
    }

    [Test]
    public void NonGenericFailure_FromError()
    {
        Result result = SampleError;

        Assert.That(result.Error, Is.EqualTo(SampleError));
    }
}
