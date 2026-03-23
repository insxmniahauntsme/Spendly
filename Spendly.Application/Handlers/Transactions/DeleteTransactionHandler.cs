using MediatR;
using Microsoft.EntityFrameworkCore;
using Spendly.Application.Handlers.Transactions.Requests;
using Spendly.Infrastructure;

namespace Spendly.Application.Handlers.Transactions;

public class DeleteTransactionHandler(SpendlyDbContext dbContext) : IRequestHandler<DeleteTransactionRequest>
{
    public async Task Handle(DeleteTransactionRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Transactions.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null) return;

        dbContext.Transactions.Remove(entity);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}