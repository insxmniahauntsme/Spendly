using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spendly.ViewModels.Analytics;
using Spendly.ViewModels.Budgets;
using Spendly.ViewModels.Dashboard;
using Spendly.ViewModels.Transactions;

namespace Spendly.ViewModels;

public partial class MainLayoutViewModel : ObservableObject
{
	public DashboardViewModel DashboardVm { get; }
	public TransactionsViewModel TransactionsVm { get; }
	public AnalyticsViewModel AnalyticsVm { get; }
	public BudgetsViewModel BudgetsVm { get; }

	[ObservableProperty]
	private ObservableObject _currentVm = null!;

	[ObservableProperty]
	private bool _isDashboardSelected;

	[ObservableProperty]
	private bool _isTransactionsSelected;
	
	[ObservableProperty]
	private bool _isAnalyticsSelected;
	
	[ObservableProperty]
	private bool _isBudgetsSelected;

	public MainLayoutViewModel(
		DashboardViewModel dashboardVm,
		TransactionsViewModel transactionsVm,
		AnalyticsViewModel analyticsVm,
		BudgetsViewModel budgetsVm)
	{
		DashboardVm = dashboardVm;
		TransactionsVm = transactionsVm;
		AnalyticsVm = analyticsVm;
		BudgetsVm = budgetsVm;

		NavigateToDashboard();
	}

	[RelayCommand]
	private void NavigateToDashboard()
	{
		CurrentVm = DashboardVm;
		IsDashboardSelected = true;
		IsTransactionsSelected = false;
		IsAnalyticsSelected = false;
		IsBudgetsSelected = false;

	}

	[RelayCommand]
	private void NavigateToTransactions()
	{
		CurrentVm = TransactionsVm;
		IsDashboardSelected = false;
		IsTransactionsSelected = true;
		IsAnalyticsSelected = false;
		IsBudgetsSelected = false;

	}

	[RelayCommand]
	private void NavigateToAnalytics()
	{
		CurrentVm = AnalyticsVm;
		IsDashboardSelected = false;
		IsTransactionsSelected = false;
		IsAnalyticsSelected = true;
		IsBudgetsSelected = false;

	}
	
	[RelayCommand]
	private void NavigateToBudgets()
	{
		CurrentVm = BudgetsVm;
		IsDashboardSelected = false;
		IsTransactionsSelected = false;
		IsAnalyticsSelected = false;
		IsBudgetsSelected = true;
	}
}