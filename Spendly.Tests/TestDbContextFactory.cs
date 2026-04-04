using Microsoft.EntityFrameworkCore;
using Spendly.Infrastructure;

namespace Spendly.Tests;

public static class TestDbContextFactory
{
    public static SpendlyDbContext Create()
    {
        var options = new DbContextOptionsBuilder<SpendlyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new SpendlyDbContext(options, null);
    }
}