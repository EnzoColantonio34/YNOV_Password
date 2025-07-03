using System.Collections.Generic;
using System.Linq;
using YNOV_Password.Models;

namespace YNOV_Password.Services
{
    public class DuplicatePasswordService
    {
        public static void MarkDuplicatePasswords(List<PasswordEntry> passwords, PasswordDatabaseService? passwordService = null)
        {
            var allPasswords = passwords;
            
            // Si un service est fourni, utiliser tous les mots de passe de l'utilisateur
            if (passwordService != null)
            {
                allPasswords = passwordService.GetAll();
            }

            // Regrouper les mots de passe par valeur
            var passwordGroups = allPasswords
                .Where(p => !string.IsNullOrEmpty(p.Password))
                .GroupBy(p => p.Password)
                .ToList();

            // Marquer les mots de passe qui apparaissent plus d'une fois
            foreach (var group in passwordGroups)
            {
                var isDuplicate = group.Count() > 1;
                
                // Pour les mots de passe de la liste d'origine uniquement
                foreach (var password in passwords.Where(p => p.Password == group.Key))
                {
                    password.IsDuplicate = isDuplicate;
                }
            }
        }

        public static List<PasswordEntry> GetDuplicatesForPassword(List<PasswordEntry> allPasswords, string password)
        {
            return allPasswords
                .Where(p => p.Password == password && !string.IsNullOrEmpty(password))
                .ToList();
        }

        public static int GetDuplicateCount(List<PasswordEntry> passwords)
        {
            return passwords
                .Where(p => !string.IsNullOrEmpty(p.Password))
                .GroupBy(p => p.Password)
                .Count(g => g.Count() > 1);
        }
    }
}
