using FluentAssertions;
using Spendly.Application.Handlers.Budgets;
using Spendly.Application.Handlers.Budgets.Requests;
using Xunit;

namespace Spendly.Tests.Budgets;

public class GetBudgetsDataHandlerTests
{
    [Fact]
    public async Task Should_Calculate_Budget_And_Spend()
    {
        // arrange
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var account = builder.AddAccount();
        var groceries = builder.AddCategory("Groceries");
        var cafes = builder.AddCategory("Cafes");

        builder.AddLimit(groceries, 2000m);
        builder.AddLimit(cafes, 1000m);

        var month = new DateOnly(2026, 3, 1);

        builder.AddExpense(account, groceries, 800m, new DateTime(2026, 3, 5));
        builder.AddExpense(account, cafes, 300m, new DateTime(2026, 3, 10));

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetBudgetsDataHandler(db);

        // act
        var result = await handler.Handle(new GetBudgetsDataRequest(month), CancellationToken.None);

        // assert
        result.TotalBudget.Should().Be(3000m);
        result.TotalSpend.Should().Be(1100m);
        result.Remaining.Should().Be(1900m);
    }
    
    [Fact]
    public async Task Should_Ignore_Income_Transactions()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var account = builder.AddAccount();
        var groceries = builder.AddCategory("Groceries");

        builder.AddLimit(groceries, 2000m);

        var month = new DateOnly(2026, 3, 1);

        builder.AddExpense(account, groceries, 600m, new DateTime(2026, 3, 5));
        builder.AddIncome(account, 5000m, new DateTime(2026, 3, 6));

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetBudgetsDataHandler(db);

        var result = await handler.Handle(new GetBudgetsDataRequest(month), CancellationToken.None);

        result.TotalBudget.Should().Be(2000m);
        result.TotalSpend.Should().Be(600m);
        result.Remaining.Should().Be(1400m);
    }
    
    [Fact]
    public async Task Should_Ignore_Transactions_From_Other_Months()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var account = builder.AddAccount();
        var groceries = builder.AddCategory("Groceries");

        builder.AddLimit(groceries, 2000m);

        var month = new DateOnly(2026, 3, 1);

        builder.AddExpense(account, groceries, 500m, new DateTime(2026, 3, 10));
        builder.AddExpense(account, groceries, 999m, new DateTime(2026, 2, 10));

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetBudgetsDataHandler(db);

        var result = await handler.Handle(new GetBudgetsDataRequest(month), CancellationToken.None);

        result.TotalSpend.Should().Be(500m);
    }
    
    [Fact]
    public async Task Should_Return_Zero_Spend_When_Category_Has_No_Transactions()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        builder.AddAccount();
        var groceries = builder.AddCategory("Groceries");

        builder.AddLimit(groceries, 2000m);

        var month = new DateOnly(2026, 3, 1);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetBudgetsDataHandler(db);

        var result = await handler.Handle(new GetBudgetsDataRequest(month), CancellationToken.None);

        result.TotalBudget.Should().Be(2000m);
        result.TotalSpend.Should().Be(0m);
        result.Remaining.Should().Be(2000m);
    }
}