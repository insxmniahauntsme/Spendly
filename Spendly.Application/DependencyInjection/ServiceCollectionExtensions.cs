using Microsoft.Extensions.DependencyInjection;

namespace Spendly.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(options =>
            options.RegisterServicesFromAssemblyContaining(typeof(ServiceCollectionExtensions)));
        
        return services;
    }
}