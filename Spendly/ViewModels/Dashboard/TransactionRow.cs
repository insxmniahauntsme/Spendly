using Spendly.Data.Enums;

namespace Spendly.ViewModels.Dashboard;

// TODO: Move to Domain
public sealed class TransactionRow
{
    public string IconText { get; set; } = "";
    public string Title { get; init; } = "";
    public string Amount { get; init; } = "-";
    public TransactionType Type { get; init; }
    public string Category { get; init; } = "";
    public string Date { get; init; }
}