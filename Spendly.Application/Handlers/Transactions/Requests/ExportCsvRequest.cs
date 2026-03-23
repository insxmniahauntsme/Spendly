using MediatR;
using Spendly.Domain.Enums;

namespace Spendly.Application.Handlers.Transactions.Requests;

public record ExportCsvRequest(
    string FileName,
    TransactionType? Type,
    DateOnly? Date,
    Guid? CategoryId,
    Guid? AccountId,
    string? SearchTerm) : IRequest<string>;