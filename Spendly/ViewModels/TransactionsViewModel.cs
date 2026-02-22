using Spendly.Views;

namespace Spendly.ViewModels;

public class TransactionsViewModel : BaseViewModel
{
	public TransactionsViewModel()
	{
		HeaderView = new TransactionsHeaderView();
	}
}