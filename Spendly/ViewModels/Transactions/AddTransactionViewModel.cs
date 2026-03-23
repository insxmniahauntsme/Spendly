using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spendly.Domain.Enums;
using Spendly.Helpers;
using Spendly.Models;

namespace Spendly.ViewModels.Transactions;

public partial class AddTransactionViewModel : ObservableObject
{
    [ObservableProperty] private bool _isOpen;

    [ObservableProperty] private decimal _amount;
    [ObservableProperty] private string _comment = string.Empty;
    [ObservableProperty] private TransactionType _type = TransactionType.Expense;
    [ObservableProperty] private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty] private bool _isCategoryPickerOpen;
    [ObservableProperty] private bool _isAccountPickerOpen;

    [ObservableProperty] private Guid? _selectedCategoryId;
    [ObservableProperty] private Guid? _selectedAccountId;

    [ObservableProperty] private ObservableCollection<Category> _categories = [];
    [ObservableProperty] private ObservableCollection<Account> _accounts = [];

    public IAsyncRelayCommand? SaveCommand { get; set; }

    public string CategoryTitle =>
        Categories.FirstOrDefault(x => x.Id == SelectedCategoryId)?.Name ?? "Select category";

    public string AccountTitle =>
        Accounts.FirstOrDefault(x => x.Id == SelectedAccountId)?.Name ?? "Select account";

    public string CategoryIcon =>
        Categories.FirstOrDefault(x => x.Id == SelectedCategoryId)?.IconSource
        ?? CategoryIconProvider.GetIcon("Other");

    [RelayCommand]
    private void Close()
    {
        IsOpen = false;
    }

    public void Open(
        IEnumerable<Category> categories,
        IEnumerable<Account> accounts,
        Guid? preselectedCategoryId = null,
        Guid? preselectedAccountId = null)
    {
        Categories.Clear();
        foreach (var category in categories)
            Categories.Add(category);

        Accounts.Clear();
        foreach (var account in accounts)
            Accounts.Add(account);

        Amount = 0;
        Comment = string.Empty;
        Type = TransactionType.Expense;
        SelectedDate = DateTime.Today;

        SelectedCategoryId = preselectedCategoryId;
        SelectedAccountId = preselectedAccountId;

        IsCategoryPickerOpen = false;
        IsAccountPickerOpen = false;
        IsOpen = true;

        OnPropertyChanged(nameof(CategoryTitle));
        OnPropertyChanged(nameof(AccountTitle));
        OnPropertyChanged(nameof(CategoryIcon));
    }

    partial void OnSelectedCategoryIdChanged(Guid? value)
    {
        IsCategoryPickerOpen = false;
        OnPropertyChanged(nameof(CategoryTitle));
        OnPropertyChanged(nameof(CategoryIcon));
    }

    partial void OnSelectedAccountIdChanged(Guid? value)
    {
        IsAccountPickerOpen = false;
        OnPropertyChanged(nameof(AccountTitle));
    }
}