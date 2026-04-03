namespace Spendly.Application.Handlers.Budgets;

public record CategoryLimitItem(Guid CategoryId, string CategoryName, decimal SpentAmount, decimal LimitAmount);