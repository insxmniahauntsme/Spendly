namespace Spendly.ViewModels.Transactions;

public sealed class PaginationItem
{
    public string Text { get; init; } = string.Empty;
    public int? Page { get; init; }
    public bool IsCurrent { get; init; }
    public bool IsEllipsis => Page is null;
}