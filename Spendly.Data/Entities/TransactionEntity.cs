using System.ComponentModel.DataAnnotations.Schema;
using Spendly.Data.Enums;

namespace Spendly.Data.Entities;

/// <summary>
/// The entity that represents a money transaction.
/// </summary>
[Table("transactions")]
public class TransactionEntity
{
	[Column("id")]
	public Guid Id { get; set; }
	
	[Column("account_id")]
	public Guid AccountId { get; set; }
	
	[Column("category_id")]
	public Guid? CategoryId { get; set; }
	
	[Column("amount")]
	public decimal Amount { get; set; }
	
	[Column("date_utc")]
	public DateTime DateUtc { get; set; }
	
	[Column("type")]
	public TransactionType Type { get; set; }
	
	[Column("comment")]
	public string Comment { get; set; } = "";

	public AccountEntity Account { get; init; } = null!;
	public CategoryEntity? Category { get; init; }
}