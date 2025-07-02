using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace YNOV_Password.Services
{
    public class WordDatabaseService
    {
        private readonly string _connectionString = "Data Source=passwords.db";
        public int CurrentUserId { get; set; }

        public WordDatabaseService(int userId = 0)
        {
            CurrentUserId = userId;
            Initialize();
        }

        public void Initialize()
        {
            LoggingService.ExecuteWithLogging(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();
                
                // Créer la table WordLibrary si elle n'existe pas
                var command = connection.CreateCommand();
                command.CommandText =
                    @"CREATE TABLE IF NOT EXISTS WordLibrary (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER NOT NULL,
                        Word TEXT NOT NULL,
                        LibraryName TEXT,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        UNIQUE(UserId, Word)
                    );";
                command.ExecuteNonQuery();

                // Créer un index pour améliorer les performances
                command = connection.CreateCommand();
                command.CommandText = "CREATE INDEX IF NOT EXISTS idx_wordlibrary_userid ON WordLibrary(UserId);";
                command.ExecuteNonQuery();
            }, $"Initialisation de la base de données pour l'utilisateur {CurrentUserId}");
        }

        public void SaveWords(List<string> words, string libraryName = "Default")
        {
            if (words == null || !words.Any())
                return;

            LoggingService.ExecuteWithLogging(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                // Commencer une transaction pour de meilleures performances
                using var transaction = connection.BeginTransaction();
                
                try
                {
                    // Supprimer les anciens mots de cette bibliothèque pour cet utilisateur
                    var deleteCommand = connection.CreateCommand();
                    deleteCommand.CommandText = "DELETE FROM WordLibrary WHERE UserId = @userId AND LibraryName = @libraryName";
                    deleteCommand.Parameters.AddWithValue("@userId", CurrentUserId);
                    deleteCommand.Parameters.AddWithValue("@libraryName", libraryName);
                    deleteCommand.ExecuteNonQuery();

                    // Insérer les nouveaux mots
                    var insertCommand = connection.CreateCommand();
                    insertCommand.CommandText = "INSERT OR IGNORE INTO WordLibrary (UserId, Word, LibraryName) VALUES (@userId, @word, @libraryName)";
                    
                    foreach (var word in words.Where(w => !string.IsNullOrWhiteSpace(w)))
                    {
                        insertCommand.Parameters.Clear();
                        insertCommand.Parameters.AddWithValue("@userId", CurrentUserId);
                        insertCommand.Parameters.AddWithValue("@word", word.Trim());
                        insertCommand.Parameters.AddWithValue("@libraryName", libraryName);
                        insertCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    LoggingService.LogInfo($"Sauvegarde réussie de {words.Count} mots dans la bibliothèque '{libraryName}' pour l'utilisateur {CurrentUserId}");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    LoggingService.LogError(ex, $"Erreur lors de la sauvegarde des mots dans la bibliothèque '{libraryName}'");
                    throw;
                }
            }, $"Sauvegarde des mots dans la bibliothèque '{libraryName}'");
        }

        public List<string> GetWords(string libraryName = "Default")
        {
            return LoggingService.ExecuteWithLogging(() =>
            {
                var words = new List<string>();
                
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();
                
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Word FROM WordLibrary WHERE UserId = @userId AND LibraryName = @libraryName ORDER BY Word";
                command.Parameters.AddWithValue("@userId", CurrentUserId);
                command.Parameters.AddWithValue("@libraryName", libraryName);
                
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    words.Add(reader.GetString(0));
                }
                
                return words;
            }, $"Récupération des mots de la bibliothèque '{libraryName}'", new List<string>());
        }

        public List<string> GetAllWords()
        {
            var words = new List<string>();
            
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            var command = connection.CreateCommand();
            command.CommandText = "SELECT DISTINCT Word FROM WordLibrary WHERE UserId = @userId ORDER BY Word";
            command.Parameters.AddWithValue("@userId", CurrentUserId);
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                words.Add(reader.GetString(0));
            }
            
            return words;
        }

        public List<string> GetLibraryNames()
        {
            var libraryNames = new List<string>();
            
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            var command = connection.CreateCommand();
            command.CommandText = "SELECT DISTINCT LibraryName FROM WordLibrary WHERE UserId = @userId ORDER BY LibraryName";
            command.Parameters.AddWithValue("@userId", CurrentUserId);
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                libraryNames.Add(reader.GetString(0));
            }
            
            return libraryNames;
        }

        public void DeleteLibrary(string libraryName)
        {
            LoggingService.ExecuteWithLogging(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();
                
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM WordLibrary WHERE UserId = @userId AND LibraryName = @libraryName";
                command.Parameters.AddWithValue("@userId", CurrentUserId);
                command.Parameters.AddWithValue("@libraryName", libraryName);
                var deletedRows = command.ExecuteNonQuery();
                
                LoggingService.LogInfo($"Bibliothèque '{libraryName}' supprimée ({deletedRows} mots supprimés) pour l'utilisateur {CurrentUserId}");
            }, $"Suppression de la bibliothèque '{libraryName}'");
        }

        public int GetWordCount(string libraryName = "Default")
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM WordLibrary WHERE UserId = @userId AND LibraryName = @libraryName";
            command.Parameters.AddWithValue("@userId", CurrentUserId);
            command.Parameters.AddWithValue("@libraryName", libraryName);
            
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public bool HasWords(string libraryName = "Default")
        {
            return GetWordCount(libraryName) > 0;
        }
    }
}
