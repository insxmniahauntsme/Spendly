using System.Windows.Controls;
using System.Windows.Input;
using Spendly.Helpers;

namespace Spendly.Views.Analytics;

public partial class AnalyticsView : UserControl
{
    public AnalyticsView()
    {
        InitializeComponent();
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