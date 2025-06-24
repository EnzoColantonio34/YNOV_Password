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
            System.Diagnostics.Debug.WriteLine($"[DEBUG] PasswordVisibilityConverter appelé avec {values.Count} valeurs");
            
            if (values.Count >= 2)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] values[0]: {values[0]} (type: {values[0]?.GetType()})");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] values[1]: {values[1]} (type: {values[1]?.GetType()})");
                
                if (values[0] is string password && values[1] is bool isVisible)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Password: '{password}', IsVisible: {isVisible}");
                    
                    if (!string.IsNullOrEmpty(password))
                    {
                        if (isVisible)
                        {
                            System.Diagnostics.Debug.WriteLine($"[DEBUG] Retourne le mot de passe en clair: {password}");
                            return password;
                        }
                        else
                        {
                            var masked = new string('•', password.Length);
                            System.Diagnostics.Debug.WriteLine($"[DEBUG] Retourne le mot de passe masqué: {masked}");
                            return masked;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[DEBUG] Password est vide ou null");
                        return "••••••••";  // Fallback pour debug
                    }
                }
            }
            
            System.Diagnostics.Debug.WriteLine("[DEBUG] Retourne fallback");
            return "••••••••";  // Fallback par défaut pour debug
        }
    }
}
