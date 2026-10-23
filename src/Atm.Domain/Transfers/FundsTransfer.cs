using Atm.Domain.Accounts;
using Atm.Domain.Common;

namespace Atm.Domain.Transfers;

/// <summary>
/// Moves money between two accounts. Both legs are validated before either account changes,
/// so a failed transfer never leaves one side applied.
/// </summary>
public static class FundsTransfer
{
    public static Result<TransferLegs> Execute(Account from, Account to, decimal amount, DateTime utcNow)
    {
        if (from.Id == to.Id)
        {
            return AccountErrors.SameAccountTransfer;
        }

        var canDebit = from.CanDebit(amount);
        if (canDebit.IsFailure)
        {
            return canDebit.Error;
        }

        var canCredit = to.CanCredit(amount);
        if (canCredit.IsFailure)
        {
            return canCredit.Error;
        }

        var transferId = Guid.NewGuid();
        var debit = from.Debit(amount, TransactionType.TransferOut, transferId, utcNow).Value;
        var credit = to.Credit(amount, TransactionType.TransferIn, transferId, utcNow).Value;

        return new TransferLegs(transferId, debit, credit);
    }
}
