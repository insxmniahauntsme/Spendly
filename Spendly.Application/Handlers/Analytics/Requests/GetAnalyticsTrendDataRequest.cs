using MediatR;

namespace Spendly.Application.Handlers.Analytics.Requests;

public sealed record GetAnalyticsTrendDataRequest(Guid CategoryId) : IRequest<AnalyticsTrendSectionData>;