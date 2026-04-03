using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Spendly.Helpers;
using Spendly.ViewModels.Analytics;

namespace Spendly.Views.Analytics;

public partial class AnalyticsView
{
    private bool _isInitialized;
    
    public AnalyticsView()
    {
        InitializeComponent();
        Loaded += AnalyticsView_Loaded;
    }
    
    private async void AnalyticsView_Loaded(object sender, RoutedEventArgs e)
    {
        if (_isInitialized)
            return;

        if (DataContext is not AnalyticsViewModel vm)
            return;

        _isInitialized = true;
        await vm.LoadData();
    }
    
    private void CategoryChipsScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer)
            return;

        if (scrollViewer.ScrollableWidth <= 0)
            return;

        const double step = 140;

        var targetOffset = e.Delta > 0
            ? scrollViewer.HorizontalOffset - step
            : scrollViewer.HorizontalOffset + step;

        if (targetOffset < 0)
            targetOffset = 0;

        if (targetOffset > scrollViewer.ScrollableWidth)
            targetOffset = scrollViewer.ScrollableWidth;

        ScrollViewerAnimationHelper.AnimateHorizontalOffset(scrollViewer, targetOffset, 240);
        e.Handled = true;
    }
}