using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Spendly.Controls.Dashboard;

namespace Spendly.Controls.Analytics;

public partial class AnalyticsBudgetCard : UserControl
{
    public AnalyticsBudgetCard()
    {
        InitializeComponent();
        Loaded += (_, _) => UpdateVisualState();
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(AnalyticsBudgetCard),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty BadgeTextProperty =
        DependencyProperty.Register(
            nameof(BadgeText),
            typeof(string),
            typeof(AnalyticsBudgetCard),
            new PropertyMetadata(string.Empty, OnVisualPropertyChanged));

    public static readonly DependencyProperty IconSourceProperty =
        DependencyProperty.Register(
            nameof(IconSource),
            typeof(string),
            typeof(AnalyticsBudgetCard),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty CurrentTextProperty =
        DependencyProperty.Register(
            nameof(CurrentText),
            typeof(string),
            typeof(AnalyticsBudgetCard),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty LimitTextProperty =
        DependencyProperty.Register(
            nameof(LimitText),
            typeof(string),
            typeof(AnalyticsBudgetCard),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ProgressValueProperty =
        DependencyProperty.Register(
            nameof(ProgressValue),
            typeof(double),
            typeof(AnalyticsBudgetCard),
            new PropertyMetadata(0d, OnProgressValueChanged));

    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(
            nameof(Variant),
            typeof(KpiVariant),
            typeof(AnalyticsBudgetCard),
            new PropertyMetadata(KpiVariant.Primary, OnVisualPropertyChanged));

    public static readonly DependencyProperty AccentBrushProperty =
        DependencyProperty.Register(
            nameof(AccentBrush),
            typeof(Brush),
            typeof(AnalyticsBudgetCard),
            new PropertyMetadata(Brushes.Transparent));
    
    public static readonly DependencyProperty HasDataProperty =
        DependencyProperty.Register(
            nameof(HasData),
            typeof(bool),
            typeof(AnalyticsBudgetCard),
            new PropertyMetadata(true));

    public bool HasData
    {
        get => (bool)GetValue(HasDataProperty);
        set => SetValue(HasDataProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string BadgeText
    {
        get => (string)GetValue(BadgeTextProperty);
        set => SetValue(BadgeTextProperty, value);
    }

    public string IconSource
    {
        get => (string)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    public string CurrentText
    {
        get => (string)GetValue(CurrentTextProperty);
        set => SetValue(CurrentTextProperty, value);
    }

    public string LimitText
    {
        get => (string)GetValue(LimitTextProperty);
        set => SetValue(LimitTextProperty, value);
    }

    public double ProgressValue
    {
        get => (double)GetValue(ProgressValueProperty);
        set => SetValue(ProgressValueProperty, value);
    }

    public KpiVariant Variant
    {
        get => (KpiVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public Brush AccentBrush
    {
        get => (Brush)GetValue(AccentBrushProperty);
        private set => SetValue(AccentBrushProperty, value);
    }

    private static void OnProgressValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AnalyticsBudgetCard card)
            card.UpdateProgress();
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AnalyticsBudgetCard card)
            card.UpdateVisualState();
    }

    private void ProgressHost_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        if (ProgressFill is null || ProgressHost is null)
            return;

        var clamped = Math.Max(0d, Math.Min(1d, ProgressValue));
        ProgressFill.Width = ProgressHost.ActualWidth * clamped;
    }

    private void UpdateVisualState()
    {
        if (Badge is null || Glow is null)
            return;

        Badge.Visibility = string.IsNullOrWhiteSpace(BadgeText)
            ? Visibility.Collapsed
            : Visibility.Visible;

        switch (Variant)
        {
            case KpiVariant.Success:
                AccentBrush = TryFindBrush("Brush.Success");
                Badge.Background = TryFindBrush("Brush.SuccessOverlay");
                Glow.Fill = CreateGlowBrush("Spendly.Success.A60", "Spendly.Success.A10");
                Glow.Opacity = 0.60;
                break;

            case KpiVariant.Danger:
                AccentBrush = TryFindBrush("Brush.Danger");
                Badge.Background = TryFindBrush("Brush.DangerOverlay");
                Glow.Fill = CreateGlowBrush("Spendly.Danger.A60", "Spendly.Danger.A10");
                Glow.Opacity = 0.60;
                break;

            case KpiVariant.Warning:
                AccentBrush = TryFindBrush("Brush.Warning");
                Badge.Background = TryFindBrush("Brush.WarningOverlay");
                Glow.Fill = CreateGlowBrush("Spendly.Warning.A60", "Spendly.Warning.A10");
                Glow.Opacity = 0.48;
                break;

            case KpiVariant.Neutral:
                AccentBrush = TryFindBrush("Brush.TextPrimary");
                Badge.Background = TryFindBrush("Brush.NeutralOverlay");
                Glow.Fill = CreateGlowBrush("Spendly.TextMuted.A60", "Spendly.TextMuted.A10");
                Glow.Opacity = 0.35;
                break;

            default:
                AccentBrush = TryFindBrush("Brush.Primary");
                Badge.Background = TryFindBrush("Brush.PrimaryOverlay");
                Glow.Fill = CreateGlowBrush("Spendly.Primary.A60", "Spendly.Primary.A10");
                Glow.Opacity = 0.60;
                break;
        }

        UpdateProgress();
    }

    private Brush TryFindBrush(string resourceKey)
    {
        return (Brush)(TryFindResource(resourceKey) ?? Brushes.Transparent);
    }

    private Brush CreateGlowBrush(string strongColorKey, string softColorKey)
    {
        var strong = (Color)(TryFindResource(strongColorKey) ?? Colors.Transparent);
        var soft = (Color)(TryFindResource(softColorKey) ?? Colors.Transparent);

        return new RadialGradientBrush
        {
            Center = new Point(0.5, 0.5),
            GradientOrigin = new Point(0.5, 0.5),
            RadiusX = 0.5,
            RadiusY = 0.5,
            GradientStops =
            {
                new GradientStop(strong, 0.0),
                new GradientStop(soft, 0.45),
                new GradientStop(Colors.Transparent, 1.0)
            }
        };
    }
}