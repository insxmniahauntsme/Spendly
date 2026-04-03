namespace Spendly.Application.Handlers.Analytics;

public record AnalyticsRiskItem(
	Guid CategoryId,
	double Progress,
	string CategoryName = "");
