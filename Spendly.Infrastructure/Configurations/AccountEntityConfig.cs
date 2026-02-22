using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spendly.Data.Entities;

namespace Spendly.Infrastructure.Configurations;

public class AccountEntityConfig : IEntityTypeConfiguration<AccountEntity>
{
	public void Configure(EntityTypeBuilder<AccountEntity> entity)
	{
		entity.HasMany(a => a.Transactions)
			.WithOne(t => t.Account)
			.HasForeignKey(t => t.AccountId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}