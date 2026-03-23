using Spendly.Data.Enums;
using Spendly.Domain.Enums;

namespace Spendly.ViewModels.Dashboard;

// TODO: Move to Domain
public sealed class TransactionRow
{
    public string CategoryIconSource { get; set; } = "";
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public string Account { get; set; } = "";
    public string Date { get; set; } = "";
    public string Amount { get; set; } = "";
    public TransactionType Type { get; set; }
}