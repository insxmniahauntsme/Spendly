using Spendly.Data.Entities;

namespace Spendly.Infrastructure.Interfaces;

public interface IAccountsRepository
{
    void UpdateAccount(AccountEntity entity);
    void AddAccount(AccountEntity entity);
    void DeleteAccount(AccountEntity entity);
    Task<bool> ExistsAsync(Guid accountId);
}