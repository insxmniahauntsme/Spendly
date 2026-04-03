namespace Spendly.Application.Handlers.Analytics;

public record AnalyticsBudgetData(
	bool HasData,
	Guid? CategoryId,
	string CategoryName,
	decimal Current,
	decimal Limit,
	double Progress)
{
	public static AnalyticsBudgetData Empty => new(false, null, "", 0, 0, 0);
}