using Spendly.Helpers;

namespace Spendly.Models;

public sealed record Category(Guid Id, string Name)
{
    public string IconSource => CategoryIconProvider.GetIcon(Name);
}