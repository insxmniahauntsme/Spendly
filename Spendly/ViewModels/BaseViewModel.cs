using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Spendly.ViewModels;

public abstract class BaseViewModel : ObservableObject
{
	public UserControl HeaderView { get; protected set; } = null!;
}