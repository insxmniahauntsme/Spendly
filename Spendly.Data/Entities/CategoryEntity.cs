using System.ComponentModel.DataAnnotations.Schema;

namespace Spendly.Data.Entities;

/// <summary>
/// The entity that represents a custom category.
/// </summary>
[Table("categories")]
public class CategoryEntity
{
	[Column("id")]
	public Guid Id { get; set; }
	
	[Column("name")]
	public required string Name { get; set; }
}