using FluentAssertions;
using Spendly.Application.Handlers.Analytics;
using Spendly.Application.Handlers.Analytics.Requests;
using Spendly.Tests.Budgets;
using Xunit;

namespace Spendly.Tests.Analytics;

public class AnalyticsPageHandlerTests
{
    [Fact]
    public async Task Should_Return_Overspent_And_Underused_Categories()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var account = builder.AddAccount();
        var groceries = builder.AddCategory("Groceries");
        var cafes = builder.AddCategory("Cafes");

        builder.AddLimit(groceries, 1000m);
        builder.AddLimit(cafes, 3000m);

        var now = DateTime.UtcNow;
        var date = new DateTime(now.Year, now.Month, 10);

        builder.AddExpense(account, groceries, 1400m, date);
        builder.AddExpense(account, cafes, 400m, date);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new AnalyticsPageHandler(db);

        var result = await handler.Handle(new GetAnalyticsDataRequest(), CancellationToken.None);

        result.TopSectionData.Overspent.HasData.Should().BeTrue();
        result.TopSectionData.Overspent.CategoryName.Should().Be("Groceries");
        result.TopSectionData.Overspent.Current.Should().Be(1400m);
        result.TopSectionData.Overspent.Limit.Should().Be(1000m);

        result.TopSectionData.Underused.HasData.Should().BeTrue();
        result.TopSectionData.Underused.CategoryName.Should().Be("Cafes");
        result.TopSectionData.Underused.Current.Should().Be(400m);
        result.TopSectionData.Underused.Limit.Should().Be(3000m);
    }
    
    [Fact]
    public async Task Should_Return_Risk_Items()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var account = builder.AddAccount();
        var groceries = builder.AddCategory("Groceries");
        var transport = builder.AddCategory("Transport");
        var cafes = builder.AddCategory("Cafes");

        builder.AddLimit(groceries, 1000m); // 85%
        builder.AddLimit(transport, 1000m); // 92%
        builder.AddLimit(cafes, 1000m);     // 40%

        var now = DateTime.UtcNow;
        var date = new DateTime(now.Year, now.Month, 10);

        builder.AddExpense(account, groceries, 850m, date);
        builder.AddExpense(account, transport, 920m, date);
        builder.AddExpense(account, cafes, 400m, date);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new AnalyticsPageHandler(db);

        var result = await handler.Handle(new GetAnalyticsDataRequest(), CancellationToken.None);

        result.TopSectionData.RiskItems.Should().HaveCount(2);
        result.TopSectionData.RiskItems[0].CategoryName.Should().Be("Transport");
        result.TopSectionData.RiskItems[1].CategoryName.Should().Be("Groceries");
    }
    
    [Fact]
    public async Task Should_Return_Empty_Overspent_When_No_Category_Exceeded_Limit()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var account = builder.AddAccount();
        var groceries = builder.AddCategory("Groceries");
        var cafes = builder.AddCategory("Cafes");

        builder.AddLimit(groceries, 2000m);
        builder.AddLimit(cafes, 1500m);

        var now = DateTime.UtcNow;
        var date = new DateTime(now.Year, now.Month, 10);

        builder.AddExpense(account, groceries, 800m, date);
        builder.AddExpense(account, cafes, 600m, date);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new AnalyticsPageHandler(db);

        var result = await handler.Handle(new GetAnalyticsDataRequest(), CancellationToken.None);

        result.TopSectionData.Overspent.HasData.Should().BeFalse();
    }
    
    [Fact]
    public async Task Should_Calculate_Account_Shares()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var acc1 = builder.AddAccount();
        var acc2 = builder.AddAccount();

        var groceries = builder.AddCategory("Groceries");

        var now = DateTime.UtcNow;

        builder.AddExpense(acc1, groceries, 1000m, now);
        builder.AddExpense(acc2, groceries, 500m, now);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new AnalyticsPageHandler(db);

        var result = await handler.Handle(new GetAnalyticsDataRequest(), CancellationToken.None);

        var items = result.AccountsSectionData.Items;

        items.Should().HaveCount(2);

        items[0].Share.Should().BeApproximately(0.666, 0.01);
        items[1].Share.Should().BeApproximately(0.333, 0.01);
    }

    [Fact]
    public async Task Should_Order_By_Amount_Descending()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var acc1 = builder.AddAccount();
        var acc2 = builder.AddAccount();

        var groceries = builder.AddCategory("Groceries");

        var now = DateTime.UtcNow;

        builder.AddExpense(acc1, groceries, 100m, now);
        builder.AddExpense(acc2, groceries, 1000m, now);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new AnalyticsPageHandler(db);

        var result = await handler.Handle(new GetAnalyticsDataRequest(), CancellationToken.None);

        var items = result.AccountsSectionData.Items;

        items[0].Amount.Should().Be(1000m);
        items[1].Amount.Should().Be(100m);
    }

    [Fact]
    public async Task Should_Ignore_Income_In_Accounts()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var acc = builder.AddAccount();
        var groceries = builder.AddCategory("Groceries");

        var now = DateTime.UtcNow;

        builder.AddExpense(acc, groceries, 300m, now);
        builder.AddIncome(acc, 5000m, now);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new AnalyticsPageHandler(db);

        var result = await handler.Handle(new GetAnalyticsDataRequest(), CancellationToken.None);

        result.AccountsSectionData.Items[0].Amount.Should().Be(300m);
    }
}