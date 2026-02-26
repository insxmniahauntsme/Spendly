using MediatR;
using Microsoft.EntityFrameworkCore;
using Spendly.Application.Analytics.Dashboard;
using Spendly.Application.Handlers.Dashboard.Requests;
using Spendly.Application.Models.Dashboard;
using Spendly.Data.Enums;
using Spendly.Domain.Models;
using Spendly.Infrastructure;

namespace Spendly.Application.Handlers.Dashboard;

// TODO: Consider about refactoring entire dashboard backend (architecture, db queries, unified dashboard data source)
public sealed class GetDashboardDataHandler(
    SpendlyDbContext dbContext)
    : IRequestHandler<GetDashboardDataRequest, DashboardData>
{
    public async Task<DashboardData> Handle(GetDashboardDataRequest request, CancellationToken ct)
    {
        var currentStart = new DateOnly(request.Year, request.Month, 1);
        var currentEnd = currentStart.AddMonths(1);
        var prevStart = currentStart.AddMonths(-1);
        var halfYearAgo = currentStart.AddMonths(-5);
        
        var fromUtc = DateTime.SpecifyKind(halfYearAgo.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var toUtc = DateTime.SpecifyKind(currentEnd.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

        var transactions = await dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.DateUtc >= fromUtc && t.DateUtc < toUtc)
            .Select(t => new Transaction
            {
                Id = t.Id,
                AccountId = t.AccountId,
                CategoryId = t.CategoryId,
                Amount = t.Amount,
                DateUtc = t.DateUtc,
                Type = t.Type,
                Comment = t.Comment
            })
            .ToListAsync(ct);

        var kpi = CalculateKpiData(transactions, currentStart, currentEnd, prevStart);

        var barData = CalculateBarChartData(transactions, currentEnd, halfYearAgo);
        var donutData = await CalculateDonutChartData(request, ct);

        return new DashboardData(kpi.data, kpi.transactions, barData, donutData);
    }

    private static (DashboardKpi data, IReadOnlyList<Transaction> transactions) CalculateKpiData(
        IReadOnlyList<Transaction> transactions,
        DateOnly currentStart,
        DateOnly currentEnd,
        DateOnly prevStart)
    {
        var prevTx = transactions.Where(t =>
            DateOnly.FromDateTime(t.DateUtc) >= prevStart && DateOnly.FromDateTime(t.DateUtc) < currentStart).ToList();
        var currTx = transactions.Where(t =>
            DateOnly.FromDateTime(t.DateUtc) >= currentStart && DateOnly.FromDateTime(t.DateUtc) < currentEnd).ToList();

        var (currIncome, currExpense) = SumMonth(currTx);
        var (prevIncome, prevExpense) = SumMonth(prevTx);

        var incomeChangePct = MonthKpiCalculator.CalculateAmountChangePct(currIncome, prevIncome);
        var expenseChangePct = MonthKpiCalculator.CalculateAmountChangePct(currExpense, prevExpense);

        return new ValueTuple<DashboardKpi, IReadOnlyList<Transaction>>(
            new DashboardKpi(currIncome, currExpense, incomeChangePct, expenseChangePct), currTx);
    }

    private static IReadOnlyList<MonthPoint> CalculateBarChartData(
        IReadOnlyList<Transaction> transactions,
        DateOnly currentEnd,
        DateOnly halfYearAgo)
    {
        var months = Enumerable.Range(0, 6)
            .Select(halfYearAgo.AddMonths)
            .ToList();

        var filtered = transactions.Where(t =>
        {
            var d = DateOnly.FromDateTime(t.DateUtc);
            return d >= halfYearAgo && d < currentEnd;
        });

        var agg = filtered
            .GroupBy(t =>
            {
                var d = DateOnly.FromDateTime(t.DateUtc);
                return (d.Year, d.Month);
            })
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    Income = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    Expense = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
                });

        return months
            .Select(m =>
            {
                var key = (m.Year, m.Month);
                agg.TryGetValue(key, out var v);

                return new MonthPoint(
                    MonthStart: m,
                    Income: v?.Income ?? 0m,
                    Expense: v?.Expense ?? 0m
                );
            })
            .ToList();
    }

    private async Task<IReadOnlyList<CategorySlice>> CalculateDonutChartData(
        GetDashboardDataRequest request,
        CancellationToken ct)
    {
        var start = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1);

        var rows = await dbContext.Transactions
            .AsNoTracking()
            .Where(t => 
                t.Type == TransactionType.Expense &&
                t.DateUtc >= start && t.DateUtc < end &&
                t.CategoryId != null)
            .Select(t => new
            {
                t.Category!.Name,
                t.Amount
            })
            .ToListAsync(ct);

        return rows
            .GroupBy(x => x.Name)
            .Select(g => new CategorySlice(
                CategoryName: g.Key,
                Amount: g.Sum(x => x.Amount)))
            .OrderByDescending(x => x.Amount)
            .ToList();
    }

    private static (decimal Income, decimal Expense) SumMonth(IReadOnlyList<Transaction> tx)
    {
        var income = tx.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount);
        var expense = tx.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount);
        return (income, expense);
    }
}