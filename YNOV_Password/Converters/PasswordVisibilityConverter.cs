using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace YNOV_Password.Converters
{
    public class PasswordVisibilityConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count >= 2)
            {
                if (values[0] is string password && values[1] is bool isVisible)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        if (isVisible)
                        {
                            return password;
                        }
                        else
                        {
                            return new string('•', password.Length);
                        }
                    }
                    else
                    {
                        return "••••••••";
                    }
                }
            }
            
            return "••••••••";
        }
    }
}
