using Atm.Application.Abstractions;
using Atm.Application.Accounts;
using Atm.Application.Commands.Deposit;
using Atm.Application.Commands.Transfer;
using Atm.Application.Commands.Withdraw;
using Atm.Application.Queries.GetAccount;
using Atm.Application.Queries.GetAccounts;
using Atm.Application.Queries.GetAccountTransactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Atm.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<ICommandHandler<DepositCommand, AccountOperationResult>, DepositHandler>();
        services.AddScoped<ICommandHandler<WithdrawCommand, AccountOperationResult>, WithdrawHandler>();
        services.AddScoped<ICommandHandler<TransferCommand, TransferResult>, TransferHandler>();

        services.AddScoped<IQueryHandler<GetAccountsQuery, IReadOnlyList<AccountDto>>, GetAccountsHandler>();
        services.AddScoped<IQueryHandler<GetAccountQuery, AccountDto>, GetAccountHandler>();
        services.AddScoped<IQueryHandler<GetAccountTransactionsQuery, IReadOnlyList<TransactionDto>>, GetAccountTransactionsHandler>();

        return services;
    }
}
