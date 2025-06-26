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
        private readonly string _connectionString = "Data Source=passwords.db";

        public UserDatabaseService()
        {
            Initialize();
        }

        public void Initialize()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText =
                @"CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL,
                    Email TEXT UNIQUE NOT NULL,
                    Password TEXT NOT NULL
                );";
            command.ExecuteNonQuery();

            // Créer un utilisateur par défaut si aucun utilisateur n'existe
            // Ceci est pour gérer les données existantes avant l'ajout du système de login
            CreateDefaultUserIfNeeded();
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
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la création de l'utilisateur par défaut: {ex.Message}");
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
            catch
            {
                return false;
            }
        }

        public User? Login(string email, string password)
        {
            try
            {
                var hashedPassword = HashPassword(password);

                using var connection = new SqliteConnection(_connectionString);
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Username, Email, Password FROM Users WHERE Email = @email AND Password = @password";
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@password", hashedPassword);

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
            catch
            {
                return null;
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
            catch
            {
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
            catch
            {
                // Log error if needed
            }
            return users;
        }
    }
}
