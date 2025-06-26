using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using YNOV_Password.Models;

namespace YNOV_Password.Services
{
    public class PasswordDatabaseService
    {
        private readonly string _connectionString = "Data Source=passwords.db";
        private readonly EncryptionService _encryptionService;
        public int CurrentUserId { get; set; }

        public PasswordDatabaseService(int userId = 0, string masterPassword = "default_master_key")
        {
            CurrentUserId = userId;
            _encryptionService = new EncryptionService(masterPassword);
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
            
            // Créer la table Passwords si elle n'existe pas
            var command = connection.CreateCommand();
            command.CommandText =
                @"CREATE TABLE IF NOT EXISTS Passwords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Site TEXT,
                    Username TEXT,
                    Password TEXT,
                    Url TEXT
                );";
            command.ExecuteNonQuery();

            // Vérifier si la colonne UserId existe déjà
            command = connection.CreateCommand();
            command.CommandText = "PRAGMA table_info(Passwords)";
            using var reader = command.ExecuteReader();
            bool userIdExists = false;
            while (reader.Read())
            {
                if (reader.GetString(1) == "UserId")
                {
                    userIdExists = true;
                    break;
                }
            }
            reader.Close();

            // Si la colonne UserId n'existe pas, l'ajouter
            if (!userIdExists)
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] Migration: Ajout de la colonne UserId");
                
                // Ajouter la colonne UserId avec une valeur par défaut de 1
                command = connection.CreateCommand();
                command.CommandText = "ALTER TABLE Passwords ADD COLUMN UserId INTEGER NOT NULL DEFAULT 1";
                command.ExecuteNonQuery();
                
                System.Diagnostics.Debug.WriteLine("[DEBUG] Migration: Colonne UserId ajoutée avec succès");
            }
        }

        public List<PasswordEntry> GetAll()
        {
            var list = new List<PasswordEntry>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Site, Username, Password, Url FROM Passwords WHERE UserId = @userId";
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
                    Url = reader.GetString(4)
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
            command.CommandText = "INSERT INTO Passwords (UserId, Site, Username, Password, Url) VALUES (@userId, @site, @username, @password, @url)";
            command.Parameters.AddWithValue("@userId", CurrentUserId);
            command.Parameters.AddWithValue("@site", entry.Site);
            command.Parameters.AddWithValue("@username", entry.Username ?? "");
            
            // Chiffrer le mot de passe avant de l'enregistrer
            var encryptedPassword = _encryptionService.Encrypt(entry.Password ?? "");
            command.Parameters.AddWithValue("@password", encryptedPassword);
            
            command.Parameters.AddWithValue("@url", entry.Url ?? "");
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
                    
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Migration: Mot de passe chiffré pour l'ID {id}");
                }

                if (passwordsToMigrate.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Migration: {passwordsToMigrate.Count} mots de passe migrés vers le format chiffré");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Erreur lors de la migration des mots de passe: {ex.Message}");
            }
        }
    }
}
