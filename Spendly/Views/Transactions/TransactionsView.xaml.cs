using System.Windows;
using System.Windows.Controls;
using Spendly.ViewModels.Transactions;

namespace Spendly.Views.Transactions;

public partial class TransactionsView : UserControl
{
	private bool _isInitialized;
	
	public TransactionsView()
	{
		InitializeComponent();
		Loaded += TransactionsView_Loaded;
	}
	
	private async void TransactionsView_Loaded(object sender, RoutedEventArgs e)
	{
		if (_isInitialized)
			return;

		if (DataContext is not TransactionsViewModel vm)
			return;

		_isInitialized = true;
		await vm.InitializeAsync();
	}
}