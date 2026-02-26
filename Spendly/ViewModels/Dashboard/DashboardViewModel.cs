using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MediatR;
using SkiaSharp;
using Spendly.Application.Handlers.Dashboard.Requests;
using Spendly.Application.Models.Dashboard;
using Spendly.Data.Enums;
using Spendly.Domain.Models;
using Spendly.Models;
using Spendly.Views.Dashboard;

namespace Spendly.ViewModels.Dashboard;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private CancellationTokenSource? _loadCts;
    
    [ObservableProperty] private YearMonth _selectedMonth = new(DateTime.Today.Year, DateTime.Today.Month);
    
    // Kpi cards
    [ObservableProperty] private string _incomeMonthText = "—";
    [ObservableProperty] private string _incomeMonthBadgeText = "";
    [ObservableProperty] private string _expenseMonthText = "—";
    [ObservableProperty] private string _expenseMonthBadgeText = "";
    [ObservableProperty] private string _balanceText = "—";
    [ObservableProperty] private string _balanceSubText = "";
    [ObservableProperty] private string _transactionsCountText = "—";
    
    
    // CHARTS
    // Bar chart
    [ObservableProperty] private ISeries[] _incomeExpenseSeries = [];
    [ObservableProperty] private Axis[] _monthsAxis = [];
    [ObservableProperty] private Axis[] _moneyAxis = [];
    
    // Donut chart
    [ObservableProperty] private ISeries[] _categorySeries = [];
    [ObservableProperty] private ObservableCollection<CategoryLegendItem> _categoryLegend = [];
    
    
    // Transactions table
    [ObservableProperty] private ObservableCollection<TransactionRow> _recentTransactions = [];
    
    public DashboardViewModel(IMediator mediator)
    {
        _mediator = mediator;
        
        _ = LoadDataForMonthAsync(SelectedMonth);
    }

    private async Task LoadDataForMonthAsync(YearMonth month)
    {
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        try
        {
            var data = await _mediator.Send(
                new GetDashboardDataRequest(month.Year, month.Month),
                token);

            if (token.IsCancellationRequested) return;

            FillKpiData(data);
            FillBarChart(data);
            FillDonutChart(data);
            FillTransactions(data);
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
    }
    
    partial void OnSelectedMonthChanged(YearMonth value){
        _ = LoadDataForMonthAsync(value);
    }

    private void FillKpiData(DashboardData data)
    {
        IncomeMonthText = Money(data.Kpi.MonthlyIncome);
        ExpenseMonthText = Money(data.Kpi.MonthlyExpense);
        BalanceText = Money(data.Kpi.Balance);

        TransactionsCountText = data.Transactions.Count.ToString("N0");

        IncomeMonthBadgeText = FormatBadge(data.Kpi.IncomeChangePct);
        ExpenseMonthBadgeText = FormatBadge(data.Kpi.ExpenseChangePct);

        BalanceSubText = data.Kpi.BalancePctOfIncome is null
            ? ""
            : $"{data.Kpi.BalancePctOfIncome.Value:0.0%} of income";
    }
    
    private void FillBarChart(DashboardData data)
    {
        var points = data.Last6Months;

        var labels = points.Select(p => p.MonthStart
                .ToDateTime(TimeOnly.MinValue)
                .ToString("MMM", CultureInfo.InvariantCulture))
            .ToArray();

        var income = points.Select(p => (double)p.Income).ToArray();
        var expense = points.Select(p => (double)p.Expense).ToArray();

        IncomeExpenseSeries =
        [
            new ColumnSeries<double>
            {
                Name = "Income",
                Values = income,
                Fill = new SolidColorPaint(new SKColor(0x43, 0xD9, 0xA2, 0x22)),
                Stroke = new SolidColorPaint(new SKColor(0x43, 0xD9, 0xA2, 0xFF), 2),
                MaxBarWidth = 42
            },
            new ColumnSeries<double>
            {
                Name = "Expenses",
                Values = expense,
                Fill = new SolidColorPaint(new SKColor(0xFF, 0x65, 0x84, 0x22)),
                Stroke = new SolidColorPaint(new SKColor(0xFF, 0x65, 0x84, 0xFF), 2),
                MaxBarWidth = 42
            }
        ];

        MonthsAxis =
        [
            new Axis
            {
                Labels = labels,
                LabelsPaint = new SolidColorPaint(new SKColor(0x88, 0x88, 0xAA, 0xFF)),
                SeparatorsPaint = null,
                TicksPaint = null
            }
        ];

        MoneyAxis =
        [
            new Axis
            {
                LabelsPaint = new SolidColorPaint(new SKColor(0x88, 0x88, 0xAA, 0xFF)),
                SeparatorsPaint = new SolidColorPaint(new SKColor(0x2A, 0x2A, 0x38, 0xFF), 1),
                MinLimit = 0
            }
        ];
    }

    private void FillDonutChart(DashboardData data)
    {
        var slices = data.Categories;

        CategoryLegend.Clear();

        var total = slices.Sum(x => x.Amount);
        if (total <= 0)
        {
            CategorySeries = [];
            OnPropertyChanged(nameof(CategorySeries)); // TODO: Remove if possible
            return;
        }

        // TODO: Remove hardcoded colors
        var palette = new[]
        {
            "#6C63FF", "#FF6584", "#43D9A2", "#FFC94D", "#8888AA",
            "#4D9DE0", "#E15554", "#3BB273", "#E1BC29"
        };

        CategorySeries = slices.Select(s =>
        {
            var hex = palette[ColorIndex(s.CategoryName)]; // TODO: Remove randomness and duplication
            var c = (Color)ColorConverter.ConvertFromString(hex)!;
            var sk = new SKColor(c.R, c.G, c.B, c.A);

            return new PieSeries<double>
            {
                Name = s.CategoryName,
                Values = [(double)s.Amount],
                Fill = new SolidColorPaint(sk, 1),
                Stroke = new SolidColorPaint(new SKColor(0x1E, 0x1E, 0x28, 0xFF), 4),
                InnerRadius = 30,
                DataLabelsSize = 0
            };
        }).ToArray<ISeries>();

        foreach (var s in slices.OrderByDescending(x => x.Amount))
        {
            var pct = (double)(s.Amount / total);
            var hex = palette[ColorIndex(s.CategoryName)];
            CategoryLegend.Add(new CategoryLegendItem
            {
                Name = s.CategoryName,
                PercentText = $"— {pct:0%}",
                ColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)!)
            });
        }

        return;

        int ColorIndex(string name)
            => Math.Abs(name.GetHashCode()) % palette.Length;
    }
    
    private void FillTransactions(DashboardData data)
    {
        RecentTransactions.Clear();

        foreach (var t in data.Transactions
                     .OrderByDescending(x => x.DateUtc)
                     .Take(3))
        {
            RecentTransactions.Add(new TransactionRow
            {
                IconText = BuildIconText(t),
                Title = string.IsNullOrWhiteSpace(t.Comment) ? "(без коментаря)" : t.Comment,
                Category = t.Category?.Name ?? "Other", 
                Date = t.DateUtc.ToLocalTime().ToString("dd.MM.yy"),
                Amount = FormatAmount(t),
                Type = t.Type
            });
        }
    }

    private static string Money(decimal value) => $"₴ {value:N0}";

    private static string FormatBadge(double? pct)
    {
        if (pct is null) return "";
        var sign = pct >= 0 ? "▲" : "▼";
        return $"{sign} {Math.Abs(pct.Value):0.0%}";
    }
    
    private static string FormatAmount(Transaction t)
    {
        var sign = t.Type == TransactionType.Expense ? "−" : "+";
        return $"{sign} ₴ {t.Amount:N0}";
    }

    private static string BuildIconText(Transaction t)
    {
        if (t.Type == TransactionType.Income) return "₴";

        var name = t.Category?.Name ?? "?";
        return string.IsNullOrEmpty(name) ? "•" : name[0].ToString().ToUpperInvariant();
    }
}