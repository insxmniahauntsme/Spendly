using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Spendly.Models;

namespace Spendly.Controls.MonthPicker;

// TODO: Mdaaaaa...
public partial class MonthPicker
{
    public MonthPicker()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Ensure default
        if (SelectedMonth.Year == 0)
            SelectedMonth = new YearMonth(DateTime.Today.Year, DateTime.Today.Month);

        // Provide safe defaults for styles if user didn't pass them
        NavButtonStyle ??= TryFindStyle("MonthPickerNavButtonStyle");
    }

    private Style? TryFindStyle(string key)
    {
        try { return (Style)FindResource(key); }
        catch { return null; }
    }

    // Dependency Properties
    public static readonly DependencyProperty SelectedMonthProperty =
        DependencyProperty.Register(
            nameof(SelectedMonth),
            typeof(YearMonth),
            typeof(MonthPicker),
            new FrameworkPropertyMetadata(
                new YearMonth(DateTime.Today.Year, DateTime.Today.Month),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedMonthChanged));

    public YearMonth SelectedMonth
    {
        get => (YearMonth)GetValue(SelectedMonthProperty);
        set => SetValue(SelectedMonthProperty, value);
    }
    
    public static readonly DependencyProperty PreviewMonthProperty =
        DependencyProperty.Register(
            nameof(PreviewMonth),
            typeof(YearMonth),
            typeof(MonthPicker),
            new PropertyMetadata(default(YearMonth)));

    public YearMonth PreviewMonth
    {
        get => (YearMonth)GetValue(PreviewMonthProperty);
        set => SetValue(PreviewMonthProperty, value);
    }

    private static void OnSelectedMonthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var c = (MonthPicker)d;
        if (c is { IsLoaded: true, IsOpen: true })
            c.RefreshMonthHighlights();
    }

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(MonthPicker),
            new PropertyMetadata(false));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly DependencyProperty ButtonStyleProperty =
        DependencyProperty.Register(nameof(ButtonStyle), typeof(Style), typeof(MonthPicker));

    public Style? ButtonStyle
    {
        get => (Style?)GetValue(ButtonStyleProperty);
        set => SetValue(ButtonStyleProperty, value);
    }

    public static readonly DependencyProperty NavButtonStyleProperty =
        DependencyProperty.Register(nameof(NavButtonStyle), typeof(Style), typeof(MonthPicker));

    public Style? NavButtonStyle
    {
        get => (Style?)GetValue(NavButtonStyleProperty);
        set => SetValue(NavButtonStyleProperty, value);
    }

    public static readonly DependencyProperty IconSourceProperty =
        DependencyProperty.Register(
            nameof(IconSource),
            typeof(string),
            typeof(MonthPicker),
            new PropertyMetadata("pack://application:,,,/Resources/Icons/calendar.svg"));

    public string IconSource
    {
        get => (string)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    // Popup
    private void Popup_Opened(object sender, EventArgs e)
    {
        PreviewMonth = IsValid(SelectedMonth)
            ? SelectedMonth
            : new YearMonth(DateTime.Today.Year, DateTime.Today.Month);

        CenterPopupUnderButton();
        RefreshMonthHighlights();
        PlayPopupOpenAnimation();
    }
    
    private void Popup_Closed(object sender, EventArgs e)
    {
        if (!IsValid(PreviewMonth))
            return;

        if (PreviewMonth != SelectedMonth)
            SelectedMonth = PreviewMonth;
    }

    private void CenterPopupUnderButton()
    {
        PopupRoot.UpdateLayout();
        PopupRoot.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        var popupWidth = PopupRoot.DesiredSize.Width;
        var buttonWidth = DropButton.ActualWidth;

        PartPopup.HorizontalOffset = (buttonWidth - popupWidth) / 2.0;
    }

    private void PlayPopupOpenAnimation()
    {
        var sb = new Storyboard();

        var opacity = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(120),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(opacity, PopupRoot);
        Storyboard.SetTargetProperty(opacity, new PropertyPath(Border.OpacityProperty));
        sb.Children.Add(opacity);

        if (PopupRoot.RenderTransform is ScaleTransform st)
        {
            var sx = new DoubleAnimation
            {
                From = 0.97,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(120),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(sx, st);
            Storyboard.SetTargetProperty(sx, new PropertyPath(ScaleTransform.ScaleXProperty));
            sb.Children.Add(sx);

            var sy = new DoubleAnimation
            {
                From = 0.97,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(120),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(sy, st);
            Storyboard.SetTargetProperty(sy, new PropertyPath(ScaleTransform.ScaleYProperty));
            sb.Children.Add(sy);
        }

        sb.Begin();
    }

    // Header nav (month paging)
    private void PrevMonth_Click(object sender, RoutedEventArgs e)
    {
        var delta = GetDeltaByModifiers(isPrev: true);
        PreviewMonth = AddMonths(PreviewMonth, delta);
        RefreshMonthHighlights();
    }

    private void NextMonth_Click(object sender, RoutedEventArgs e)
    {
        var delta = GetDeltaByModifiers(isPrev: false);
        PreviewMonth = AddMonths(PreviewMonth, delta);
        RefreshMonthHighlights();
    }

    private static int GetDeltaByModifiers(bool isPrev)
    {
        var step = 1;

        if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            step = 12;

        return isPrev ? -step : step;
    }

    private static YearMonth AddMonths(YearMonth ym, int delta)
    {
        var dt = new DateTime(ym.Year, ym.Month, 1).AddMonths(delta);
        return new YearMonth(dt.Year, dt.Month);
    }
    
    private void GoToToday_Click(object sender, RoutedEventArgs e)
    {
        var now = DateTime.Today;
        PreviewMonth = new YearMonth(now.Year, now.Month);
        RefreshMonthHighlights();
        IsOpen = false;
    }

    // Month click
    private void Month_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.Tag is not string s || !int.TryParse(s, out var month)) return;

        PreviewMonth = new YearMonth(PreviewMonth.Year, month);
        IsOpen = false;
    }

    private static bool IsValid(YearMonth ym)
        => ym is { Year: > 0, Month: >= 1 and <= 12 };
    
    // Highlighting (today/selected)
    private void RefreshMonthHighlights()
    {
        if (PopupRoot is null) return;

        var border = (Brush)FindResource("Brush.Border");
        var textPrimary = (Brush)FindResource("Brush.TextPrimary");
        var primary = (Brush)FindResource("Brush.Primary");
        var primaryA60 = (Brush)FindResource("Brush.Primary.A60");
        var selectedGradient = (Brush)FindResource("Brush.MonthSelected");

        var today = DateTime.Today;
        var selected = IsValid(PreviewMonth)
            ? PreviewMonth
            : new YearMonth(DateTime.Today.Year, DateTime.Today.Month);

        foreach (var btn in FindVisualChildren<Button>(MonthsGrid))
        {
            if (btn.Tag is not string s || !int.TryParse(s, out var m))
                continue;

            // Reset (keep hover working from style)
            btn.ClearValue(Button.BackgroundProperty);
            btn.BorderBrush = border;
            btn.Foreground = textPrimary;
            btn.Effect = null;

            // Today: thin primary border
            if (today.Year == selected.Year && m == today.Month)
            {
                btn.BorderBrush = primaryA60;
            }

            // Selected: gradient + glow
            if (m != selected.Month) continue;
            btn.Background = selectedGradient;
            btn.BorderBrush = primary;

            btn.Effect = CreateGlow(primary);
        }
    }

    private static DropShadowEffect CreateGlow(Brush primaryBrush)
    {
        var color = primaryBrush is SolidColorBrush scb ? scb.Color : Colors.MediumPurple;

        return new DropShadowEffect
        {
            Color = color,
            BlurRadius = 14,
            ShadowDepth = 0,
            Opacity = 0.55
        };
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T typed)
                yield return typed;

            foreach (var sub in FindVisualChildren<T>(child))
                yield return sub;
        }
    }
}