using Spendly.Application.Handlers.Analytics;
using Spendly.Application.Handlers.Analytics.Requests;
using Spendly.Tests.Budgets;
using FluentAssertions;
using Xunit;

namespace Spendly.Tests.Analytics;

public class AnalyticsTrendHandlerTests
{
    [Fact]
    public async Task Should_Return_6_Months_Data()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var account = builder.AddAccount();
        var groceries = builder.AddCategory("Groceries");

        var now = DateTime.UtcNow;

        for (int i = 0; i < 6; i++)
        {
            var date = now.AddMonths(-i);
            builder.AddExpense(account, groceries, 100m * (i + 1),
                new DateTime(date.Year, date.Month, 5));
        }

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new AnalyticsTrendHandler(db);

        var result = await handler.Handle(
            new GetAnalyticsTrendDataRequest(groceries),
            CancellationToken.None);

        result.Points.Should().HaveCount(6);
    }

    [Fact]
    public async Task Should_Filter_By_Category()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var account = builder.AddAccount();
        var groceries = builder.AddCategory("Groceries");
        var cafes = builder.AddCategory("Cafes");

        var now = DateTime.UtcNow;

        builder.AddExpense(account, groceries, 500m, now);
        builder.AddExpense(account, cafes, 999m, now);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new AnalyticsTrendHandler(db);

        var result = await handler.Handle(
            new GetAnalyticsTrendDataRequest(groceries),
            CancellationToken.None);

        result.Points.Last().Amount.Should().Be(500m);
    }

    [Fact]
    public async Task Should_Ignore_Income()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var account = builder.AddAccount();
        var groceries = builder.AddCategory("Groceries");

        var now = DateTime.UtcNow;

        builder.AddExpense(account, groceries, 300m, now);
        builder.AddIncome(account, 5000m, now);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new AnalyticsTrendHandler(db);

        var result = await handler.Handle(
            new GetAnalyticsTrendDataRequest(groceries),
            CancellationToken.None);

        result.Points.Last().Amount.Should().Be(300m);
    }
}