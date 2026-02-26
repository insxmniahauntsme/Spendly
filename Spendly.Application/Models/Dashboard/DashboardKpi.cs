namespace Spendly.Application.Models.Dashboard;

public sealed record DashboardKpi(
    decimal MonthlyIncome,
    decimal MonthlyExpense,
    double? IncomeChangePct,
    double? ExpenseChangePct
)
{
    public decimal Balance => MonthlyIncome - MonthlyExpense;

    public double? BalancePctOfIncome =>
        MonthlyIncome == 0 ? null : (double)(Balance / MonthlyIncome);
}