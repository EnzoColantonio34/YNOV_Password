using Avalonia.Data.Converters;
using YNOV_Password.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace YNOV_Password.Converters
{
    public class FolderEmptyMessageConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count != 3)
            {
                return "Aucun mot de passe";
            }

            bool hasNoPasswords;
            string searchText;
            PasswordFolder? selectedFolder;

            try
            {
                hasNoPasswords = values[0] is bool b ? b : false;
                searchText = values[1]?.ToString() ?? string.Empty;
                selectedFolder = values[2] as PasswordFolder;
            }
            catch
            {
                return "Aucun mot de passe";
            }

            var isSearchActive = !string.IsNullOrWhiteSpace(searchText);

            if (selectedFolder != null)
            {
                if (isSearchActive)
                {
                    return $"Aucun résultat trouvé pour \"{searchText}\" dans le dossier \"{selectedFolder.Name}\"";
                }
                return $"Le dossier \"{selectedFolder.Name}\" est vide";
            }
            
            if (isSearchActive)
            {
                return $"Aucun résultat trouvé pour \"{searchText}\"";
            }
            return "Pas encore de mot de passe créé !";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
