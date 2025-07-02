using System.Collections.Generic;
using YNOV_Password.Models;
using YNOV_Password.Services;

namespace YNOV_Password.Tests
{
    /// <summary>
    /// Classe de test simple pour vérifier le fonctionnement du service de détection de doublons
    /// </summary>
    public static class DuplicatePasswordServiceTest
    {
        public static void RunTests()
        {
            System.Console.WriteLine("=== Test du service de détection de doublons ===");
            
            // Créer des données de test
            var passwords = new List<PasswordEntry>
            {
                new PasswordEntry { Id = 1, Site = "site1.com", Password = "password123" },
                new PasswordEntry { Id = 2, Site = "site2.com", Password = "password123" }, // Doublon
                new PasswordEntry { Id = 3, Site = "site3.com", Password = "unique456" },
                new PasswordEntry { Id = 4, Site = "site4.com", Password = "password123" }, // Doublon
                new PasswordEntry { Id = 5, Site = "site5.com", Password = "another789" }
            };

            // Tester la détection de doublons
            DuplicatePasswordService.MarkDuplicatePasswords(passwords);

            // Vérifier les résultats
            System.Console.WriteLine("Résultats de la détection de doublons :");
            foreach (var password in passwords)
            {
                System.Console.WriteLine($"- {password.Site}: {password.Password} => Doublon: {password.IsDuplicate}");
            }

            // Tester le comptage des doublons
            var duplicateCount = DuplicatePasswordService.GetDuplicateCount(passwords);
            System.Console.WriteLine($"Nombre de groupes de mots de passe dupliqués : {duplicateCount}");

            // Tester la récupération des doublons pour un mot de passe spécifique
            var duplicatesForPassword123 = DuplicatePasswordService.GetDuplicatesForPassword(passwords, "password123");
            System.Console.WriteLine($"Nombre d'entrées utilisant 'password123' : {duplicatesForPassword123.Count}");
            
            System.Console.WriteLine("=== Fin des tests ===");
        }
    }
}
