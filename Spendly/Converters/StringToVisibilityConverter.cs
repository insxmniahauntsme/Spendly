using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Spendly.Converters;

public sealed class StringToVisibilityConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		var invert = parameter is string s &&
		             (s.Equals("invert", StringComparison.OrdinalIgnoreCase) ||
		              s.Equals("true", StringComparison.OrdinalIgnoreCase));

		var hasText = value is string str && !string.IsNullOrWhiteSpace(str);

		if (invert) hasText = !hasText;

		return hasText ? Visibility.Visible : Visibility.Collapsed;
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}