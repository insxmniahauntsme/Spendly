namespace Spendly.Application.Analytics.Dashboard;

public static class MonthKpiCalculator
{
    public static double? CalculateAmountChangePct(decimal current, decimal previous)
    {
        if (previous == 0) return null;
        return (double)((current - previous) / previous);
    }
}