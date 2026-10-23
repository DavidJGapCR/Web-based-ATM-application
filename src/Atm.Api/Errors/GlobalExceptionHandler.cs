using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Atm.Api.Errors;

/// <summary>
/// Last line of defense: turns exceptions into ProblemDetails without leaking internals.
/// Expected business failures never reach here; they travel as Results.
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var problem = exception switch
        {
            // Malformed JSON, wrong types or a missing body.
            BadHttpRequestException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "The request body is invalid.",
                Extensions = { ["code"] = "request.invalid" }
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Extensions = { ["code"] = "server.error" }
            }
        };

        if (problem.Status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception processing {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = problem.Status!.Value;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }
}
