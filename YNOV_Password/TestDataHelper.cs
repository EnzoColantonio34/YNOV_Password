using YNOV_Password.Models;
using YNOV_Password.Services;

namespace YNOV_Password
{
    public static class TestDataHelper
    {
        public static void CreateTestDuplicatePasswords(int userId)
        {
            var dbService = new PasswordDatabaseService(userId);
            
            // Créer des mots de passe de test avec des doublons
            var testPasswords = new[]
            {
                new PasswordEntry { Site = "Gmail", Username = "user1@gmail.com", Password = "motdepasse123", Url = "https://gmail.com" },
                new PasswordEntry { Site = "Facebook", Username = "user1", Password = "motdepasse123", Url = "https://facebook.com" }, // Doublon
                new PasswordEntry { Site = "Twitter", Username = "user1", Password = "unique456", Url = "https://twitter.com" },
                new PasswordEntry { Site = "Instagram", Username = "user1", Password = "motdepasse123", Url = "https://instagram.com" }, // Doublon
                new PasswordEntry { Site = "LinkedIn", Username = "user1", Password = "autre789", Url = "https://linkedin.com" }
            };

            try
            {
                foreach (var password in testPasswords)
                {
                    dbService.Add(password);
                    System.Diagnostics.Debug.WriteLine($"Ajouté: {password.Site} avec mot de passe: {password.Password}");
                }
                System.Diagnostics.Debug.WriteLine("Données de test créées avec succès");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la création des données de test: {ex.Message}");
            }
        }
    }
}
