using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace UniversalOfficeDataManager.Converters
{
    /// <summary>
    /// Visible when the bound integer is greater than zero. Pass
    /// ConverterParameter="Invert" to flip that (useful for "no items yet"
    /// empty-state messages).
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var count = value is int i ? i : 0;
            var invert = string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase);
            var visible = invert ? count == 0 : count > 0;
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
