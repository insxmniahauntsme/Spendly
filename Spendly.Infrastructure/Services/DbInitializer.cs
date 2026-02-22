using Microsoft.EntityFrameworkCore;
using Spendly.Data.Entities;
using Spendly.Data.Enums;
using Spendly.Infrastructure.Interfaces;

namespace Spendly.Infrastructure.Services;

public sealed class DbInitializer(SpendlyDbContext dbContext) : IDbInitializer
{
	public async Task InitializeAsync(CancellationToken ct = default)
	{
		await dbContext.Database.MigrateAsync(ct);

		await EnsureCashAccountCreated();
	}

	private async Task EnsureCashAccountCreated()
	{
		if (dbContext.Accounts.Any(x => x.Type == AccountType.Cash)) return;

		var entity = new AccountEntity
		{
			Name = "Cash",
			Balance = 0,
			Type = AccountType.Cash
		};
		
		await dbContext.Accounts.AddAsync(entity);
		await dbContext.SaveChangesAsync();
	}
}