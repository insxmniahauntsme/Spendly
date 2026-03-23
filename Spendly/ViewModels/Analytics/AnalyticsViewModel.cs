using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Spendly.Helpers;
using Spendly.Infrastructure.Queries;

namespace Spendly.ViewModels.Analytics;

public partial class AnalyticsViewModel : ObservableObject
{
    private readonly CategoryQueries _categoryQueries;
    
    [ObservableProperty] private ObservableCollection<CategoryChipItem> _categoryChips = [];
    [ObservableProperty] private Guid? _selectedCategoryId;
    [ObservableProperty] private ISeries[] _trendSeries = [];
    [ObservableProperty] private Axis[] _trendXAxes = [];
    [ObservableProperty] private Axis[] _trendYAxes = [];
    [ObservableProperty] private ObservableCollection<AccountCardItem> _topAccounts = [];
    [ObservableProperty] private ObservableCollection<AccountBreakdownItem> _accountBreakdownItems = [];

    public AnalyticsViewModel(CategoryQueries categoryQueries)
    {
        _categoryQueries = categoryQueries;
        _ = BuildCategoryChips();
        BuildTrendChart();
        BuildAccounts();
    }
    
    [RelayCommand]
    private void SelectCategory(CategoryChipItem? item)
    {
        if (item is null)
            return;

        SelectedCategoryId = item.Id;

        foreach (var chip in CategoryChips)
            chip.IsSelected = chip.Id == item.Id;
    }

    private async Task BuildCategoryChips()
    {
        var items = await _categoryQueries.GetCategoriesAsync();

        CategoryChips.Clear();

        foreach (var item in items)
        {
            CategoryChips.Add(
                new CategoryChipItem(
                    item.Id,
                    item.Name,
                    CategoryIconProvider.GetIcon(item.Name)));
        }
    }

    private void BuildTrendChart()
    {
        double[] values = [1200, 800, 920, 640, 590, 1300];

        TrendSeries =
        [
            new LineSeries<double>
            {
                Values = values,
                GeometrySize = 0,
                LineSmoothness = 0,
                Stroke = new SolidColorPaint(new SKColor(0x6C, 0x63, 0xFF, 255), 2),
                Fill = new LinearGradientPaint(
                    [
                        new SKColor(0x6C, 0x63, 0xFF, 28),
                        new SKColor(0x6C, 0x63, 0xFF, 8),
                        new SKColor(0x6C, 0x63, 0xFF, 0)
                    ],
                    new SKPoint(0, 0),
                    new SKPoint(0, 180)),
            }
        ];

        TrendXAxes =
        [
            new Axis
            {
                Labels = ["SEP", "OCT", "NOV", "DEC", "JAN", "FEB"],
                LabelsPaint = new SolidColorPaint(new SKColor(0x88, 0x88, 0xAA, 0xFF)),
                SeparatorsPaint = null,
                TicksPaint = null,
                TextSize = 12,
                Padding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0),
                MinStep = 1,
                ForceStepToMin = true
            }
        ];

        TrendYAxes =
        [
            new Axis
            {
                IsVisible = false,
                MinLimit = 0,
                SeparatorsPaint = null,
                TicksPaint = null,
                LabelsPaint = null,
                Padding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0)
            }
        ];
    }

    private void BuildAccounts()
    {
        AccountBreakdownItems =
        [
            new AccountBreakdownItem
            {
                Name = "Monobank",
                Percent = 54,
                PercentText = "54%",
                BarWidth = 420,
                FillBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6584")!),
                IconBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2A38")!),
                IconText = "mono"
            },
            new AccountBreakdownItem
            {
                Name = "PrivatBank",
                Percent = 35,
                PercentText = "35%",
                BarWidth = 272,
                FillBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4B93FF")!),
                IconBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D7F0C9")!),
                IconText = "↱"
            },
            new AccountBreakdownItem
            {
                Name = "Cash",
                Percent = 8,
                PercentText = "8%",
                BarWidth = 62,
                FillBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC94D")!),
                IconBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4A3A2B")!),
                IconText = "₴"
            },
            new AccountBreakdownItem
            {
                Name = "OTP Credit",
                Percent = 3,
                PercentText = "3%",
                BarWidth = 24,
                FillBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43D9A2")!),
                IconBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5A8F43")!),
                IconText = "₴"
            }
        ];
    }
}

public partial class CategoryChipItem : ObservableObject
{
    [ObservableProperty] private Guid _id;
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _icon;
    [ObservableProperty] private bool _isSelected;

    public CategoryChipItem(Guid id, string name, string icon, bool isSelected = false)
    {
        _id = id;
        _name = name;
        _icon = icon;
        _isSelected = isSelected;
    }
}

public partial class AccountCardItem : ObservableObject
{
    [ObservableProperty] private string _accountName = "";
    [ObservableProperty] private string _badgeText = "";
    [ObservableProperty] private string _amountText = "";
    [ObservableProperty] private string _shareText = "";
    [ObservableProperty] private Brush _accentBrush = Brushes.Transparent;
    [ObservableProperty] private Brush _badgeBackground = Brushes.Transparent;
    [ObservableProperty] private string _iconText = "";
    [ObservableProperty] private Brush _iconBackground = Brushes.Transparent;
}

public partial class AccountBreakdownItem : ObservableObject
{
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private int _percent;
    [ObservableProperty] private string _percentText = "";
    [ObservableProperty] private double _barWidth;
    [ObservableProperty] private Brush _fillBrush = Brushes.Transparent;
    [ObservableProperty] private Brush _iconBackground = Brushes.Transparent;
    [ObservableProperty] private string _iconText = "";
}