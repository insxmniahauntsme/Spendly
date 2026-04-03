using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MediatR;
using Spendly.Application.Handlers.Budgets;
using Spendly.Application.Handlers.Budgets.Requests;
using Spendly.Helpers;

namespace Spendly.ViewModels.Budgets;

public partial class BudgetsViewModel : ObservableObject
{
    private readonly IMediator _mediator;

    [ObservableProperty] private DateOnly _selectedMonth = new(DateTime.Now.Year, DateTime.Now.Month, 1);

    [ObservableProperty] private string _totalBudgetText = "₴ -";
    [ObservableProperty] private string _spentText = "₴ -";
    [ObservableProperty] private string _remainingText = "₴ -";

    [ObservableProperty] private string _overallStatusText = "-";
    [ObservableProperty] private string _overallSpentCaption = "₴ -";
    [ObservableProperty] private string _overallBudgetCaption = "₴ -";
    [ObservableProperty] private double _overallProgress;

    [ObservableProperty] private ObservableCollection<BudgetLimitRowItem> _limitRows = [];

    public BudgetsViewModel(IMediator mediator)
    {
        _mediator = mediator;
        _ = LoadData();
    }

    public async Task LoadData()
    {
        var data = await _mediator.Send(new GetBudgetsDataRequest(SelectedMonth));

        TotalBudgetText = Money(data.TotalBudget);
        SpentText = Money(data.TotalSpend);
        RemainingText = Money(data.Remaining);

        OverallSpentCaption = $"{Money(data.TotalSpend)} spent";
        OverallBudgetCaption = Money(data.TotalBudget);

        OverallProgress = data.TotalBudget <= 0
            ? 0
            : Math.Min((double)(data.TotalSpend / data.TotalBudget), 1.0);

        OverallStatusText = $"{OverallProgress:P0} used";

        BuildRows(data);
    }

    private void BuildRows(BudgetsPageData data)
    {
        LimitRows.Clear();

        foreach (var item in data.Items)
        {
            var progress = item.LimitAmount <= 0
                ? 0
                : Math.Min((double)(item.SpentAmount / item.LimitAmount), 1.0);

            var rawPercent = item.LimitAmount <= 0
                ? 0
                : (double)(item.SpentAmount / item.LimitAmount);

            var remaining = item.LimitAmount - item.SpentAmount;
            var isOverspent = remaining < 0;

            LimitRows.Add(new BudgetLimitRowItem
            {
                CategoryName = item.CategoryName,
                IconSource = CategoryIconProvider.GetIcon(item.CategoryName),
                CurrentText = Money(item.SpentAmount),
                LimitText = Money(item.LimitAmount),
                Progress = progress,
                ProgressPercentText = $"{rawPercent:P0}",
                RemainingText = isOverspent
                    ? $"Overspent by {Money(Math.Abs(remaining))}"
                    : $"Remaining {Money(remaining)}",
                IsOverspent = isOverspent
            });
        }

        LimitRows = new ObservableCollection<BudgetLimitRowItem>(
            LimitRows.OrderByDescending(x => x.Progress)
        );
    }

    private static string Money(decimal value) => $"₴ {value:N0}";
}

public partial class BudgetLimitRowItem : ObservableObject
{
    [ObservableProperty] private string _categoryName = "";
    [ObservableProperty] private string _iconSource = "";

    [ObservableProperty] private string _currentText = "";
    [ObservableProperty] private string _limitText = "";

    [ObservableProperty] private double _progress;

    [ObservableProperty] private string _progressPercentText = "";
    [ObservableProperty] private string _remainingText = "";
    [ObservableProperty] private bool _isOverspent;
}