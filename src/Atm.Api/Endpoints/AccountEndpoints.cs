using Atm.Api.Errors;
using Atm.Application.Abstractions;
using Atm.Application.Accounts;
using Atm.Application.Queries.GetAccount;
using Atm.Application.Queries.GetAccounts;
using Atm.Application.Queries.GetAccountTransactions;

namespace Atm.Api.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/accounts").WithTags("Accounts");

        group.MapGet("/", async (
                IQueryHandler<GetAccountsQuery, IReadOnlyList<AccountDto>> handler,
                CancellationToken ct) =>
            (await handler.HandleAsync(new GetAccountsQuery(), ct)).ToHttpResult(TypedResults.Ok))
            .WithName("GetAccounts");

        group.MapGet("/{id:int}", async (
                int id,
                IQueryHandler<GetAccountQuery, AccountDto> handler,
                CancellationToken ct) =>
            (await handler.HandleAsync(new GetAccountQuery(id), ct)).ToHttpResult(TypedResults.Ok))
            .WithName("GetAccount");

        group.MapGet("/{id:int}/transactions", async (
                int id,
                IQueryHandler<GetAccountTransactionsQuery, IReadOnlyList<TransactionDto>> handler,
                CancellationToken ct) =>
            (await handler.HandleAsync(new GetAccountTransactionsQuery(id), ct)).ToHttpResult(TypedResults.Ok))
            .WithName("GetAccountTransactions");

        return app;
    }
}
