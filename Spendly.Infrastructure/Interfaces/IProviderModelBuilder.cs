using Microsoft.EntityFrameworkCore;

namespace Spendly.Infrastructure.Interfaces;

public interface IProviderModelBuilder
{
	public void Configure(ModelBuilder modelBuilder);
}