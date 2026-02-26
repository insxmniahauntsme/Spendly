using Riok.Mapperly.Abstractions;
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
                Id = entity.AccountId,
                Balance = entity.Account.Balance,
                Name = entity.Account.Name,
                Type = entity.Account.Type,
                Transactions = entity.Account.Transactions.Select(ToModel).ToList()
            },
        };

    public static IReadOnlyList<Transaction> ProjectToModels(this IReadOnlyList<TransactionEntity> transactions)
        => transactions.Select(ToModel).ToList();
}