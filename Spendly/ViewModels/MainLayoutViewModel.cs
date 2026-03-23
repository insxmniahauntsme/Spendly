using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spendly.ViewModels.Analytics;
using Spendly.ViewModels.Dashboard;
using Spendly.ViewModels.Transactions;

namespace Spendly.ViewModels;

public partial class MainLayoutViewModel : ObservableObject
{
	public DashboardViewModel DashboardVm { get; }
	public TransactionsViewModel TransactionsVm { get; }
	public AnalyticsViewModel AnalyticsVm { get; }

	[ObservableProperty]
	private ObservableObject _currentVm = null!;

	[ObservableProperty]
	private bool _isDashboardSelected;

	[ObservableProperty]
	private bool _isTransactionsSelected;
	
	[ObservableProperty]
	private bool _isAnalyticsSelected;

	public MainLayoutViewModel(
		DashboardViewModel dashboardVm,
		TransactionsViewModel transactionsVm,
		AnalyticsViewModel analyticsVm)
	{
		DashboardVm = dashboardVm;
		TransactionsVm = transactionsVm;
		AnalyticsVm = analyticsVm;

		NavigateToDashboard();
	}

	[RelayCommand]
	private void NavigateToDashboard()
	{
		CurrentVm = DashboardVm;
		IsDashboardSelected = true;
		IsTransactionsSelected = false;
		IsAnalyticsSelected = false;
	}

	[RelayCommand]
	private void NavigateToTransactions()
	{
		CurrentVm = TransactionsVm;
		IsDashboardSelected = false;
		IsTransactionsSelected = true;
		IsAnalyticsSelected = false;
	}

	[RelayCommand]
	private void NavigateToAnalytics()
	{
		CurrentVm = AnalyticsVm;
		IsDashboardSelected = false;
		IsTransactionsSelected = false;
		IsAnalyticsSelected = true;
	}
}