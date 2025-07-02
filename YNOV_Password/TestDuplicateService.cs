using System;
using System.Collections.Generic;
using YNOV_Password.Models;
using YNOV_Password.Services;

namespace YNOV_Password
{
    public static class TestDuplicateService
    {
        public static void RunTest()
        {
            Console.WriteLine("=== Test de détection de doublons ===");
            
            // Créer des données de test avec des doublons
            var testPasswords = new List<PasswordEntry>
            {
                new PasswordEntry { Id = 1, Site = "Site1", Password = "motdepasse123", IsDuplicate = false },
                new PasswordEntry { Id = 2, Site = "Site2", Password = "motdepasse123", IsDuplicate = false }, // Doublon
                new PasswordEntry { Id = 3, Site = "Site3", Password = "unique456", IsDuplicate = false },
                new PasswordEntry { Id = 4, Site = "Site4", Password = "motdepasse123", IsDuplicate = false }, // Doublon
                new PasswordEntry { Id = 5, Site = "Site5", Password = "autre789", IsDuplicate = false }
            };

            Console.WriteLine("Avant la détection :");
            foreach (var p in testPasswords)
            {
                Console.WriteLine($"- {p.Site}: {p.Password} => IsDuplicate: {p.IsDuplicate}");
            }

            // Appliquer la détection
            DuplicatePasswordService.MarkDuplicatePasswords(testPasswords);

            Console.WriteLine("\nAprès la détection :");
            foreach (var p in testPasswords)
            {
                Console.WriteLine($"- {p.Site}: {p.Password} => IsDuplicate: {p.IsDuplicate}");
            }

            Console.WriteLine("=== Fin du test ===");
        }
    }
}
