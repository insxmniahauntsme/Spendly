using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Spendly.Converters;

public class ProgressToBrushConverter : IValueConverter
{
	public Brush SuccessBrush { get; set; } = null!;
	public Brush WarningBrush { get; set; } = null!;
	public Brush DangerBrush { get; set; } = null!;

	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is not double progress || progress < 0.75)
			return SuccessBrush;

		return progress < 0.9 ? WarningBrush : DangerBrush;
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotImplementedException();
}