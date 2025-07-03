using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using YNOV_Password.Models;
using System.IO;

namespace YNOV_Password.Services
{
    public class PasswordDatabaseService
    {
        private readonly string _connectionString;
        private readonly EncryptionService _encryptionService;
        public int CurrentUserId { get; set; }

        public PasswordDatabaseService(int userId = 0, string masterPassword = "default_master_key")
        {
            CurrentUserId = userId;
            _encryptionService = new EncryptionService(masterPassword);
            _connectionString = DatabaseHelper.GetConnectionString();
            Initialize();
            
            // Migrer les mots de passe existants vers le format chiffré
            if (userId > 0)
            {
                MigratePasswordsToEncrypted();
            }
        }

        public PasswordDatabaseService(User user) : this(user.Id, user.Email + user.Password)
        {
            // Utilise l'email + password comme clé maître
        }

        public void Initialize()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Créer la table Passwords uniquement si elle n'existe pas déjà
            var createTableCmd = connection.CreateCommand();
            createTableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Passwords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Site TEXT,
                    Username TEXT,
                    Password TEXT,
                    Url TEXT,
                    UserId INTEGER NOT NULL DEFAULT 0,
                    FolderId INTEGER NOT NULL DEFAULT 1,
                    FOREIGN KEY(FolderId) REFERENCES Folders(Id)
                );";
            createTableCmd.ExecuteNonQuery();

            // Créer un index sur la colonne FolderId pour améliorer les performances
            var createIndexCmd = connection.CreateCommand();
            createIndexCmd.CommandText = "CREATE INDEX IF NOT EXISTS idx_passwords_folderid ON Passwords(FolderId);";
            createIndexCmd.ExecuteNonQuery();

            // Vérifier si le dossier Général existe
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id FROM Folders WHERE Id = 1;";
            var result = command.ExecuteScalar();

            if (result == null)
            {
                // Créer le dossier Général s'il n'existe pas
                command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Folders (Id, Name, Description, UserId)
                    VALUES (1, 'Général', 'Dossier par défaut', @userId);";
                command.Parameters.AddWithValue("@userId", CurrentUserId);
                command.ExecuteNonQuery();
            }

            // Mettre à jour tous les mots de passe sans dossier
            command = connection.CreateCommand();
            command.CommandText = "UPDATE Passwords SET FolderId = 1 WHERE FolderId IS NULL OR FolderId = 0;";
            command.ExecuteNonQuery();
        }

        public List<PasswordEntry> GetAll()
        {
            var list = new List<PasswordEntry>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Site, Username, Password, Url, FolderId FROM Passwords WHERE UserId = @userId";
            command.Parameters.AddWithValue("@userId", CurrentUserId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var encryptedPassword = reader.GetString(3);
                var decryptedPassword = _encryptionService.Decrypt(encryptedPassword);
                
                list.Add(new PasswordEntry
                {
                    Id = reader.GetInt32(0),
                    Site = reader.GetString(1),
                    Username = reader.GetString(2),
                    Password = decryptedPassword,
                    Url = reader.GetString(4),
                    FolderId = reader.IsDBNull(5) ? null : reader.GetInt32(5)
                });
            }
            return list;
        }

        public List<PasswordEntry> Search(string searchTerm)
        {
            var list = new List<PasswordEntry>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Site, Username, Password, Url FROM Passwords WHERE UserId = @userId AND (Site LIKE @search OR Username LIKE @search)";
            command.Parameters.AddWithValue("@userId", CurrentUserId);
            command.Parameters.AddWithValue("@search", $"%{searchTerm}%");

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var encryptedPassword = reader.GetString(3);
                var decryptedPassword = _encryptionService.Decrypt(encryptedPassword);
                
                list.Add(new PasswordEntry
                {
                    Id = reader.GetInt32(0),
                    Site = reader.GetString(1),
                    Username = reader.GetString(2),
                    Password = decryptedPassword,
                    Url = reader.GetString(4)
                });
            }
            return list;
        }

        private void ValidateEntry(PasswordEntry entry)
        {
            if (string.IsNullOrWhiteSpace(entry.Site))
                throw new ArgumentException("Le site ne peut pas être vide");
            if (string.IsNullOrWhiteSpace(entry.Password))
                throw new ArgumentException("Le mot de passe ne peut pas être vide");
        }

        public void Add(PasswordEntry entry)
        {
            ValidateEntry(entry);

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Passwords (UserId, Site, Username, Password, Url, FolderId) VALUES (@userId, @site, @username, @password, @url, @folderId)";
            command.Parameters.AddWithValue("@userId", CurrentUserId);
            command.Parameters.AddWithValue("@site", entry.Site);
            command.Parameters.AddWithValue("@username", entry.Username ?? "");
            
            // Chiffrer le mot de passe avant de l'enregistrer
            var encryptedPassword = _encryptionService.Encrypt(entry.Password ?? "");
            command.Parameters.AddWithValue("@password", encryptedPassword);
            
            command.Parameters.AddWithValue("@url", entry.Url ?? "");
            command.Parameters.AddWithValue("@folderId", entry.FolderId.HasValue ? (object)entry.FolderId.Value : DBNull.Value);
            command.ExecuteNonQuery();
        }

        public void Update(PasswordEntry entry)
        {
            ValidateEntry(entry);

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "UPDATE Passwords SET Site = @site, Username = @username, Password = @password, Url = @url, FolderId = @folderId WHERE Id = @id AND UserId = @userId";
            command.Parameters.AddWithValue("@id", entry.Id);
            command.Parameters.AddWithValue("@userId", CurrentUserId);
            command.Parameters.AddWithValue("@site", entry.Site);
            command.Parameters.AddWithValue("@username", entry.Username ?? "");
            
            // Chiffrer le mot de passe avant de l'enregistrer
            var encryptedPassword = _encryptionService.Encrypt(entry.Password ?? "");
            command.Parameters.AddWithValue("@password", encryptedPassword);
            
            command.Parameters.AddWithValue("@url", entry.Url ?? "");
            command.Parameters.AddWithValue("@folderId", entry.FolderId.HasValue ? (object)entry.FolderId.Value : DBNull.Value);
            command.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Passwords WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Migre les mots de passe existants en texte clair vers le format chiffré
        /// </summary>
        public void MigratePasswordsToEncrypted()
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                // Récupérer tous les mots de passe pour cet utilisateur
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Password FROM Passwords WHERE UserId = @userId";
                command.Parameters.AddWithValue("@userId", CurrentUserId);

                var passwordsToMigrate = new List<(int Id, string Password)>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var password = reader.GetString(1);
                    
                    // Vérifier si le mot de passe semble déjà chiffré
                    if (!_encryptionService.IsEncrypted(password))
                    {
                        passwordsToMigrate.Add((id, password));
                    }
                }
                reader.Close();

                // Mettre à jour chaque mot de passe non chiffré
                foreach (var (id, plainPassword) in passwordsToMigrate)
                {
                    var encryptedPassword = _encryptionService.Encrypt(plainPassword);
                    
                    var updateCommand = connection.CreateCommand();
                    updateCommand.CommandText = "UPDATE Passwords SET Password = @password WHERE Id = @id";
                    updateCommand.Parameters.AddWithValue("@password", encryptedPassword);
                    updateCommand.Parameters.AddWithValue("@id", id);
                    updateCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, "Migration des mots de passe non chiffrés");
            }
        }
    }
}
