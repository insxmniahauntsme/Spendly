namespace Spendly.Application.Handlers.Budgets;

public record BudgetsPageData(
	decimal TotalBudget,
	decimal TotalSpend,
	IEnumerable<CategoryLimitItem> Items)
{
	public decimal Remaining => TotalBudget - TotalSpend;
};