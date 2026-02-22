namespace Spendly.Infrastructure.Interfaces;

public interface IDbInitializer
{
	Task InitializeAsync(CancellationToken ct = default);
}