using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Spendly.Application.Handlers.Transactions.Requests;
using Spendly.Application.Models.Transactions;
using Spendly.Controls.Notifications;
using Spendly.Domain.Enums;
using Spendly.Helpers;
using Spendly.Infrastructure.Queries;
using Account = Spendly.Models.Account;
using Category = Spendly.Models.Category;
using DomainTransaction = Spendly.Domain.Models.Transaction;
using UiTransaction = Spendly.Models.Transaction;

namespace Spendly.ViewModels.Transactions;

public partial class TransactionsViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly CategoryQueries _categoryQueries;
    private readonly AccountQueries _accountQueries;

    private CancellationTokenSource? _searchDebounceCts;
    private CancellationTokenSource? _toastHideCts;
    
    [ObservableProperty] private AddTransactionViewModel _addTransaction = new();
    [ObservableProperty] private EditTransactionViewModel _editTransaction = new();

    [ObservableProperty] private bool _isCategoryPickerOpen;
    [ObservableProperty] private bool _isAccountPickerOpen;

    [ObservableProperty] private Guid? _selectedCategoryId;
    [ObservableProperty] private Guid? _selectedAccountId;
    [ObservableProperty] private DateOnly? _selectedMonth;

    [ObservableProperty] private string _searchText = string.Empty;

    [ObservableProperty] private ObservableCollection<Category> _categories = [];
    [ObservableProperty] private ObservableCollection<Account> _accounts = [];
    [ObservableProperty] private ObservableCollection<UiTransaction> _transactions = [];
    [ObservableProperty] private ObservableCollection<PaginationItem> _visiblePages = [];
    
    [ObservableProperty] private decimal _totalExpenses;
    [ObservableProperty] private decimal _totalIncome;

    [ObservableProperty] private TransactionTypeFilter _selectedTypeFilter = TransactionTypeFilter.All;

    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _pageSize = 10;
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private int _totalPages;

    [ObservableProperty] private bool _isToastVisible;
    [ObservableProperty] private string _toastTitle = string.Empty;
    [ObservableProperty] private string _toastMessage = string.Empty;
    [ObservableProperty] private ToastVariant _toastVariant = ToastVariant.Info;
    [ObservableProperty] private bool _isToastClickable;
    [ObservableProperty] private string? _lastExportedFilePath;

    public string CategoryTitle =>
        Categories.FirstOrDefault(x => x.Id == SelectedCategoryId)?.Name ?? "All categories";

    public string AccountTitle =>
        Accounts.FirstOrDefault(x => x.Id == SelectedAccountId)?.Name ?? "All accounts";

    public string CategoryIcon =>
        Categories.FirstOrDefault(x => x.Id == SelectedCategoryId)?.IconSource
        ?? CategoryIconProvider.GetIcon("Other");

    public string RecordsShownText => TotalCount.ToString();

    public string TotalExpensesText => $"₴ {TotalExpenses:N0}";
    public string TotalIncomeText => $"₴ {TotalIncome:N0}";

    public string ShowingText =>
        TotalCount == 0
            ? "Showing 0 of 0"
            : $"Showing {(CurrentPage - 1) * PageSize + 1}–{Math.Min(CurrentPage * PageSize, TotalCount)} of {TotalCount}";

    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext => CurrentPage < TotalPages;

    public TransactionsViewModel(
        CategoryQueries categoryQueries,
        AccountQueries accountQueries,
        IMediator mediator)
    {
        _categoryQueries = categoryQueries;
        _accountQueries = accountQueries;
        _mediator = mediator;
        
        AddTransaction.SaveCommand = SaveNewTransactionCommand;
        EditTransaction.SaveCommand = SaveEditedTransactionCommand;
    }

    [RelayCommand]
    private void SetAll() => SelectedTypeFilter = TransactionTypeFilter.All;

    [RelayCommand]
    private void SetExpenses() => SelectedTypeFilter = TransactionTypeFilter.Expenses;

    [RelayCommand]
    private void SetIncome() => SelectedTypeFilter = TransactionTypeFilter.Income;

    [RelayCommand]
    private async Task GoToPreviousPage()
    {
        if (!CanGoPrevious)
            return;

        CurrentPage--;
        await LoadTransactions();
    }

    [RelayCommand]
    private async Task GoToNextPage()
    {
        if (!CanGoNext)
            return;

        CurrentPage++;
        await LoadTransactions();
    }

    [RelayCommand]
    private async Task GoToPage(int page)
    {
        if (page < 1 || page > TotalPages || page == CurrentPage)
            return;

        CurrentPage = page;
        await LoadTransactions();
    }

    [RelayCommand]
    private async Task ExportCsv()
    {
        TransactionType? type = SelectedTypeFilter switch
        {
            TransactionTypeFilter.All => null,
            TransactionTypeFilter.Expenses => TransactionType.Expense,
            TransactionTypeFilter.Income => TransactionType.Income,
            _ => null
        };
        
        try
        {
            var request = new ExportCsvRequest(
                BuildFileName(),
                type,
                SelectedMonth,
                SelectedCategoryId,
                SelectedAccountId,
                SearchText);
            
            LastExportedFilePath = await _mediator.Send(request);

            ShowToast(
                "CSV exported successfully",
                "Click to open export folder",
                ToastVariant.Success,
                isClickable: true);
        }
        catch
        {
            LastExportedFilePath = null;

            ShowToast(
                "Failed to export CSV",
                "Please try again",
                ToastVariant.Danger,
                isClickable: false);
        }
    }

    [RelayCommand]
    private void OpenExportFolder()
    {
        if (string.IsNullOrWhiteSpace(LastExportedFilePath))
            return;

        var directory = Path.GetDirectoryName(LastExportedFilePath);
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            return;

        _toastHideCts?.Cancel();
        IsToastVisible = false;

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"\"{directory}\"",
            UseShellExecute = true
        });
    }
    
    [RelayCommand]
    private void OpenAddTransaction()
    {
        AddTransaction.Open(Categories, Accounts);
    }
    
    [RelayCommand]
    private void OpenEditTransaction(UiTransaction transaction)
    {
        EditTransaction.Open(
            transaction.Id,
            transaction.Amount,
            transaction.DateUtc,
            transaction.Comment,
            transaction.Type);
    }

    [RelayCommand]
    private async Task DeleteTransaction(Guid id)
    {
        await _mediator.Send(new DeleteTransactionRequest(id));
        await LoadTransactions();
    }
    
    [RelayCommand]
    private async Task SaveNewTransaction()
    {
        var model = new CreateTransactionModel(
            AddTransaction.SelectedAccountId!.Value,
            AddTransaction.SelectedCategoryId,
            AddTransaction.Amount,
            DateTime.SpecifyKind(AddTransaction.SelectedDate.Date, DateTimeKind.Utc),
            AddTransaction.Comment,
            AddTransaction.Type);
        
        var request = new CreateTransactionRequest(model);

        await _mediator.Send(request);

        AddTransaction.IsOpen = false;
        await LoadTransactions();
    }
    
    [RelayCommand]
    private async Task SaveEditedTransaction()
    {
        var request = new UpdateTransactionRequest(
            EditTransaction.TransactionId,
            new UpdateTransactionModel(
                EditTransaction.Amount,
                EditTransaction.SelectedDate,
                EditTransaction.Comment,
                EditTransaction.Type));

        await _mediator.Send(request);

        EditTransaction.IsOpen = false;
        await LoadTransactions();
    }

    [RelayCommand]
    private void DismissToast()
    {
        _toastHideCts?.Cancel();
        IsToastVisible = false;
    }

    public async Task InitializeAsync()
    {
        await EnsureLookupsLoadedAsync();
        await LoadTransactions();
    }

    async partial void OnIsCategoryPickerOpenChanged(bool value)
    {
        if (value && Categories.Count == 0)
            await LoadCategories();
    }

    async partial void OnIsAccountPickerOpenChanged(bool value)
    {
        if (value && Accounts.Count == 0)
            await LoadAccounts();
    }

    partial void OnSelectedMonthChanged(DateOnly? value)
    {
        CurrentPage = 1;
        _ = LoadTransactions();
    }

    partial void OnSelectedCategoryIdChanged(Guid? value)
    {
        IsCategoryPickerOpen = false;
        OnPropertyChanged(nameof(CategoryTitle));
        OnPropertyChanged(nameof(CategoryIcon));
        CurrentPage = 1;
        _ = LoadTransactions();
    }

    partial void OnSelectedAccountIdChanged(Guid? value)
    {
        IsAccountPickerOpen = false;
        OnPropertyChanged(nameof(AccountTitle));
        CurrentPage = 1;
        _ = LoadTransactions();
    }

    partial void OnSelectedTypeFilterChanged(TransactionTypeFilter value)
    {
        CurrentPage = 1;
        _ = LoadTransactions();
    }

    partial void OnSearchTextChanged(string? value)
    {
        var text = value?.Trim();

        _searchDebounceCts?.Cancel();
        _searchDebounceCts = new CancellationTokenSource();
        var token = _searchDebounceCts.Token;

        if (!string.IsNullOrEmpty(text) && text.Length < 3)
            return;

        CurrentPage = 1;
        _ = DebounceSearchAsync(token);
    }

    private async Task DebounceSearchAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(300, cancellationToken);
            await LoadTransactions();
        }
        catch (TaskCanceledException)
        {
        }
    }

    private async Task EnsureLookupsLoadedAsync()
    {
        if (Categories.Count == 0)
            await LoadCategories();

        if (Accounts.Count == 0)
            await LoadAccounts();
    }

    private async Task LoadCategories()
    {
        Categories.Clear();

        var items = await _categoryQueries.GetCategoriesAsync();
        foreach (var item in items)
            Categories.Add(new Category(item.Id, item.Name));

        OnPropertyChanged(nameof(CategoryTitle));
        OnPropertyChanged(nameof(CategoryIcon));
    }

    private async Task LoadAccounts()
    {
        Accounts.Clear();

        var items = await _accountQueries.GetAccountsAsync();
        foreach (var item in items)
            Accounts.Add(new Account(item.Id, item.Name, item.Balance, item.Type));

        OnPropertyChanged(nameof(AccountTitle));
    }

    private async Task LoadTransactions()
    {
        TransactionType? type = SelectedTypeFilter switch
        {
            TransactionTypeFilter.All => null,
            TransactionTypeFilter.Expenses => TransactionType.Expense,
            TransactionTypeFilter.Income => TransactionType.Income,
            _ => null
        };

        var searchTerm = string.IsNullOrWhiteSpace(SearchText)
            ? null
            : SearchText.Trim();

        var request = new GetTransactionsRequest(
            type,
            SelectedMonth,
            SelectedCategoryId,
            SelectedAccountId,
            searchTerm)
        {
            Page = CurrentPage,
            PageSize = PageSize
        };

        var response = await _mediator.Send(request);

        Transactions.Clear();

        foreach (var transaction in response.Page.Items
                     .OrderByDescending(x => x.DateUtc)
                     .Select(MapToUiTransaction))
        {
            Transactions.Add(transaction);
        }

        TotalCount = response.Page.TotalCount;
        TotalPages = response.Page.TotalPages;

        TotalExpenses = response.TotalExpenses;
        TotalIncome = response.TotalIncomes;

        RebuildVisiblePages();
        RaiseAllComputedProperties();
    }

    private static UiTransaction MapToUiTransaction(DomainTransaction transaction)
    {
        var categoryName = string.IsNullOrWhiteSpace(transaction.Category?.Name)
            ? "Other"
            : transaction.Category!.Name;

        var comment = string.IsNullOrWhiteSpace(transaction.Comment)
            ? "Untitled transaction"
            : transaction.Comment;

        return new UiTransaction(
            transaction.Id,
            transaction.AccountId,
            transaction.CategoryId,
            transaction.Amount,
            transaction.DateUtc,
            transaction.Type,
            comment,
            transaction.Account.Name,
            categoryName,
            CategoryIconProvider.GetIcon(categoryName),
            FormatAmount(transaction.Amount, transaction.Type),
            transaction.DateUtc.ToString("dd.MM.yy"));
    }

    private static DomainTransaction MapToDomainTransaction(UiTransaction transaction)
    {
        return new DomainTransaction
        {
            Id = transaction.Id,
            AccountId = transaction.AccountId,
            CategoryId = transaction.CategoryId,
            Amount = transaction.Amount,
            DateUtc = transaction.DateUtc,
            Type = transaction.Type,
            Comment = transaction.Comment
        };
    }

    private static string FormatAmount(decimal amount, TransactionType type)
    {
        var sign = type == TransactionType.Expense ? "-" : "+";
        return $"{sign} ₴ {amount:N0}";
    }

    private void RebuildVisiblePages()
    {
        VisiblePages.Clear();

        if (TotalPages <= 0)
            return;

        foreach (var item in BuildPagination(CurrentPage, TotalPages))
            VisiblePages.Add(item);
    }

    private static List<PaginationItem> BuildPagination(int currentPage, int totalPages)
    {
        var result = new List<PaginationItem>();

        switch (totalPages)
        {
            case <= 0:
                return result;
            case <= 5:
            {
                for (var i = 1; i <= totalPages; i++)
                    result.Add(CreatePageItem(i, currentPage));

                return result;
            }
        }

        if (currentPage <= 3)
        {
            result.Add(CreatePageItem(1, currentPage));
            result.Add(CreatePageItem(2, currentPage));
            result.Add(CreatePageItem(3, currentPage));
            result.Add(CreateEllipsis());
            result.Add(CreatePageItem(totalPages, currentPage));

            return result;
        }

        if (currentPage >= totalPages - 2)
        {
            result.Add(CreatePageItem(1, currentPage));
            result.Add(CreateEllipsis());
            result.Add(CreatePageItem(totalPages - 2, currentPage));
            result.Add(CreatePageItem(totalPages - 1, currentPage));
            result.Add(CreatePageItem(totalPages, currentPage));

            return result;
        }

        result.Add(CreatePageItem(1, currentPage));
        result.Add(CreateEllipsis());
        result.Add(CreatePageItem(currentPage, currentPage));
        result.Add(CreatePageItem(currentPage + 1, currentPage));
        result.Add(CreatePageItem(totalPages, currentPage));

        return result;
    }

    private static PaginationItem CreatePageItem(int page, int currentPage) =>
        new()
        {
            Page = page,
            Text = page.ToString(),
            IsCurrent = page == currentPage
        };

    private static PaginationItem CreateEllipsis() =>
        new()
        {
            Page = null,
            Text = "..."
        };

    private void RaiseAllComputedProperties()
    {
        OnPropertyChanged(nameof(RecordsShownText));
        OnPropertyChanged(nameof(TotalExpensesText));
        OnPropertyChanged(nameof(TotalIncomeText));
        OnPropertyChanged(nameof(ShowingText));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
    }

    private void ShowToast(
        string title,
        string message,
        ToastVariant variant,
        bool isClickable,
        int durationMs = 4000)
    {
        _toastHideCts?.Cancel();
        _toastHideCts = new CancellationTokenSource();

        ToastTitle = title;
        ToastMessage = message;
        ToastVariant = variant;
        IsToastClickable = isClickable;
        IsToastVisible = true;

        _ = HideToastLaterAsync(durationMs, _toastHideCts.Token);
    }

    private async Task HideToastLaterAsync(int durationMs, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(durationMs, cancellationToken);
            IsToastVisible = false;
        }
        catch (TaskCanceledException)
        {
        }
    }

    private string BuildFileName()
    {
        var parts = new List<string>
        {
            "Spendly",
            "transactions",
            SelectedTypeFilter.ToString().ToLowerInvariant()
        };

        if (SelectedCategoryId is not null)
            parts.Add(NormalizeFileName(CategoryTitle));

        if (SelectedAccountId is not null)
            parts.Add(NormalizeFileName(AccountTitle));

        parts.Add(SelectedMonth.HasValue ? SelectedMonth.Value.ToString("MMMM-yyyy") : "all-time");

        parts.Add(DateTime.Now.ToString("yyyyMMdd-HHmmss"));

        return string.Join("-", parts) + ".csv";

        string NormalizeFileName(string value)
        {
            var invalid = Path.GetInvalidFileNameChars();

            value = invalid.Aggregate(value, (current, c) => current.Replace(c, '_'));

            return value
                .Replace(" ", "-")
                .ToLowerInvariant();
        }
    }
}