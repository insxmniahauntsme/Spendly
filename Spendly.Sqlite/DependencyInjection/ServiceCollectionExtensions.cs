using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Spendly.Infrastructure;
using Spendly.Infrastructure.Interfaces;
using Spendly.Sqlite.Configurations;

namespace Spendly.Sqlite.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSqlite(this IServiceCollection services)
	{
		services.AddTransient<IProviderModelBuilder, SqliteModelBuilder>();

		var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		var folder = Path.Combine(appData, "Spendly");

		Directory.CreateDirectory(folder);

		var dbPath = Path.Combine(folder, "spendly.db");

		services.AddDbContext<SpendlyDbContext>(options =>
			options.UseSqlite($"Data Source={dbPath}", x =>
				{
					x.MigrationsAssembly(typeof(ServiceCollectionExtensions).Assembly.FullName);
				}));

		return services;
	}
}