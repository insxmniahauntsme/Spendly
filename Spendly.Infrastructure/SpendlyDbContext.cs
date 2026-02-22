using Microsoft.EntityFrameworkCore;
using Spendly.Data.Entities;
using Spendly.Infrastructure.Interfaces;

namespace Spendly.Infrastructure;

public class SpendlyDbContext(DbContextOptions<SpendlyDbContext> options, IProviderModelBuilder? providerModelBuilder)
	: DbContext(options)
{
	public DbSet<AccountEntity> Accounts => Set<AccountEntity>();
	public DbSet<TransactionEntity> Transactions => Set<TransactionEntity>();
	public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(SpendlyDbContext).Assembly);

		providerModelBuilder?.Configure(modelBuilder);
	}
}