using Spendly.Domain.Models;

namespace Spendly.Application.Models.Dashboard;

public sealed record DashboardData(
    DashboardKpi Kpi,
    IReadOnlyList<Transaction> Transactions,
    IReadOnlyList<MonthPoint> Last6Months,
    IReadOnlyList<CategorySlice> Categories
);