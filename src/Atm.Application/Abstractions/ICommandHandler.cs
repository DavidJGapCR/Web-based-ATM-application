using Atm.Domain.Common;

namespace Atm.Application.Abstractions;

/// <summary>
/// Handles one state-changing use case.
/// </summary>
public interface ICommandHandler<in TCommand, TResult>
{
    Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken ct);
}
