using MediatR;
using Microsoft.EntityFrameworkCore;
using Spendly.Application.Analytics.Dashboard;
using Spendly.Application.Handlers.Dashboard.Requests;
using Spendly.Application.Models.Dashboard;
using Spendly.Data.Entities;
using Spendly.Data.Enums;
using Spendly.Domain.Enums;
using Spendly.Domain.Models;
using Spendly.Infrastructure;

namespace Spendly.Application.Handlers.Dashboard;

public sealed class GetDashboardDataHandler(SpendlyDbContext dbContext)
    : IRequestHandler<GetDashboardDataRequest, DashboardData>
{
    public async Task<DashboardData> Handle(GetDashboardDataRequest request, CancellationToken ct)
    {
        var selectedMonth = request.Date;

        var chartCurrentMonth = selectedMonth ?? new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
        var chartCurrentMonthEnd = chartCurrentMonth.AddMonths(1);
        var chartHalfYearAgo = chartCurrentMonth.AddMonths(-5);

        IQueryable<TransactionEntity> query = dbContext.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Account);

        if (selectedMonth is not null)
        {
            var fromUtc = DateTime.SpecifyKind(chartHalfYearAgo.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(chartCurrentMonthEnd.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

            query = query.Where(t => t.DateUtc >= fromUtc && t.DateUtc < toUtc);
        }

        var transactions = await query
            .Select(t => new Transaction
            {
                Id = t.Id,
                AccountId = t.AccountId,
                CategoryId = t.CategoryId,
                Amount = t.Amount,
                DateUtc = t.DateUtc,
                Type = t.Type,
                Comment = t.Comment,
                Category = t.Category == null
                    ? null
                    : new Category { Id = t.Category.Id, Name = t.Category.Name },
                Account = new Account { Id = t.Account.Id, Name = t.Account.Name, Type = t.Account.Type }
            })
            .ToListAsync(ct);

        DashboardKpi kpi;
        IReadOnlyList<Transaction> transactionTableData;

        if (selectedMonth is not null)
        {
            var currentMonthStart = selectedMonth.Value;
            var currentMonthEnd = currentMonthStart.AddMonths(1);
            var prevMonthStart = currentMonthStart.AddMonths(-1);

            var kpiResult = CalculateKpiDataForMonth(transactions, currentMonthStart, currentMonthEnd, prevMonthStart);
            kpi = kpiResult.data;
            transactionTableData = kpiResult.transactions;
        }
        else
        {
            kpi = CalculateKpiDataForAllTime(transactions);
            transactionTableData = transactions
                .OrderByDescending(x => x.DateUtc)
                .ToList();
        }

        var barData = CalculateBarChartData(transactions, chartCurrentMonthEnd, chartHalfYearAgo);
        var donutData = CalculateDonutChartData(transactions, selectedMonth);

        return new DashboardData(kpi, transactionTableData, barData, donutData);
    }

    private static (DashboardKpi data, IReadOnlyList<Transaction> transactions) CalculateKpiDataForMonth(
        IReadOnlyList<Transaction> transactions,
        DateOnly currentStart,
        DateOnly currentEnd,
        DateOnly prevStart)
    {
        var prevTx = transactions.Where(t =>
            DateOnly.FromDateTime(t.DateUtc) >= prevStart &&
            DateOnly.FromDateTime(t.DateUtc) < currentStart).ToList();

        var currTx = transactions.Where(t =>
            DateOnly.FromDateTime(t.DateUtc) >= currentStart &&
            DateOnly.FromDateTime(t.DateUtc) < currentEnd).ToList();

        var (currIncome, currExpense) = SumMonth(currTx);
        var (prevIncome, prevExpense) = SumMonth(prevTx);

        var incomeChangePct = MonthKpiCalculator.CalculateAmountChangePct(currIncome, prevIncome);
        var expenseChangePct = MonthKpiCalculator.CalculateAmountChangePct(currExpense, prevExpense);

        return (
            new DashboardKpi(currIncome, currExpense, incomeChangePct, expenseChangePct),
            currTx
        );
    }

    private static DashboardKpi CalculateKpiDataForAllTime(IReadOnlyList<Transaction> transactions)
    {
        var income = transactions
            .Where(x => x.Type == TransactionType.Income)
            .Sum(x => x.Amount);

        var expense = transactions
            .Where(x => x.Type == TransactionType.Expense)
            .Sum(x => x.Amount);

        return new DashboardKpi(
            MonthlyIncome: income,
            MonthlyExpense: expense,
            IncomeChangePct: null,
            ExpenseChangePct: null
        );
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

    private static IReadOnlyList<CategorySlice> CalculateDonutChartData(
        IReadOnlyList<Transaction> transactions,
        DateOnly? month)
    {
        IEnumerable<Transaction> filtered = transactions
            .Where(t => t is { Type: TransactionType.Expense, Category: not null });

        if (month is not null)
        {
            var start = month.Value;
            var end = start.AddMonths(1);

            filtered = filtered.Where(t =>
            {
                var d = DateOnly.FromDateTime(t.DateUtc);
                return d >= start && d < end;
            });
        }

        return filtered
            .GroupBy(t => t.Category!.Name)
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