using Microsoft.EntityFrameworkCore;
using Spendly.Data.Entities;
using Spendly.Infrastructure.Interfaces;

namespace Spendly.Sqlite.Configurations;

public class SqliteModelBuilder : IProviderModelBuilder
{
	public void Configure(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<AccountEntity>()
			.Property(x => x.Type)
			.HasConversion<string>()
			.HasMaxLength(32);
	}
}