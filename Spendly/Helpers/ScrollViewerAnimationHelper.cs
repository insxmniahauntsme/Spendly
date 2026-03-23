using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Spendly.Helpers;

public static class ScrollViewerAnimationHelper
{
    public static readonly DependencyProperty HorizontalOffsetProperty =
        DependencyProperty.RegisterAttached(
            "HorizontalOffset",
            typeof(double),
            typeof(ScrollViewerAnimationHelper),
            new PropertyMetadata(0d, OnHorizontalOffsetChanged));

    public static void SetHorizontalOffset(DependencyObject element, double value)
    {
        element.SetValue(HorizontalOffsetProperty, value);
    }

    public static double GetHorizontalOffset(DependencyObject element)
    {
        return (double)element.GetValue(HorizontalOffsetProperty);
    }

    private static void OnHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer scrollViewer)
            scrollViewer.ScrollToHorizontalOffset((double)e.NewValue);
    }

    public static void AnimateHorizontalOffset(ScrollViewer scrollViewer, double toOffset, int durationMs = 220)
    {
        var animation = new DoubleAnimation
        {
            From = GetHorizontalOffset(scrollViewer),
            To = toOffset,
            Duration = TimeSpan.FromMilliseconds(durationMs),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        scrollViewer.BeginAnimation(HorizontalOffsetProperty, animation, HandoffBehavior.Compose);
    }
}