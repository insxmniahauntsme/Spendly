using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Spendly.Application.Handlers.Transactions;
using Spendly.Application.Handlers.Transactions.Requests;
using Spendly.Domain.Enums;
using Spendly.Tests.Budgets;
using Xunit;

namespace Spendly.Tests.Transactions;

public class UpdateTransactionHandlerTests
{
    [Fact]
    public async Task Handle_Should_Update_Existing_Transaction()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var accountId = builder.AddAccount("Mono");
        var categoryId = builder.AddCategory("Groceries");

        var transactionId = builder.AddTransaction(
            accountId,
            categoryId,
            300m,
            TransactionType.Expense,
            new DateTime(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc),
            "Old");

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var model = TestDataBuilder.BuildUpdateTransactionModel(
            777m,
            new DateTime(2026, 3, 11, 10, 0, 0, DateTimeKind.Utc),
            TransactionType.Expense,
            "Updated");

        var handler = new UpdateTransactionHandler(db);

        await handler.Handle(
            new UpdateTransactionRequest(transactionId, model),
            TestContext.Current.CancellationToken);

        var entity = await db.Transactions.FirstAsync(
            x => x.Id == transactionId,
            TestContext.Current.CancellationToken);

        entity.Amount.Should().Be(777m);
        entity.Type.Should().Be(TransactionType.Expense);
        entity.Comment.Should().Be("Updated");
        entity.DateUtc.Should().Be(new DateTime(2026, 3, 11, 10, 0, 0, DateTimeKind.Utc));

        entity.CategoryId.Should().Be(categoryId);
        entity.AccountId.Should().Be(accountId);
    }

    [Fact]
    public async Task Handle_Should_Do_Nothing_When_Transaction_Does_Not_Exist()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var accountId = builder.AddAccount();
        var categoryId = builder.AddCategory("Groceries");

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var model = TestDataBuilder.BuildUpdateTransactionModel(
            500m,
            new DateTime(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc),
            TransactionType.Expense,
            "Comment");

        var handler = new UpdateTransactionHandler(db);

        var act = async () =>
            await handler.Handle(
                new UpdateTransactionRequest(Guid.NewGuid(), model),
                TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
    }
}