using Spendly.Domain.Enums;

namespace Spendly.Application.Models.Transactions;

public record CreateTransactionModel(
    Guid AccountId,
    Guid? CategoryId,
    decimal Amount,
    DateTime DateUtc,
    string Comment,
    TransactionType Type);