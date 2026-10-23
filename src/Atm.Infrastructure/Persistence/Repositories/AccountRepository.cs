using Atm.Application.Abstractions;
using Atm.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Atm.Infrastructure.Persistence.Repositories;

internal sealed class AccountRepository(AtmDbContext db) : IAccountRepository
{
    public Task<Account?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Accounts.SingleOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken ct) =>
        await db.Accounts.AsNoTracking().OrderBy(a => a.Id).ToListAsync(ct);
}
