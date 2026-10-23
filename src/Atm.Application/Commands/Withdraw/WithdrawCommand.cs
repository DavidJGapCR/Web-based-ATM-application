namespace Atm.Application.Commands.Withdraw;

public sealed record WithdrawCommand(int AccountId, decimal Amount);
