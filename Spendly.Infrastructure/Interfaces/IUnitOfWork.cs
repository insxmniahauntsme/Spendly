namespace Spendly.Infrastructure.Interfaces;

public interface IUnitOfWork
{
    Task CompleteAsync(CancellationToken ct = default);
}