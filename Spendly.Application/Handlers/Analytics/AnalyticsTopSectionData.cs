namespace Spendly.Application.Handlers.Analytics;

public record AnalyticsTopSectionData(
	AnalyticsBudgetData Overspent, 
	AnalyticsBudgetData Underused,
	List<AnalyticsRiskItem> RiskItems, 
	decimal ForecastAmount,
	bool HasForecast);