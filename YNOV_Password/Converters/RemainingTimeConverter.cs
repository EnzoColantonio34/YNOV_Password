using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace YNOV_Password.Converters
{
    public class RemainingTimeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int remainingTime && remainingTime > 0)
            {
                return $"{remainingTime}s";
            }
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
