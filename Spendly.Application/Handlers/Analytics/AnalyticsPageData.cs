namespace Spendly.Application.Handlers.Analytics;

public record AnalyticsPageData
{
    public AnalyticsTopSectionData TopSection { get; init; } = new();
    public AnalyticsTrendSectionData TrendSection { get; init; } = new();
    public AnalyticsAccountsSectionData AccountsSection { get; init; } = new();
}