using Microsoft.EntityFrameworkCore;
using Spendly.Data.Entities;

namespace Spendly.Infrastructure.Queries;

public sealed class AccountQueries(SpendlyDbContext dbContext)
{
    public async Task<List<AccountEntity>> GetAccountsAsync()
    {
        return await dbContext.Accounts
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .ToListAsync();
    }
}