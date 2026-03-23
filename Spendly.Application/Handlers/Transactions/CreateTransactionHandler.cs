using MediatR;
using Spendly.Application.Handlers.Transactions.Requests;
using Spendly.Application.Mappers;
using Spendly.Infrastructure;

namespace Spendly.Application.Handlers.Transactions;

public class CreateTransactionHandler(SpendlyDbContext dbContext) : IRequestHandler<CreateTransactionRequest>
{
    public async Task Handle(CreateTransactionRequest request, CancellationToken cancellationToken)
    {
        var entity = request.Model.ToEntity();
        
        dbContext.Transactions.Add(entity);
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}