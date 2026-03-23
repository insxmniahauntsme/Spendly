using MediatR;
using Spendly.Application.Handlers.Analytics.Requests;

namespace Spendly.Application.Handlers.Analytics;

public class AnalyticsPageHandler() : IRequestHandler<GetAnalyticsDataRequest, AnalyticsPageData>
{
    public Task<AnalyticsPageData> Handle(GetAnalyticsDataRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}