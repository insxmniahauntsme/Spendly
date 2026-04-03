namespace Spendly.Application.Handlers.Analytics;

public record AnalyticsAccountItem(
	string Name,
	decimal Amount,
	double Share,
	int Rank);