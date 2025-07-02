using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace YNOV_Password.Converters
{
    public class DuplicatePasswordConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isDuplicate && isDuplicate)
            {
                return "⚠️"; // Icône de danger
            }
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
