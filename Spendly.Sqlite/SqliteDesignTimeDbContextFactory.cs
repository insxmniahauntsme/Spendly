using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Spendly.Infrastructure;
using Spendly.Sqlite.Configurations;

namespace Spendly.Sqlite;

public class SqliteDesignTimeDbContextFactory : IDesignTimeDbContextFactory<SpendlyDbContext>
{
	public SpendlyDbContext CreateDbContext(string[] args)
	{
		var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		var folder = Path.Combine(appData, "Spendly");
		
		Directory.CreateDirectory(folder);

		var dbPath = Path.Combine(folder, "spendly.db");

		var options = new DbContextOptionsBuilder<SpendlyDbContext>()
			.UseSqlite($"Data Source={dbPath}", x =>
			{
				x.MigrationsAssembly(typeof(SqliteDesignTimeDbContextFactory).Assembly.FullName);
			}).Options;

		return new SpendlyDbContext(options, new SqliteModelBuilder());
	}
}