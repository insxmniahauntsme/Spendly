using Spendly.Data.Enums;

namespace Spendly.Domain.Models;

public class Transaction
{
    public Guid Id { get; set; }
	
    public Guid AccountId { get; set; }
	
    public Guid? CategoryId { get; set; }
	
    public decimal Amount { get; set; }
	
    public DateTime DateUtc { get; set; }
	
    public TransactionType Type { get; set; }
	
    public string Comment { get; set; } = "";

    public Account Account { get; init; } = null!;
    public Category? Category { get; init; }
}