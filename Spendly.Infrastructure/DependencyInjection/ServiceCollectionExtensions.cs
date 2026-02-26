using Microsoft.Extensions.DependencyInjection;
using Spendly.Infrastructure.Interfaces;
using Spendly.Infrastructure.Repositories;
using Spendly.Infrastructure.Services;

namespace Spendly.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services)
	{
		services.AddTransient<IDbInitializer, DbInitializer>();
		services.AddTransient<IAccountsRepository, AccountsRepository>();
		services.AddTransient<ITransactionsRepository, TransactionsRepository>();
		services.AddTransient<IUnitOfWork, UnitOfWork>();
		
		return services;
	}
}