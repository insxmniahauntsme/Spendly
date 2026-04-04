using FluentAssertions;
using Spendly.Application.Handlers.Transactions;
using Spendly.Application.Handlers.Transactions.Requests;
using Spendly.Domain.Enums;
using Spendly.Tests.Budgets;
using Xunit;

namespace Spendly.Tests.Transactions;

public class DeleteTransactionHandlerTests
{
    [Fact]
    public async Task Handle_Should_Delete_Existing_Transaction()
    {
        await using var db = TestDbContextFactory.Create();
        var builder = new TestDataBuilder(db);

        var accountId = builder.AddAccount();
        var categoryId = builder.AddCategory("Groceries");

        var transactionId = builder.AddTransaction(
            accountId,
            categoryId,
            300m,
            TransactionType.Expense,
            new DateTime(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc));

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new DeleteTransactionHandler(db);

        await handler.Handle(new DeleteTransactionRequest(transactionId), CancellationToken.None);

        db.Transactions.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Do_Nothing_When_Transaction_Does_Not_Exist()
    {
        await using var db = TestDbContextFactory.Create();

        var handler = new DeleteTransactionHandler(db);

        var act = async () =>
            await handler.Handle(new DeleteTransactionRequest(Guid.NewGuid()), CancellationToken.None);

        await act.Should().NotThrowAsync();
        db.Transactions.Should().BeEmpty();
    }
}