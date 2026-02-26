using Spendly.Data.Enums;

namespace Spendly.Domain.Models;

public class Account
{
    public Guid Id { get; set; }
	
    public string Name { get; set; } = null!;
	
    public decimal Balance { get; set; }
	
    public AccountType Type { get; set; }
	
    public List<Transaction> Transactions { get; init; } = [];
}