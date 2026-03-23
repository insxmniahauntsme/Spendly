using Spendly.Application.Models;
using Spendly.Domain.Models;

namespace Spendly.Application.Handlers.Transactions;

public sealed record TransactionsPageData(
    PagedResponse<Transaction> Page,
    decimal TotalExpenses,
    decimal TotalIncomes);