namespace Spendly.Application.Handlers.Analytics;

public record AnalyticsTrendPointItem(
	DateOnly Month,
	decimal Amount);