namespace Atm.Application.Commands.Deposit;

public sealed record DepositCommand(int AccountId, decimal Amount);
