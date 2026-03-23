using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spendly.Domain.Enums;

namespace Spendly.ViewModels.Transactions;

public partial class EditTransactionViewModel : ObservableObject
{
    [ObservableProperty] private bool _isOpen;

    [ObservableProperty] private Guid _transactionId;
    [ObservableProperty] private decimal _amount;
    [ObservableProperty] private string _comment = string.Empty;
    [ObservableProperty] private TransactionType _type;

    [ObservableProperty] private DateTime _selectedDate = DateTime.Today;

    public IAsyncRelayCommand? SaveCommand { get; set; }

    [RelayCommand]
    private void Close()
    {
        IsOpen = false;
    }

    public void Open(Guid id, decimal amount, DateTime dateUtc, string comment, TransactionType type)
    {
        TransactionId = id;
        Amount = amount;
        Comment = comment;
        Type = type;
        SelectedDate = dateUtc.Date;
        IsOpen = true;
    }
}