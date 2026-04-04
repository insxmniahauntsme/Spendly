using FluentAssertions;
using Spendly.Application.Handlers.Dashboard;
using Spendly.Application.Handlers.Dashboard.Requests;
using Spendly.Domain.Enums;
using Spendly.Tests.Budgets;
using Xunit;

namespace Spendly.Tests.Dashboard;

public class GetDashboardDataHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Selected_Month_Kpi_And_Current_Month_Transactions()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var mono = builder.AddAccount("Monobank");
        var groceries = builder.AddCategory("Groceries");
        var cafes = builder.AddCategory("Cafes");

        var selectedMonth = new DateOnly(2026, 3, 1);

        builder.AddIncome(mono, 1000m, new DateTime(2026, 2, 5, 10, 0, 0, DateTimeKind.Utc), "Prev income");
        builder.AddExpense(mono, groceries, 400m, new DateTime(2026, 2, 10, 10, 0, 0, DateTimeKind.Utc), "Prev expense");

        builder.AddIncome(mono, 1500m, new DateTime(2026, 3, 5, 10, 0, 0, DateTimeKind.Utc), "Curr income");
        builder.AddExpense(mono, groceries, 500m, new DateTime(2026, 3, 6, 10, 0, 0, DateTimeKind.Utc), "Groceries");
        builder.AddExpense(mono, cafes, 200m, new DateTime(2026, 3, 7, 10, 0, 0, DateTimeKind.Utc), "Cafe");

        builder.AddExpense(mono, groceries, 999m, new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc), "Next");

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetDashboardDataHandler(db);

        var result = await handler.Handle(
            new GetDashboardDataRequest(selectedMonth),
            CancellationToken.None);

        result.Kpi.MonthlyIncome.Should().Be(1500m);
        result.Kpi.MonthlyExpense.Should().Be(700m);
        result.Kpi.Balance.Should().Be(800m);

        result.Transactions.Should().HaveCount(3);
        result.Transactions.Should().OnlyContain(x =>
            DateOnly.FromDateTime(x.DateUtc) >= selectedMonth &&
            DateOnly.FromDateTime(x.DateUtc) < selectedMonth.AddMonths(1));
    }

    [Fact]
    public async Task Handle_Should_Return_All_Time_Kpi_When_Date_Is_Null()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var mono = builder.AddAccount("Monobank");
        var groceries = builder.AddCategory("Groceries");

        builder.AddIncome(mono, 2000m, new DateTime(2026, 1, 5, 10, 0, 0, DateTimeKind.Utc), "Income 1");
        builder.AddIncome(mono, 3000m, new DateTime(2026, 2, 5, 10, 0, 0, DateTimeKind.Utc), "Income 2");
        builder.AddExpense(mono, groceries, 800m, new DateTime(2026, 1, 6, 10, 0, 0, DateTimeKind.Utc), "Expense 1");
        builder.AddExpense(mono, groceries, 200m, new DateTime(2026, 2, 6, 10, 0, 0, DateTimeKind.Utc), "Expense 2");

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetDashboardDataHandler(db);

        var result = await handler.Handle(
            new GetDashboardDataRequest(null),
            CancellationToken.None);

        result.Kpi.MonthlyIncome.Should().Be(5000m);
        result.Kpi.MonthlyExpense.Should().Be(1000m);
        result.Kpi.IncomeChangePct.Should().BeNull();
        result.Kpi.ExpenseChangePct.Should().BeNull();
        result.Kpi.Balance.Should().Be(4000m);

        result.Transactions.Should().HaveCount(4);
        result.Transactions.Should().BeInDescendingOrder(x => x.DateUtc);
    }

    [Fact]
    public async Task Handle_Should_Return_Categories_Only_For_Expenses_With_Category()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var mono = builder.AddAccount("Monobank");
        var groceries = builder.AddCategory("Groceries");
        var cafes = builder.AddCategory("Cafes");

        var selectedMonth = new DateOnly(2026, 3, 1);

        builder.AddExpense(mono, groceries, 500m, new DateTime(2026, 3, 5, 10, 0, 0, DateTimeKind.Utc), "Groceries");
        builder.AddExpense(mono, cafes, 300m, new DateTime(2026, 3, 6, 10, 0, 0, DateTimeKind.Utc), "Cafe");
        builder.AddIncome(mono, 1000m, new DateTime(2026, 3, 7, 10, 0, 0, DateTimeKind.Utc), "Salary");
        builder.AddTransaction(mono, null, 250m, TransactionType.Expense, new DateTime(2026, 3, 8, 10, 0, 0, DateTimeKind.Utc), "No category");

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetDashboardDataHandler(db);

        var result = await handler.Handle(
            new GetDashboardDataRequest(selectedMonth),
            CancellationToken.None);

        result.Categories.Should().HaveCount(2);
        result.Categories.Should().Contain(x => x.CategoryName == "Groceries" && x.Amount == 500m);
        result.Categories.Should().Contain(x => x.CategoryName == "Cafes" && x.Amount == 300m);
    }

    [Fact]
    public async Task Handle_Should_Return_6_Month_Points_And_Fill_Missing_Months_With_Zero()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var mono = builder.AddAccount("Monobank");
        var groceries = builder.AddCategory("Groceries");

        var selectedMonth = new DateOnly(2026, 3, 1);

        builder.AddIncome(mono, 1000m, new DateTime(2025, 10, 5, 10, 0, 0, DateTimeKind.Utc), "Income Oct");
        builder.AddExpense(mono, groceries, 400m, new DateTime(2025, 12, 6, 10, 0, 0, DateTimeKind.Utc), "Expense Dec");
        builder.AddIncome(mono, 1500m, new DateTime(2026, 3, 7, 10, 0, 0, DateTimeKind.Utc), "Income Mar");

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetDashboardDataHandler(db);

        var result = await handler.Handle(
            new GetDashboardDataRequest(selectedMonth),
            CancellationToken.None);

        result.Last6Months.Should().HaveCount(6);

        result.Last6Months.Should().Contain(x =>
            x.MonthStart == new DateOnly(2025, 10, 1) &&
            x.Income == 1000m &&
            x.Expense == 0m);

        result.Last6Months.Should().Contain(x =>
            x.MonthStart == new DateOnly(2025, 11, 1) &&
            x.Income == 0m &&
            x.Expense == 0m);

        result.Last6Months.Should().Contain(x =>
            x.MonthStart == new DateOnly(2025, 12, 1) &&
            x.Income == 0m &&
            x.Expense == 400m);

        result.Last6Months.Should().Contain(x =>
            x.MonthStart == new DateOnly(2026, 3, 1) &&
            x.Income == 1500m &&
            x.Expense == 0m);
    }
}