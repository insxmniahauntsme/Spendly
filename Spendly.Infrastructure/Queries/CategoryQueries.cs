using Microsoft.EntityFrameworkCore;
using Spendly.Data.Entities;

namespace Spendly.Infrastructure.Queries;

public sealed class CategoryQueries(SpendlyDbContext dbContext)
{
    public async Task<List<CategoryEntity>> GetCategoriesAsync()
    {
        return await dbContext.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}