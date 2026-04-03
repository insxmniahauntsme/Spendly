using System.ComponentModel.DataAnnotations.Schema;

namespace Spendly.Data.Entities;

[Table("category_limits")]
public sealed class CategoryLimitEntity
{
	[Column("id")]
	public Guid Id { get; set; }
	
	[Column("category_id")]
	public Guid CategoryId { get; set; }
	
	[Column("amount")]
	public decimal Amount { get; set; }

	public CategoryEntity Category { get; set; } = null!;
}