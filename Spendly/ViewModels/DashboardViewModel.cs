using System.Collections.ObjectModel;
using System.Windows.Media;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Spendly.Views;

namespace Spendly.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    // Bar chart
    public ISeries[] IncomeExpenseSeries { get; }
    public Axis[] MonthsAxis { get; }
    public Axis[] MoneyAxis { get; }

    // Donut chart
    public ISeries[] CategorySeries { get; }
    public ObservableCollection<CategoryLegendItem> CategoryLegend { get; } = new();

    // Transactions table
    public ObservableCollection<TransactionItem> RecentTransactions { get; } = new();

    public DashboardViewModel()
    {
        HeaderView = new DashboardHeaderView();
        
        // months labels
        var months = new[] { "Вер", "Жов", "Лис", "Гру", "Січ", "Лют" };

        // hardcoded values
        double[] income =  { 12000, 22000, 15000, 21000, 28000, 22000 };
        double[] expense = {  9000, 15000, 17000, 20000, 30000, 19000 };

        // Series: two column series
        IncomeExpenseSeries =
        [
            new ColumnSeries<double>
            {
                Name = "Доходи",
                Values = income,
                // Colors: take your palette vibe (green/red)
                Fill = new SolidColorPaint(new SKColor(0x43, 0xD9, 0xA2, 0x22)), // semi
                Stroke = new SolidColorPaint(new SKColor(0x43, 0xD9, 0xA2, 0xFF), 2),
                MaxBarWidth = 42
            },
            new ColumnSeries<double>
            {
                Name = "Витрати",
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
                Labels = months,
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

        // ======== DONUT ========
        // Hardcoded percents
        AddCategory("Продукти", 30, ColorFromHex("#6C63FF"));
        AddCategory("Транспорт", 22, ColorFromHex("#FF6584"));
        AddCategory("Розваги", 20, ColorFromHex("#43D9A2"));
        AddCategory("Комунал.", 12, ColorFromHex("#FFC94D"));
        AddCategory("Інше", 16, ColorFromHex("#8888AA"));

        CategorySeries =
        [
            CreatePie("Продукти", 30, "#6C63FF"),
            CreatePie("Транспорт", 22, "#FF6584"),
            CreatePie("Розваги", 20, "#43D9A2"),
            CreatePie("Комунал.", 12, "#FFC94D"),
            CreatePie("Інше", 16, "#8888AA"),
        ];

        // ======== TRANSACTIONS ========
        RecentTransactions.Add(new TransactionItem
        {
            IconText = "S",
            Title = "Супермаркет Сільпо",
            Category = "Продукти",
            DateText = "14.02.25",
            AmountText = "− ₴ 847",
            AmountBrush = new SolidColorBrush(ColorFromHex("#FF6584"))
        });

        RecentTransactions.Add(new TransactionItem
        {
            IconText = "₴",
            Title = "Зарплата",
            Category = "Дохід",
            DateText = "10.02.25",
            AmountText = "+ ₴ 28 000",
            AmountBrush = new SolidColorBrush(ColorFromHex("#43D9A2"))
        });

        RecentTransactions.Add(new TransactionItem
        {
            IconText = "A",
            Title = "Аптека АНЦ",
            Category = "Здоровʼя",
            DateText = "13.02.25",
            AmountText = "− ₴ 120",
            AmountBrush = new SolidColorBrush(ColorFromHex("#FF6584"))
        });
    }

    private void AddCategory(string name, int percent, Color color)
    {
        CategoryLegend.Add(new CategoryLegendItem
        {
            Name = name,
            PercentText = $"— {percent}%",
            ColorBrush = new SolidColorBrush(color)
        });
    }

    private static PieSeries<double> CreatePie(string name, double value, string hex)
    {
        var c = ColorFromHex(hex);
        var sk = new SKColor(c.R, c.G, c.B, c.A);

        return new PieSeries<double>
        {
            Name = name,
            Values = new[] { value },
            Fill = new SolidColorPaint(sk, 1),
            Stroke = new SolidColorPaint(new SKColor(0x1E, 0x1E, 0x28, 0xFF), 4),
            InnerRadius = 42, // donut hole
            DataLabelsSize = 0
        };
    }

    private static Color ColorFromHex(string hex)
        => (Color)ColorConverter.ConvertFromString(hex)!;
}

public sealed class CategoryLegendItem
{
    public string Name { get; set; } = "";
    public string PercentText { get; set; } = "";
    public Brush ColorBrush { get; set; } = Brushes.Transparent;
}

public sealed class TransactionItem
{
    public string IconText { get; set; } = "";
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public string DateText { get; set; } = "";
    public string AmountText { get; set; } = "";
    public Brush AmountBrush { get; set; } = Brushes.White;
}
