using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spendly.ViewModels.Transactions;

namespace Spendly.ViewModels;

public partial class MainLayoutViewModel(
	Dashboard.DashboardViewModel dashboardVm,
	TransactionsViewModel transactionsVm)
	: ObservableObject
{
	private Dashboard.DashboardViewModel DashboardVm { get; } = dashboardVm;
	private TransactionsViewModel TransactionsVm { get; } = transactionsVm;

	[ObservableProperty] private ObservableObject _currentVm = dashboardVm;

	public bool IsDashboardSelected
	{
		get => CurrentVm == DashboardVm;
		set
		{
			if (value)
				CurrentVm = DashboardVm;
		}
	}
	
	public bool IsTransactionsSelected
	{
		get => CurrentVm == TransactionsVm;
		set
		{
			if (value)
				CurrentVm = TransactionsVm;
		}
	}

	[RelayCommand]
	private void ShowDashboard() => CurrentVm = DashboardVm;
	
	[RelayCommand]
	private void ShowTransactions() => CurrentVm = TransactionsVm;
}