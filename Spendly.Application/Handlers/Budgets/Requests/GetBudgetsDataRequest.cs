using MediatR;

namespace Spendly.Application.Handlers.Budgets.Requests;

public record GetBudgetsDataRequest(DateOnly Month) : IRequest<BudgetsPageData>;