using Atm.Application.Abstractions;
using Atm.Application.Accounts;
using Atm.Application.Common;
using Atm.Domain.Accounts;
using Atm.Domain.Common;

namespace Atm.Application.Commands.Deposit;

public sealed class DepositHandler(
    IAccountRepository accounts,
    ITransactionRepository transactions,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<DepositCommand, AccountOperationResult>
{
    public Task<Result<AccountOperationResult>> HandleAsync(DepositCommand command, CancellationToken ct) =>
        ConcurrencyRetry.ExecuteAsync(unitOfWork, token => DepositAsync(command, token), ct);

    private async Task<Result<AccountOperationResult>> DepositAsync(DepositCommand command, CancellationToken ct)
    {
        var account = await accounts.GetByIdAsync(command.AccountId, ct);
        if (account is null)
        {
            return AccountErrors.NotFound(command.AccountId);
        }

        var deposit = account.Deposit(command.Amount, timeProvider.GetUtcNow().UtcDateTime);
        if (deposit.IsFailure)
        {
            return deposit.Error;
        }

        transactions.Add(deposit.Value);
        await unitOfWork.SaveChangesAsync(ct);

        return new AccountOperationResult(account.ToDto(), deposit.Value.ToDto());
    }
}
