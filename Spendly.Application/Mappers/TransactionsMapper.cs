using Spendly.Application.Models.Transactions;
using Spendly.Data.Entities;
using Spendly.Domain.Models;

namespace Spendly.Application.Mappers;

public static class TransactionsMapper
{
    public static TransactionEntity ToEntity(this Transaction model)
        => new()
        {
            Id = model.Id,
            AccountId = model.AccountId,
            CategoryId = model.CategoryId,
            Amount = model.Amount,
            DateUtc = model.DateUtc,
            Type = model.Type,
            Comment = model.Comment
        };

    public static Transaction ToModel(this TransactionEntity entity)
        => new()
        {
            Id = entity.Id,
            AccountId = entity.AccountId,
            CategoryId = entity.CategoryId,
            Amount = entity.Amount,
            DateUtc = entity.DateUtc,
            Type = entity.Type,
            Comment = entity.Comment,

            Account = new Account
            {
                Id = entity.Account.Id,
                Balance = entity.Account.Balance,
                Name = entity.Account.Name,
                Type = entity.Account.Type
            },

            Category = entity.Category is null
                ? null
                : new Category
                {
                    Id = entity.Category.Id,
                    Name = entity.Category.Name
                }
        };

    public static List<Transaction> ProjectToModels(this IReadOnlyList<TransactionEntity> transactions)
        => transactions.Select(ToModel).ToList();

    public static void UpdateEntity(this TransactionEntity entity, UpdateTransactionModel model)
    {
        entity.Amount = model.Amount;
        entity.DateUtc = model.DateUtc;
        entity.Comment = model.Comment;
        entity.Type = model.Type;
    }

    public static TransactionEntity ToEntity(this CreateTransactionModel model)
        => new TransactionEntity
        {
            AccountId = model.AccountId,
            CategoryId = model.CategoryId,
            Amount = model.Amount,
            DateUtc = model.DateUtc,
            Type = model.Type,
            Comment = model.Comment
        };
}