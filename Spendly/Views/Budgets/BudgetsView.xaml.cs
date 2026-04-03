using System.Windows;
using System.Windows.Controls;
using Spendly.ViewModels.Budgets;
using Spendly.ViewModels.Transactions;

namespace Spendly.Views.Budgets;

public partial class BudgetsView : UserControl
{
	private bool _isInitialized;
	
	public BudgetsView()
	{
		InitializeComponent();
		Loaded += BudgetsView_Loaded;
	}
	
	private async void BudgetsView_Loaded(object sender, RoutedEventArgs e)
	{
		if (_isInitialized)
			return;

		if (DataContext is not BudgetsViewModel vm)
			return;

		_isInitialized = true;
		await vm.LoadData();
	}
}