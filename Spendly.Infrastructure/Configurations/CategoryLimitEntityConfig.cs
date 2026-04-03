using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spendly.Data.Entities;

namespace Spendly.Infrastructure.Configurations;

public class CategoryLimitEntityConfig : IEntityTypeConfiguration<CategoryLimitEntity>
{
	public void Configure(EntityTypeBuilder<CategoryLimitEntity> builder)
	{
		builder.HasKey(x => x.Id);
		
		builder.Property(x => x.Amount).IsRequired();
		
		builder.HasOne(x => x.Category)
			.WithOne(x => x.Limit)
			.HasForeignKey<CategoryLimitEntity>(x => x.CategoryId)
			.OnDelete(DeleteBehavior.Cascade);
		
		builder.HasIndex(x => x.CategoryId).IsUnique();
	}

}