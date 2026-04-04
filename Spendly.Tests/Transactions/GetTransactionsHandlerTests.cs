using FluentAssertions;
using Spendly.Application.Handlers.Transactions;
using Spendly.Application.Handlers.Transactions.Requests;
using Spendly.Domain.Enums;
using Spendly.Tests.Budgets;
using Xunit;

namespace Spendly.Tests.Transactions;

public class GetTransactionsHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Paged_Transactions_Ordered_By_Date_Descending()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var accountId = builder.AddAccount("Monobank");
        var categoryId = builder.AddCategory("Groceries");

        builder.AddTransaction(accountId, categoryId, 100m, TransactionType.Expense, new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc), "A");
        builder.AddTransaction(accountId, categoryId, 200m, TransactionType.Expense, new DateTime(2026, 3, 2, 10, 0, 0, DateTimeKind.Utc), "B");
        builder.AddTransaction(accountId, categoryId, 300m, TransactionType.Expense, new DateTime(2026, 3, 3, 10, 0, 0, DateTimeKind.Utc), "C");

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetTransactionsHandler(db);

        var request = new GetTransactionsRequest(
            Type: null,
            Month: null,
            CategoryId: null,
            AccountId: null,
            SearchTerm: null)
        {
            Page = 1,
            PageSize = 2
        };

        var result = await handler.Handle(request, CancellationToken.None);

        result.Page.Items.Should().HaveCount(2);
        result.Page.Items[0].Amount.Should().Be(300m);
        result.Page.Items[1].Amount.Should().Be(200m);
        result.Page.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task Handle_Should_Filter_By_Type_And_Calculate_Totals()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var accountId = builder.AddAccount("Monobank");
        var categoryId = builder.AddCategory("Groceries");

        builder.AddTransaction(accountId, categoryId, 100m, TransactionType.Expense, new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc), "Expense 1");
        builder.AddTransaction(accountId, categoryId, 200m, TransactionType.Expense, new DateTime(2026, 3, 2, 10, 0, 0, DateTimeKind.Utc), "Expense 2");
        builder.AddTransaction(accountId, null, 500m, TransactionType.Income, new DateTime(2026, 3, 3, 10, 0, 0, DateTimeKind.Utc), "Income");

        await db.SaveChangesAsync();

        var handler = new GetTransactionsHandler(db);

        var request = new GetTransactionsRequest(
            Type: TransactionType.Expense,
            Month: null,
            CategoryId: null,
            AccountId: null,
            SearchTerm: null);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Page.Items.Should().HaveCount(2);
        result.TotalExpenses.Should().Be(300m);
        result.TotalIncomes.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_Should_Filter_By_Month_Category_And_Account()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var mono = builder.AddAccount("Monobank");
        var privat = builder.AddAccount("PrivatBank");

        var groceries = builder.AddCategory("Groceries");
        var cafes = builder.AddCategory("Cafes");

        builder.AddTransaction(mono, groceries, 100m, TransactionType.Expense, new DateTime(2026, 3, 5, 10, 0, 0, DateTimeKind.Utc), "A");
        builder.AddTransaction(mono, cafes, 200m, TransactionType.Expense, new DateTime(2026, 3, 6, 10, 0, 0, DateTimeKind.Utc), "B");
        builder.AddTransaction(privat, groceries, 300m, TransactionType.Expense, new DateTime(2026, 3, 7, 10, 0, 0, DateTimeKind.Utc), "C");
        builder.AddTransaction(mono, groceries, 400m, TransactionType.Expense, new DateTime(2026, 2, 7, 10, 0, 0, DateTimeKind.Utc), "D");

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetTransactionsHandler(db);

        var request = new GetTransactionsRequest(
            Type: null,
            Month: new DateOnly(2026, 3, 1),
            CategoryId: groceries,
            AccountId: mono,
            SearchTerm: null);

        var result = await handler.Handle(request, CancellationToken.None);

        result.Page.Items.Should().HaveCount(1);
        result.Page.Items[0].Amount.Should().Be(100m);
    }
}