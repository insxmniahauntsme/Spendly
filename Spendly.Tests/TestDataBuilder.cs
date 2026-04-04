using Spendly.Application.Models.Transactions;
using Spendly.Data.Entities;
using Spendly.Domain.Enums;
using Spendly.Domain.Models;
using Spendly.Infrastructure;

namespace Spendly.Tests.Budgets;

public sealed class TestDataBuilder
{
    private readonly SpendlyDbContext _db;

    public TestDataBuilder(SpendlyDbContext db)
    {
        _db = db;
    }

    public Guid AddAccount(string name = "Test Account", AccountType type = AccountType.BankAccount, decimal balance = 0)
    {
        var account = new AccountEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Balance = balance,
            Type = type
        };

        _db.Accounts.Add(account);
        return account.Id;
    }

    public Guid AddCategory(string name)
    {
        var category = new CategoryEntity
        {
            Id = Guid.NewGuid(),
            Name = name
        };

        _db.Categories.Add(category);
        return category.Id;
    }

    public void AddLimit(Guid categoryId, decimal amount)
    {
        _db.CategoryLimits.Add(new CategoryLimitEntity
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            Amount = amount
        });
    }

    public Guid AddTransaction(
        Guid accountId,
        Guid? categoryId,
        decimal amount,
        TransactionType type,
        DateTime dateUtc,
        string comment = "")
    {
        var tx = new TransactionEntity
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            CategoryId = categoryId,
            Amount = amount,
            DateUtc = dateUtc,
            Type = type,
            Comment = comment
        };

        _db.Transactions.Add(tx);
        return tx.Id;
    }

    public void AddExpense(Guid accountId, Guid categoryId, decimal amount, DateTime dateUtc, string comment = "")
        => AddTransaction(accountId, categoryId, amount, TransactionType.Expense, dateUtc, comment);

    public void AddIncome(Guid accountId, decimal amount, DateTime dateUtc, string comment = "")
        => AddTransaction(accountId, null, amount, TransactionType.Income, dateUtc, comment);

    public static CreateTransactionModel BuildCreateTransactionModel(
        Guid accountId,
        Guid? categoryId,
        decimal amount,
        TransactionType type,
        DateTime dateUtc,
        string comment = "")
    {
        return new CreateTransactionModel(
            accountId,
            categoryId,
            amount,
            dateUtc,
            comment,
            type);
    }

    public static UpdateTransactionModel BuildUpdateTransactionModel(
        decimal amount,
        DateTime dateUtc,
        TransactionType type,
        string comment = "")
    {
        return new UpdateTransactionModel(amount, dateUtc, comment, type);   
    }
}