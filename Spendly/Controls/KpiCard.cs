using System.Windows;
using System.Windows.Controls;
using Spendly.Controls.Enums;

namespace Spendly.Controls;

public class KpiCard : Control
{
    static KpiCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(KpiCard),
            new FrameworkPropertyMetadata(typeof(KpiCard)));
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(KpiCard),
            new PropertyMetadata(""));

    public static readonly DependencyProperty ValueTextProperty =
        DependencyProperty.Register(nameof(ValueText), typeof(string), typeof(KpiCard),
            new PropertyMetadata(""));

    public static readonly DependencyProperty BadgeTextProperty =
        DependencyProperty.Register(nameof(BadgeText), typeof(string), typeof(KpiCard),
            new PropertyMetadata(""));

    public static readonly DependencyProperty SubTextProperty =
        DependencyProperty.Register(nameof(SubText), typeof(string), typeof(KpiCard),
            new PropertyMetadata(""));

    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(nameof(Variant), typeof(KpiVariant), typeof(KpiCard),
            new PropertyMetadata(KpiVariant.Primary));

    /// <summary>
    /// True = data is absent - shows "—" and neutral style.
    /// False = data is present.
    /// </summary>
    public static readonly DependencyProperty IsEmptyProperty =
        DependencyProperty.Register(nameof(IsEmpty), typeof(bool), typeof(KpiCard),
            new PropertyMetadata(false));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string ValueText
    {
        get => (string)GetValue(ValueTextProperty);
        set => SetValue(ValueTextProperty, value);
    }

    public string BadgeText
    {
        get => (string)GetValue(BadgeTextProperty);
        set => SetValue(BadgeTextProperty, value);
    }

    public string SubText
    {
        get => (string)GetValue(SubTextProperty);
        set => SetValue(SubTextProperty, value);
    }

    public KpiVariant Variant
    {
        get => (KpiVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public bool IsEmpty
    {
        get => (bool)GetValue(IsEmptyProperty);
        set => SetValue(IsEmptyProperty, value);
    }
}
