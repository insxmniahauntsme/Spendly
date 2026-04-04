using FluentAssertions;
using Spendly.Application.Handlers.Transactions;
using Spendly.Application.Handlers.Transactions.Requests;
using Spendly.Domain.Enums;
using Spendly.Tests.Budgets;
using Xunit;

namespace Spendly.Tests.Transactions;

public class ExportCsvHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Csv_File_With_Filtered_Transactions()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var mono = builder.AddAccount("Monobank");
        var groceries = builder.AddCategory("Groceries");

        builder.AddTransaction(
            mono,
            groceries,
            250m,
            TransactionType.Expense,
            new DateTime(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc),
            "Weekly groceries");

        builder.AddTransaction(
            mono,
            null,
            5000m,
            TransactionType.Income,
            new DateTime(2026, 3, 11, 10, 0, 0, DateTimeKind.Utc),
            "Salary");

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var fileName = $"test-export-{Guid.NewGuid():N}.csv";
        var handler = new ExportCsvHandler(db);

        var filePath = await handler.Handle(
            new ExportCsvRequest(
                fileName,
                TransactionType.Expense,
                new DateOnly(2026, 3, 1),
                groceries,
                mono,
                null),
            CancellationToken.None);

        File.Exists(filePath).Should().BeTrue();

        var content = await File.ReadAllTextAsync(filePath, TestContext.Current.CancellationToken);

        content.Should().Contain("transaction_date");
        content.Should().Contain("amount");
        content.Should().Contain("Weekly groceries");
        content.Should().Contain("Groceries");
        content.Should().Contain("Monobank");
        content.Should().NotContain("Salary");

        File.Delete(filePath);
    }
}