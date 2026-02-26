using Microsoft.EntityFrameworkCore;
using Spendly.Data.Entities;
using Spendly.Infrastructure.Interfaces;

namespace Spendly.Infrastructure.Repositories;

public class AccountsRepository(SpendlyDbContext dbContext) : IAccountsRepository
{
    private DbSet<AccountEntity> Accounts => dbContext.Accounts;

    public void UpdateAccount(AccountEntity entity)
        => Accounts.Update(entity);

    public void AddAccount(AccountEntity entity)
        => Accounts.Add(entity);

    public void DeleteAccount(AccountEntity entity)
        => Accounts.Remove(entity);
    
    public Task<bool> ExistsAsync(Guid accountId)
        => Accounts.AnyAsync(x => x.Id == accountId);
}