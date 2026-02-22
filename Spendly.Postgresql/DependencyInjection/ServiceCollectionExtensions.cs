using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Spendly.Infrastructure;

namespace Spendly.Postgresql.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddPostgresql(this IServiceCollection services)
	{
		var cs = Environment.GetEnvironmentVariable("SPENDLY_PG_CS");

		services.AddDbContext<SpendlyDbContext>(options =>
			options.UseNpgsql(cs, x => x.MigrationsAssembly(typeof(ServiceCollectionExtensions).Assembly.GetName().Name)));
		
		return services;
	}
}