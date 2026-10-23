namespace Atm.Application.Commands.Transfer;

public sealed record TransferCommand(int FromAccountId, int ToAccountId, decimal Amount);
