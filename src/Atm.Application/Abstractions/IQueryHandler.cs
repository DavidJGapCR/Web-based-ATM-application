using Atm.Domain.Common;

namespace Atm.Application.Abstractions;

/// <summary>
/// Handles one read-only use case.
/// </summary>
public interface IQueryHandler<in TQuery, TResult>
{
    Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken ct);
}
