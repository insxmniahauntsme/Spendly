using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Spendly.Infrastructure;
using Spendly.Postgresql.Configurations;

namespace Spendly.Postgresql;

public class PostgresqlDesignTimeDbContextFactory : IDesignTimeDbContextFactory<SpendlyDbContext>
{
	public SpendlyDbContext CreateDbContext(string[] args)
	{
		var cs = Environment.GetEnvironmentVariable("SPENDLY_PG_CS");

		var options = new DbContextOptionsBuilder<SpendlyDbContext>()
			.UseNpgsql(cs, x =>
			{
				x.MigrationsAssembly(typeof(PostgresqlDesignTimeDbContextFactory).Assembly.FullName);
			}).Options;
		
		return new SpendlyDbContext(options, new PostgresqlModelBuilder());
	}
}