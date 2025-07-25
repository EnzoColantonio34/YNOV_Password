using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace YNOV_Password.Converters
{
    public class PasswordMaskConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string password && !string.IsNullOrEmpty(password))
            {
                return new string('•', password.Length);
            }
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
