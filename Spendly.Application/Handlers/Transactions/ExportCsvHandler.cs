using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spendly.Application.Handlers.Transactions.Requests;
using Spendly.Data.Entities;
using Spendly.Domain.Enums;
using Spendly.Domain.Models;
using Spendly.Infrastructure;

namespace Spendly.Application.Handlers.Transactions;

public class ExportCsvHandler(SpendlyDbContext dbContext) : IRequestHandler<ExportCsvRequest, string>
{
    public async Task<string> Handle(ExportCsvRequest request, CancellationToken cancellationToken)
    {
        var query = dbContext.Transactions
            .AsNoTracking()
            .Include(x => x.Account)
            .Include(x => x.Category)
            .AsQueryable();

        if (request.Type is not null)
            query = query.Where(x => x.Type == request.Type);

        if (request.Date is not null)
        {
            var from = request.Date.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
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

        var transactions = await query.ToListAsync(cancellationToken);

        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Spendly");

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        var filePath = Path.Combine(path, request.FileName);

        var records = transactions.Select(ToRecord);

        await using var streamWriter = new StreamWriter(filePath);
        await using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

        await csvWriter.WriteRecordsAsync(records, cancellationToken);

        return filePath;
    }

    private static TransactionRecord ToRecord(TransactionEntity transaction)
    {
        return new TransactionRecord(
            transaction.DateUtc,
            transaction.Amount,
            transaction.Comment,
            transaction.Category?.Name ?? "Other",
            transaction.Account.Name,
            transaction.Type
        );
    }

    private sealed record TransactionRecord(
        [property: Name("transaction_date")] DateTime DateUtc,
        [property: Name("amount")] decimal Amount,
        [property: Name("comment")] string Comment,
        [property: Name("category")] string Category,
        [property: Name("account")] string Account,
        [property: Name("type")] TransactionType Type);
}