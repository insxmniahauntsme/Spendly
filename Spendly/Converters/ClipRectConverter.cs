using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Spendly.Converters;

public sealed class ClipRectConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values.Length < 3 ||
		    values[0] == DependencyProperty.UnsetValue ||
		    values[1] == DependencyProperty.UnsetValue ||
		    values[2] == DependencyProperty.UnsetValue ||
		    values[0] is not double w || values[1] is not double h ||
		    w <= 0 || h <= 0 ||
		    values[2] is not CornerRadius cr)
			return Geometry.Empty;

		var r = Math.Max(0, cr.TopLeft);

		return new RectangleGeometry(new Rect(0, 0, w, h), r, r);
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}
