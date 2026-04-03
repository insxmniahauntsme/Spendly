using MediatR;
using Microsoft.EntityFrameworkCore;
using Spendly.Application.Handlers.Budgets.Requests;
using Spendly.Domain.Enums;
using Spendly.Infrastructure;

namespace Spendly.Application.Handlers.Budgets;

public class GetBudgetsDataHandler(SpendlyDbContext dbContext) : IRequestHandler<GetBudgetsDataRequest, BudgetsPageData>
{
	public async Task<BudgetsPageData> Handle(GetBudgetsDataRequest request, CancellationToken ct)
	{
		var limits = await dbContext.CategoryLimits.AsNoTracking().Include(x => x.Category).ToListAsync(ct);

		var from = request.Month.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
		var to = from.AddMonths(1);
		
			var transactions = await dbContext.Transactions
			.AsNoTracking()
			.Where(t => t.Type == TransactionType.Expense)
			.Where(t => t.DateUtc >= from && t.DateUtc < to)
			.Where(t => t.CategoryId != null && limits.Select(x => x.CategoryId).Contains(t.CategoryId.Value))
			.ToListAsync(ct);

		var totalBudget = limits.Sum(x => x.Amount);
		var totalSpend = transactions.Sum(x => x.Amount);

		var grouped = transactions
			.GroupBy(x => x.CategoryId)
			.ToDictionary(g => g.Key!.Value, g => g);

		var items = new List<CategoryLimitItem>();

		foreach (var limit in limits)
		{
			var spentAmount = grouped.TryGetValue(limit.CategoryId, out var group)
				? group.Sum(x => x.Amount)
				: 0;

			items.Add(new CategoryLimitItem(
				limit.CategoryId,
				limit.Category.Name,
				spentAmount,
				limit.Amount));
		}

		return new BudgetsPageData(totalBudget, totalSpend, items);
	}
}