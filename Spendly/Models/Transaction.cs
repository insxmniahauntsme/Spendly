using Spendly.Data.Enums;
using Spendly.Domain.Enums;

namespace Spendly.Models;

public record Transaction(
    Guid Id,
    Guid AccountId,
    Guid? CategoryId,
    decimal Amount,
    DateTime DateUtc,
    TransactionType Type,
    string Comment,
    string AccountName,
    string CategoryName,
    string CategoryIconSource,
    string AmountText,
    string DateText);