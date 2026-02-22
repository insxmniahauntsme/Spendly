using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spendly.Data.Entities;

namespace Spendly.Infrastructure.Configurations;

public class TransactionEntityConfig : IEntityTypeConfiguration<TransactionEntity>
{
	public void Configure(EntityTypeBuilder<TransactionEntity> entity)
	{
		entity.HasKey(x => x.Id);

		entity.Property(x => x.Amount).HasPrecision(18, 2);
		entity.Property(x => x.DateUtc).IsRequired();
		entity.Property(x => x.Comment).HasMaxLength(512);

		entity.HasOne(x => x.Account)
			.WithMany(x => x.Transactions)
			.HasForeignKey(x => x.AccountId);

		entity.HasOne(x => x.Category)
			.WithMany()
			.HasForeignKey(x => x.CategoryId)
			.OnDelete(DeleteBehavior.SetNull);
	}
}