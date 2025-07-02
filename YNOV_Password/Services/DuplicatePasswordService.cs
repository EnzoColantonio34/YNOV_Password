using System.Collections.Generic;
using System.Linq;
using YNOV_Password.Models;

namespace YNOV_Password.Services
{
    public class DuplicatePasswordService
    {
        public static void MarkDuplicatePasswords(List<PasswordEntry> passwords)
        {
            // Regrouper les mots de passe par valeur
            var passwordGroups = passwords
                .Where(p => !string.IsNullOrEmpty(p.Password))
                .GroupBy(p => p.Password)
                .ToList();

            // Marquer les mots de passe qui apparaissent plus d'une fois
            foreach (var group in passwordGroups)
            {
                var isDuplicate = group.Count() > 1;
                foreach (var password in group)
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
