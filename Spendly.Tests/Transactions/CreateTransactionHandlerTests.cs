using FluentAssertions;
using Spendly.Application.Handlers.Transactions;
using Spendly.Application.Handlers.Transactions.Requests;
using Spendly.Domain.Enums;
using Spendly.Tests.Budgets;
using Xunit;

namespace Spendly.Tests.Transactions;

public class CreateTransactionHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Transaction()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var accountId = builder.AddAccount("Monobank");
        var categoryId = builder.AddCategory("Groceries");

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var model = TestDataBuilder.BuildCreateTransactionModel(
            accountId,
            categoryId,
            450m,
            TransactionType.Expense,
            new DateTime(2026, 3, 10, 12, 0, 0, DateTimeKind.Utc),
            "Test comment");

        var handler = new CreateTransactionHandler(db);

        await handler.Handle(new CreateTransactionRequest(model), CancellationToken.None);

        db.Transactions.Should().HaveCount(1);

        var entity = db.Transactions.Single();
        entity.AccountId.Should().Be(accountId);
        entity.CategoryId.Should().Be(categoryId);
        entity.Amount.Should().Be(450m);
        entity.Type.Should().Be(TransactionType.Expense);
        entity.Comment.Should().Be("Test comment");
    }
}