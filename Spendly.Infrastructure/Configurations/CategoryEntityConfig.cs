using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spendly.Data.Entities;

namespace Spendly.Infrastructure.Configurations;

public class CategoryEntityConfig : IEntityTypeConfiguration<CategoryEntity>
{
	public void Configure(EntityTypeBuilder<CategoryEntity> entity)
	{
		entity.HasKey(x => x.Id);

		entity.Property(x => x.Name)
			.IsRequired()
			.HasMaxLength(64);

		entity.HasIndex(x => x.Name)
			.IsUnique();
	}
}