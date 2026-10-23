using Atm.Domain.Accounts;

namespace Atm.Application.Abstractions;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(int id, CancellationToken ct);

    Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken ct);
}
