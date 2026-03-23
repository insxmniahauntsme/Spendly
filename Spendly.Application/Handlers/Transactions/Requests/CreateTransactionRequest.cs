using MediatR;
using Spendly.Application.Models.Transactions;

namespace Spendly.Application.Handlers.Transactions.Requests;

public sealed record CreateTransactionRequest(CreateTransactionModel Model) : IRequest;