using System.ComponentModel.DataAnnotations.Schema;
using Spendly.Data.Enums;

namespace Spendly.Data.Entities;

/// <summary>
/// The entity that represents a user money account.
/// </summary>
[Table("accounts")]
public class AccountEntity
{
	[Column("id")]
	public Guid Id { get; set; }
	
	[Column("name")]
	public string Name { get; set; } = null!;
	
	[Column("balance")]
	public decimal Balance { get; set; }
	
	[Column("type")]
	public AccountType Type { get; set; }
	
	public List<TransactionEntity> Transactions { get; init; } = [];
}