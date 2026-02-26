using System.Globalization;

namespace Spendly.Models;

public readonly record struct YearMonth(int Year, int Month)
{
    public DateTime Start => new(Year, Month, 1);
    public DateTime EndExclusive => Start.AddMonths(1);

    public override string ToString()
    {
        if (Year <= 0 || Month < 1 || Month > 12)
            return "—";

        var culture = CultureInfo.CurrentCulture;
        var name = culture.DateTimeFormat.GetMonthName(Month);
        name = culture.TextInfo.ToTitleCase(name);
        return $"{name} {Year}";
    }
}