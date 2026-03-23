using MediatR;
using Microsoft.EntityFrameworkCore;
using Spendly.Application.Handlers.Transactions.Requests;
using Spendly.Application.Mappers;
using Spendly.Infrastructure;

namespace Spendly.Application.Handlers.Transactions;

public class UpdateTransactionHandler(SpendlyDbContext dbContext) : IRequestHandler<UpdateTransactionRequest>
{
    public async Task Handle(UpdateTransactionRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Transactions.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        entity?.UpdateEntity(request.Model);
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}