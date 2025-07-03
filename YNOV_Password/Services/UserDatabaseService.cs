using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using YNOV_Password.Models;

namespace YNOV_Password.Services
{
    public class UserDatabaseService
    {
        private readonly string _connectionString;

        public UserDatabaseService()
        {
            _connectionString = DatabaseHelper.GetConnectionString();
            Initialize();
        }

        public void Initialize()
        {
            try
            {
                LoggingService.LogInfo("Initialisation de la base de données utilisateurs");
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                // Vérifier si la table existe
                var checkTableCommand = connection.CreateCommand();
                checkTableCommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Users';";
                var hasTable = checkTableCommand.ExecuteScalar() != null;

                if (!hasTable)
                {
                    LoggingService.LogInfo("Création de la table Users");
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        CREATE TABLE Users (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Username TEXT NOT NULL,
                            Email TEXT UNIQUE NOT NULL,
                            Password TEXT NOT NULL
                        );";
                    command.ExecuteNonQuery();
                }

                // Vérifier s'il y a des utilisateurs
                var countCommand = connection.CreateCommand();
                countCommand.CommandText = "SELECT COUNT(*) FROM Users;";
                var userCount = Convert.ToInt32(countCommand.ExecuteScalar());
                LoggingService.LogInfo($"Nombre d'utilisateurs existants : {userCount}");

                if (userCount == 0)
                {
                    LoggingService.LogInfo("Création de l'utilisateur par défaut");
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        INSERT INTO Users (Username, Email, Password) 
                        VALUES (@username, @email, @password);";
                    command.Parameters.AddWithValue("@username", "admin");
                    command.Parameters.AddWithValue("@email", "admin@example.com");
                    command.Parameters.AddWithValue("@password", HashPassword("admin123"));
                    command.ExecuteNonQuery();
                    LoggingService.LogInfo("Utilisateur par défaut créé avec succès");
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, "Erreur lors de l'initialisation de la base de données utilisateurs");
                throw;
            }
        }

        private void CreateDefaultUserIfNeeded()
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Users";
                var count = (long)command.ExecuteScalar()!;

                if (count == 0)
                {
                    Register("admin", "admin@example.com", "admin123");
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, "Création de l'utilisateur par défaut");
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public bool Register(string username, string email, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                    return false;

                if (GetUserByEmail(email) != null)
                    return false; // Email déjà utilisé

                var hashedPassword = HashPassword(password);

                using var connection = new SqliteConnection(_connectionString);
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Users (Username, Email, Password) VALUES (@username, @email, @password)";
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@password", hashedPassword);
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, $"Enregistrement de l'utilisateur '{username}'");
                return false;
            }
        }

        public User? Login(string email, string password)
        {
            try
            {
                LoggingService.LogInfo($"Tentative de login pour l'email : {email}");
                var hashedPassword = HashPassword(password);

                using var connection = new SqliteConnection(_connectionString);
                connection.Open();
                
                // Vérifier d'abord si la table Users existe
                var checkTableCommand = connection.CreateCommand();
                checkTableCommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Users';";
                var hasTable = checkTableCommand.ExecuteScalar() != null;
                
                if (!hasTable)
                {
                    LoggingService.LogError("La table Users n'existe pas !");
                    Initialize();
                    LoggingService.LogInfo("Table Users créée et initialisée");
                }

                // Vérifier si l'utilisateur existe
                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Users WHERE Email = @email;";
                command.Parameters.AddWithValue("@email", email);
                var userExists = Convert.ToInt32(command.ExecuteScalar()) > 0;
                LoggingService.LogInfo($"Utilisateur existe : {userExists}");

                if (userExists)
                {
                    // Vérifier le mot de passe
                    command = connection.CreateCommand();
                    command.CommandText = "SELECT Id, Username, Email, Password FROM Users WHERE Email = @email AND Password = @password;";
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@password", hashedPassword);

                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        var user = new User
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Email = reader.GetString(2),
                            Password = reader.GetString(3)
                        };
                        LoggingService.LogInfo($"Login réussi pour l'utilisateur {user.Username}");
                        return user;
                    }
                    LoggingService.LogInfo("Mot de passe incorrect");
                }
                return null;
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, $"Erreur lors de la connexion de l'utilisateur '{email}'");
                throw; // On laisse l'exception remonter pour avoir plus d'informations
            }
        }

        public User? GetUserByEmail(string email)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Username, Email, Password FROM Users WHERE Email = @email";
                command.Parameters.AddWithValue("@email", email);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new User
                    {
                        Id = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        Email = reader.GetString(2),
                        Password = reader.GetString(3)
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, $"Récupération de l'utilisateur par email '{email}'");
                return null;
            }
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Username, Email, Password FROM Users";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(new User
                    {
                        Id = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        Email = reader.GetString(2),
                        Password = reader.GetString(3)
                    });
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, "Récupération de tous les utilisateurs");
            }
            return users;
        }

        public User? GetUserById(int id)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Username, Email, Password FROM Users WHERE Id = @id";
                command.Parameters.AddWithValue("@id", id);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new User
                    {
                        Id = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        Email = reader.GetString(2),
                        Password = reader.GetString(3)
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, $"Récupération de l'utilisateur par id '{id}'");
                return null;
            }
        }
    }
}
