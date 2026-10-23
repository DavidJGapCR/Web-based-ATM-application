using Atm.Application.Abstractions;
using Atm.Application.Accounts;
using Atm.Application.Common;
using Atm.Domain.Accounts;
using Atm.Domain.Common;

namespace Atm.Application.Commands.Withdraw;

public sealed class WithdrawHandler(
    IAccountRepository accounts,
    ITransactionRepository transactions,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<WithdrawCommand, AccountOperationResult>
{
    public Task<Result<AccountOperationResult>> HandleAsync(WithdrawCommand command, CancellationToken ct) =>
        ConcurrencyRetry.ExecuteAsync(unitOfWork, token => WithdrawAsync(command, token), ct);

    private async Task<Result<AccountOperationResult>> WithdrawAsync(WithdrawCommand command, CancellationToken ct)
    {
        var account = await accounts.GetByIdAsync(command.AccountId, ct);
        if (account is null)
        {
            return AccountErrors.NotFound(command.AccountId);
        }

        var withdrawal = account.Withdraw(command.Amount, timeProvider.GetUtcNow().UtcDateTime);
        if (withdrawal.IsFailure)
        {
            return withdrawal.Error;
        }

        transactions.Add(withdrawal.Value);
        await unitOfWork.SaveChangesAsync(ct);

        return new AccountOperationResult(account.ToDto(), withdrawal.Value.ToDto());
    }
}
