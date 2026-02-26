using Spendly.Infrastructure.Interfaces;

namespace Spendly.Infrastructure.Repositories;

public class UnitOfWork(SpendlyDbContext dbContext) : IUnitOfWork
{
    public async Task CompleteAsync(CancellationToken ct)
        => await dbContext.SaveChangesAsync(ct);
}