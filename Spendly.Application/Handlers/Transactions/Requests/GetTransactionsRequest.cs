using MediatR;
using Spendly.Application.Models;
using Spendly.Data.Enums;
using Spendly.Domain.Enums;
using Spendly.Domain.Models;

namespace Spendly.Application.Handlers.Transactions.Requests;

public sealed record GetTransactionsRequest(
    TransactionType? Type,
    DateOnly? Month,
    Guid? CategoryId,
    Guid? AccountId,
    string? SearchTerm) 
: PagedRequest, IRequest<TransactionsPageData>;