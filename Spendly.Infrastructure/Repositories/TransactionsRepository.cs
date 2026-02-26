using Microsoft.EntityFrameworkCore;
using Spendly.Data.Entities;
using Spendly.Infrastructure.Interfaces;

namespace Spendly.Infrastructure.Repositories;

// TODO: Consider about db querying approach
public class TransactionsRepository(SpendlyDbContext dbContext) : ITransactionsRepository
{
    private DbSet<TransactionEntity> Transactions => dbContext.Transactions;

    public void UpdateTransaction(TransactionEntity entity)
        => Transactions.Update(entity);

    public void AddTransaction(TransactionEntity entity)
        => Transactions.Add(entity);

    public void DeleteTransaction(TransactionEntity entity)
        => Transactions.Remove(entity);

    public async Task<int> Count(CancellationToken ct)
        => await Transactions.CountAsync(ct);

    public Task<bool> ExistsAsync(Guid transactionId)
        => Transactions.AnyAsync(x => x.Id == transactionId);
}