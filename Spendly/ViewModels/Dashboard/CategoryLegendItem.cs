using System.Windows.Media;

namespace Spendly.ViewModels.Dashboard;

public sealed class CategoryLegendItem
{
    public string Name { get; set; } = "";
    public string PercentText { get; set; } = "";
    public Brush ColorBrush { get; set; } = Brushes.Transparent;
}