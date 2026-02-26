namespace Spendly.Application.Models.Dashboard;

public sealed record MonthPoint(DateOnly MonthStart, decimal Income, decimal Expense);