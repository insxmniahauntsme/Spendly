using MediatR;
using Microsoft.EntityFrameworkCore;
using Spendly.Application.Handlers.Transactions.Requests;
using Spendly.Application.Mappers;
using Spendly.Application.Models;
using Spendly.Data.Enums;
using Spendly.Domain.Enums;
using Spendly.Domain.Models;
using Spendly.Infrastructure;

namespace Spendly.Application.Handlers.Transactions;

public sealed class GetTransactionsHandler(SpendlyDbContext dbContext)
    : IRequestHandler<GetTransactionsRequest, TransactionsPageData>
{
    public async Task<TransactionsPageData> Handle(GetTransactionsRequest request, CancellationToken ct)
    {
        var query = dbContext.Transactions
            .AsNoTracking()
            .Include(x => x.Account)
            .Include(x => x.Category)
            .AsQueryable();

        if (request.Type is not null)
            query = query.Where(x => x.Type == request.Type);

        if (request.Month is not null)
        {
            var from = request.Month.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var to = from.AddMonths(1);

            query = query.Where(x => x.DateUtc >= from && x.DateUtc < to);
        }

        if (request.CategoryId is not null)
            query = query.Where(x => x.CategoryId == request.CategoryId);

        if (request.AccountId is not null)
            query = query.Where(x => x.AccountId == request.AccountId);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim();
            query = query.Where(x =>
                EF.Functions.Like(x.Comment, $"%{searchTerm}%") ||
                EF.Functions.Like(x.Account.Name, $"%{searchTerm}%") ||
                (x.Category != null && EF.Functions.Like(x.Category.Name, $"%{searchTerm}%")));
        }

        var totalCount = await query.CountAsync(ct);
        var totalExpenses = await query.Where(x => x.Type == TransactionType.Expense).SumAsync(x => x.Amount, ct);
        var totalIncomes = await query.Where(x => x.Type == TransactionType.Income).SumAsync(x => x.Amount, ct);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var entities = await query
            .OrderByDescending(x => x.DateUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .OrderByDescending(x => x.DateUtc)
            .ToListAsync(ct);

        var items = entities.ProjectToModels();

        var pageData = new PagedResponse<Transaction>(items, totalCount, page, pageSize);

        return new TransactionsPageData(pageData, totalExpenses, totalIncomes);
    }
}