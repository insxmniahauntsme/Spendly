using MediatR;
using Spendly.Application.Models.Dashboard;

namespace Spendly.Application.Handlers.Dashboard.Requests;

public sealed record GetDashboardDataRequest(int Year, int Month) : IRequest<DashboardData>;