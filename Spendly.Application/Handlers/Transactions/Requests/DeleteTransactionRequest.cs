using MediatR;

namespace Spendly.Application.Handlers.Transactions.Requests;

public sealed record DeleteTransactionRequest(Guid Id) : IRequest;