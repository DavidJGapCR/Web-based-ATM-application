using Atm.Domain.Common;

namespace Atm.Api.Errors;

/// <summary>
/// The single place where business outcomes become HTTP responses.
/// </summary>
public static class ResultHttpExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value) : result.Error.ToProblem();

    public static IResult ToProblem(this Error error) =>
        TypedResults.Problem(
            statusCode: ToStatusCode(error.Kind),
            title: error.Message,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });

    private static int ToStatusCode(ErrorKind kind) => kind switch
    {
        ErrorKind.Validation => StatusCodes.Status400BadRequest,
        ErrorKind.NotFound => StatusCodes.Status404NotFound,
        ErrorKind.InsufficientFunds => StatusCodes.Status422UnprocessableEntity,
        ErrorKind.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };
}
