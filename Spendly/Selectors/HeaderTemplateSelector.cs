using System.Windows;
using System.Windows.Controls;
using Spendly.ViewModels.Analytics;
using Spendly.ViewModels.Dashboard;
using Spendly.ViewModels.Transactions;

namespace Spendly.Selectors;

public sealed class HeaderTemplateSelector : DataTemplateSelector
{
    public DataTemplate? Dashboard { get; set; }
    public DataTemplate? Transactions { get; set; }
    public DataTemplate? Analytics { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
        => item switch
        {
            DashboardViewModel => Dashboard,
            TransactionsViewModel => Transactions,
            AnalyticsViewModel => Analytics,
            _ => base.SelectTemplate(item, container)
        };
}