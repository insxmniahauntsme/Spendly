using Spendly.Data.Enums;

namespace Spendly.Models;

public record Account(Guid Id, string Name, decimal Balance, AccountType Type);