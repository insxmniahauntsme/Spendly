using Spendly.Data.Enums;
using Spendly.Domain.Enums;

namespace Spendly.Application.Models.Transactions;

public sealed record UpdateTransactionModel(
    decimal Amount,
    DateTime DateUtc,
    string Comment,
    TransactionType Type);