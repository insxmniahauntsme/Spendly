using MediatR;
using Microsoft.EntityFrameworkCore;
using Spendly.Application.Handlers.Analytics.Requests;
using Spendly.Domain.Enums;
using Spendly.Infrastructure;

namespace Spendly.Application.Handlers.Analytics;

public class AnalyticsTrendHandler(SpendlyDbContext dbContext)
    : IRequestHandler<GetAnalyticsTrendDataRequest, AnalyticsTrendSectionData>
{
    public async Task<AnalyticsTrendSectionData> Handle(GetAnalyticsTrendDataRequest request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var from = currentMonthStart.AddMonths(-5);
        var to = currentMonthStart.AddMonths(1);

        var categories = await dbContext.Categories
            .AsNoTracking()
            .ToListAsync(ct);

        var categoryItems = categories
            .Select(x => new AnalyticsTrendCategoryItem(x.Id, x.Name))
            .OrderBy(x => x.CategoryName)
            .ToList();

        var transactions = await dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.Type == TransactionType.Expense)
            .Where(t => t.CategoryId == request.CategoryId)
            .Where(t => t.DateUtc >= from && t.DateUtc < to)
            .ToListAsync(ct);

        var grouped = transactions
            .GroupBy(t => new DateOnly(t.DateUtc.Year, t.DateUtc.Month, 1))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        var months = Enumerable.Range(0, 6)
            .Select(i => currentMonthStart.AddMonths(i - 5))
            .Select(x => new DateOnly(x.Year, x.Month, 1))
            .ToList();

        var points = months
            .Select(month => new AnalyticsTrendPointItem(
                month,
                grouped.GetValueOrDefault(month, 0m)))
            .ToList();

        return new AnalyticsTrendSectionData(
            categoryItems,
            request.CategoryId,
            points);
    }
}