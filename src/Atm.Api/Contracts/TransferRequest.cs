namespace Atm.Api.Contracts;

public sealed record TransferRequest(int FromAccountId, int ToAccountId, decimal Amount);
