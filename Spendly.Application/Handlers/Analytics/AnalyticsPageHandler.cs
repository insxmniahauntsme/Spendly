using MediatR;
using Microsoft.EntityFrameworkCore;
using Spendly.Application.Handlers.Analytics.Requests;
using Spendly.Domain.Enums;
using Spendly.Infrastructure;

namespace Spendly.Application.Handlers.Analytics;

public class AnalyticsPageHandler(SpendlyDbContext dbContext)
	: IRequestHandler<GetAnalyticsDataRequest, AnalyticsPageData>
{
	public async Task<AnalyticsPageData> Handle(GetAnalyticsDataRequest request, CancellationToken ct)
	{
		var topSectionData = await BuildTopSectionData(ct);
		var accountsSectionData = await BuildAccountsSectionData(ct);
		
		return new AnalyticsPageData(topSectionData, accountsSectionData);
	}

	private async Task<AnalyticsTopSectionData> BuildTopSectionData(CancellationToken ct)
	{
		var limits = await dbContext.CategoryLimits.AsNoTracking()
			.Include(x => x.Category)
			.ToListAsync(ct);
		
		var now = DateTime.UtcNow;
		var from = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
		var to = from.AddMonths(1);
		
		var transactions = await dbContext.Transactions
			.AsNoTracking()
			.Where(t => t.Type == TransactionType.Expense)
			.Where(t => t.DateUtc >= from && t.DateUtc < to)
			.Where(t => t.CategoryId != null && limits.Select(x => x.CategoryId).Contains(t.CategoryId.Value))
			.ToListAsync(ct);
		
		var groups = transactions
			.GroupBy(x => x.CategoryId)
			.ToDictionary(x => x.Key!.Value, x => x);

		var snapshots = limits
			.Select(limit =>
			{
				var spent = groups.TryGetValue(limit.CategoryId, out var group)
					? group.Sum(x => x.Amount)
					: 0m;

				var progress = limit.Amount <= 0
					? 0
					: (double)(spent / limit.Amount);

				return new BudgetSnapshot(
					limit.CategoryId,
					limit.Category.Name,
					spent,
					limit.Amount,
					progress);
			})
			.ToList();
		
		var overspent = snapshots
			.Where(x => x.SpentAmount > x.LimitAmount)
			.OrderByDescending(x => x.SpentAmount - x.LimitAmount)
			.FirstOrDefault();
		
		var underused = snapshots
			.Where(x => x.SpentAmount <= x.LimitAmount)
			.OrderBy(x => x.Progress)
			.FirstOrDefault();
		
		var riskItems = snapshots
			.Where(x => x.Progress is >= 0.75 and < 1.0)
			.OrderByDescending(x => x.Progress)
			.Take(2)
			.Select(x => new AnalyticsRiskItem(
				x.CategoryId,
				Math.Min(x.Progress, 1.0),
				x.CategoryName))
			.ToList();
		
		return new AnalyticsTopSectionData(
			Overspent: overspent is null
				? AnalyticsBudgetData.Empty
				: new AnalyticsBudgetData(
					true,
					overspent.CategoryId,
					overspent.CategoryName,
					overspent.SpentAmount,
					overspent.LimitAmount,
					Math.Min(overspent.Progress, 1.0)),

			Underused: underused is null
				? AnalyticsBudgetData.Empty
				: new AnalyticsBudgetData(
					true,
					underused.CategoryId,
					underused.CategoryName,
					underused.SpentAmount,
					underused.LimitAmount,
					Math.Min(underused.Progress, 1.0)),

			RiskItems: riskItems,

			ForecastAmount: 0m,
			HasForecast: false
		);
	}

	private async Task<AnalyticsAccountsSectionData> BuildAccountsSectionData(CancellationToken ct)
	{
		var now = DateTime.UtcNow;
		var from = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
		var to = from.AddMonths(1);

		var transactions = await dbContext.Transactions
			.AsNoTracking()
			.Include(x => x.Account)
			.Where(t => t.Type == TransactionType.Expense)
			.Where(t => t.DateUtc >= from && t.DateUtc < to)
			.ToListAsync(ct);

		if (transactions.Count == 0)
			return new AnalyticsAccountsSectionData([]);

		var totalSpent = transactions.Sum(x => x.Amount);

		var items = transactions
			.GroupBy(x => new { x.AccountId, x.Account.Name })
			.Select(g => new
			{
				Name = g.Key.Name,
				Amount = g.Sum(x => x.Amount)
			})
			.OrderByDescending(x => x.Amount)
			.Select((x, index) => new AnalyticsAccountItem(
				x.Name,
				x.Amount,
				totalSpent == 0 ? 0 : (double)(x.Amount / totalSpent),
				index + 1))
			.ToList();

		return new AnalyticsAccountsSectionData(items);
	}
	
	private sealed record BudgetSnapshot(
		Guid CategoryId,
		string CategoryName,
		decimal SpentAmount,
		decimal LimitAmount,
		double Progress);
}