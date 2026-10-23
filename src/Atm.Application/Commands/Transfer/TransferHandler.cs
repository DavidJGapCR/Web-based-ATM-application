using Atm.Application.Abstractions;
using Atm.Application.Accounts;
using Atm.Application.Common;
using Atm.Domain.Accounts;
using Atm.Domain.Common;
using Atm.Domain.Transfers;

namespace Atm.Application.Commands.Transfer;

public sealed class TransferHandler(
    IAccountRepository accounts,
    ITransactionRepository transactions,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<TransferCommand, TransferResult>
{
    public Task<Result<TransferResult>> HandleAsync(TransferCommand command, CancellationToken ct)
    {
        if (command.FromAccountId == command.ToAccountId)
        {
            return Task.FromResult<Result<TransferResult>>(AccountErrors.SameAccountTransfer);
        }

        return ConcurrencyRetry.ExecuteAsync(unitOfWork, token => TransferAsync(command, token), ct);
    }

    private async Task<Result<TransferResult>> TransferAsync(TransferCommand command, CancellationToken ct)
    {
        var from = await accounts.GetByIdAsync(command.FromAccountId, ct);
        if (from is null)
        {
            return AccountErrors.NotFound(command.FromAccountId);
        }

        var to = await accounts.GetByIdAsync(command.ToAccountId, ct);
        if (to is null)
        {
            return AccountErrors.NotFound(command.ToAccountId);
        }

        var transfer = FundsTransfer.Execute(from, to, command.Amount, timeProvider.GetUtcNow().UtcDateTime);
        if (transfer.IsFailure)
        {
            return transfer.Error;
        }

        transactions.Add(transfer.Value.Debit);
        transactions.Add(transfer.Value.Credit);

        // Both balance changes and both history rows commit together.
        await unitOfWork.SaveChangesAsync(ct);

        return new TransferResult(transfer.Value.TransferId, from.ToDto(), to.ToDto());
    }
}
