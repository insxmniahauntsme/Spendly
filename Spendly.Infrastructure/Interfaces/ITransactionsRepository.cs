using Spendly.Data.Entities;

namespace Spendly.Infrastructure.Interfaces;

public interface ITransactionsRepository
{
    void UpdateTransaction(TransactionEntity entity);
    void AddTransaction(TransactionEntity entity);
    void DeleteTransaction(TransactionEntity entity);
    Task<int> Count(CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid transactionId);
}