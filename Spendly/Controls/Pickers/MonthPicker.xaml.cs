using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace Spendly.Controls.Pickers;

public partial class MonthPicker : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public MonthPicker()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!IsValid(PreviewMonth))
        {
            PreviewMonth = SelectedMonth is not null
                ? new DateOnly(SelectedMonth.Value.Year, SelectedMonth.Value.Month, 1)
                : new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
        }

        NavButtonStyle ??= TryFindStyle("MonthPickerNavButtonStyle");
        RefreshTextBindings();
    }

    private Style? TryFindStyle(string key)
    {
        try
        {
            return (Style)FindResource(key);
        }
        catch
        {
            return null;
        }
    }

    public string SelectedMonthText =>
        SelectedMonth?.ToDateTime(TimeOnly.MinValue).ToString("MMMM yyyy") ?? "All time";

    public string PreviewMonthText =>
        PreviewMonth.ToDateTime(TimeOnly.MinValue).ToString("MMMM yyyy");

    public DateOnly? SelectedMonth
    {
        get => (DateOnly?)GetValue(SelectedMonthProperty);
        set => SetValue(SelectedMonthProperty, value);
    }

    public static readonly DependencyProperty SelectedMonthProperty =
        DependencyProperty.Register(
            nameof(SelectedMonth),
            typeof(DateOnly?),
            typeof(MonthPicker),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedMonthChanged));

    public DateOnly PreviewMonth
    {
        get => (DateOnly)GetValue(PreviewMonthProperty);
        set => SetValue(PreviewMonthProperty, value);
    }

    public static readonly DependencyProperty PreviewMonthProperty =
        DependencyProperty.Register(
            nameof(PreviewMonth),
            typeof(DateOnly),
            typeof(MonthPicker),
            new PropertyMetadata(new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1)));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(
            nameof(IsOpen),
            typeof(bool),
            typeof(MonthPicker),
            new PropertyMetadata(false));

    public Style? ButtonStyle
    {
        get => (Style?)GetValue(ButtonStyleProperty);
        set => SetValue(ButtonStyleProperty, value);
    }

    public static readonly DependencyProperty ButtonStyleProperty =
        DependencyProperty.Register(
            nameof(ButtonStyle),
            typeof(Style),
            typeof(MonthPicker));

    public Style? NavButtonStyle
    {
        get => (Style?)GetValue(NavButtonStyleProperty);
        set => SetValue(NavButtonStyleProperty, value);
    }

    public static readonly DependencyProperty NavButtonStyleProperty =
        DependencyProperty.Register(
            nameof(NavButtonStyle),
            typeof(Style),
            typeof(MonthPicker));

    public string IconSource
    {
        get => (string)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    public static readonly DependencyProperty IconSourceProperty =
        DependencyProperty.Register(
            nameof(IconSource),
            typeof(string),
            typeof(MonthPicker),
            new PropertyMetadata("pack://application:,,,/Resources/Icons/calendar.svg"));

    private static void OnSelectedMonthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MonthPicker picker)
            return;

        if (e.NewValue is DateOnly selected)
        {
            picker.PreviewMonth = new DateOnly(selected.Year, selected.Month, 1);
        }

        picker.RefreshTextBindings();

        if (picker.IsLoaded)
            picker.RefreshMonthHighlights();
    }

    private void DropButton_OnPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        SelectedMonth = null;
        IsOpen = false;
        RefreshTextBindings();
        e.Handled = true;
    }

    private void Popup_Opened(object sender, EventArgs e)
    {
        PreviewMonth = SelectedMonth is not null
            ? new DateOnly(SelectedMonth.Value.Year, SelectedMonth.Value.Month, 1)
            : new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);

        CenterPopupUnderButton();
        RefreshTextBindings();
        RefreshMonthHighlights();
        PlayPopupOpenAnimation();
    }

    private void Popup_Closed(object sender, EventArgs e)
    {
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

    private void PrevMonth_Click(object sender, RoutedEventArgs e)
    {
        var delta = GetDeltaByModifiers(isPrev: true);
        PreviewMonth = PreviewMonth.AddMonths(delta);
        RefreshTextBindings();
        RefreshMonthHighlights();
    }

    private void NextMonth_Click(object sender, RoutedEventArgs e)
    {
        var delta = GetDeltaByModifiers(isPrev: false);
        PreviewMonth = PreviewMonth.AddMonths(delta);
        RefreshTextBindings();
        RefreshMonthHighlights();
    }

    private static int GetDeltaByModifiers(bool isPrev)
    {
        var step = 1;

        if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            step = 12;

        return isPrev ? -step : step;
    }

    private void GoToToday_Click(object sender, RoutedEventArgs e)
    {
        var today = DateTime.Today;
        SelectedMonth = new DateOnly(today.Year, today.Month, 1);
        RefreshTextBindings();
        IsOpen = false;
    }

    private void Month_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is null)
            return;

        if (!int.TryParse(button.Tag.ToString(), out var month))
            return;

        SelectedMonth = new DateOnly(PreviewMonth.Year, month, 1);
        RefreshTextBindings();
        IsOpen = false;
    }

    private static bool IsValid(DateOnly date)
        => date.Year > 0 && date.Month is >= 1 and <= 12;

    private void RefreshMonthHighlights()
    {
        if (PopupRoot is null)
            return;

        var border = (Brush)FindResource("Brush.Border");
        var textPrimary = (Brush)FindResource("Brush.TextPrimary");
        var primary = (Brush)FindResource("Brush.Primary");
        var primaryA60 = (Brush)FindResource("Brush.Primary.A60");
        var selectedGradient = (Brush)FindResource("Brush.MonthSelected");

        var today = DateTime.Today;
        var preview = IsValid(PreviewMonth)
            ? PreviewMonth
            : new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);

        foreach (var btn in FindVisualChildren<Button>(MonthsGrid))
        {
            if (btn.Tag is not string s || !int.TryParse(s, out var m))
                continue;

            btn.ClearValue(Button.BackgroundProperty);
            btn.BorderBrush = border;
            btn.Foreground = textPrimary;
            btn.Effect = null;

            if (today.Year == preview.Year && today.Month == m)
                btn.BorderBrush = primaryA60;

            if (m != preview.Month)
                continue;

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

    private void RefreshTextBindings()
    {
        RaisePropertyChanged(nameof(SelectedMonthText));
        RaisePropertyChanged(nameof(PreviewMonthText));
    }
}