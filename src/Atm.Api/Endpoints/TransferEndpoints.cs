using Atm.Api.Contracts;
using Atm.Api.Errors;
using Atm.Application.Abstractions;
using Atm.Application.Accounts;
using Atm.Application.Commands.Transfer;

namespace Atm.Api.Endpoints;

public static class TransferEndpoints
{
    public static IEndpointRouteBuilder MapTransferEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/transfers", async (
                TransferRequest request,
                ICommandHandler<TransferCommand, TransferResult> handler,
                CancellationToken ct) =>
            (await handler.HandleAsync(new TransferCommand(request.FromAccountId, request.ToAccountId, request.Amount), ct))
                .ToHttpResult(TypedResults.Ok))
            .WithTags("Transfers")
            .WithName("Transfer");

        return app;
    }
}
