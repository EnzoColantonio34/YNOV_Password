using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using YNOV_Password.Models;

namespace YNOV_Password.Services
{
    public class PasswordDatabaseService
    {
        private readonly string _connectionString = "Data Source=passwords.db";

        public PasswordDatabaseService()
        {
            Initialize();
        }

        public void Initialize()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
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
        }

        public List<PasswordEntry> GetAll()
        {
            var list = new List<PasswordEntry>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Site, Username, Password, Url FROM Passwords";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new PasswordEntry
                {
                    Site = reader.GetString(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    Url = reader.GetString(3)
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
            command.CommandText = "SELECT Site, Username, Password, Url FROM Passwords WHERE Site LIKE @search OR Username LIKE @search";
            command.Parameters.AddWithValue("@search", $"%{searchTerm}%");

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new PasswordEntry
                {
                    Site = reader.GetString(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    Url = reader.GetString(3)
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
            command.CommandText = "INSERT INTO Passwords (Site, Username, Password, Url) VALUES (@site, @username, @password, @url)";
            command.Parameters.AddWithValue("@site", entry.Site);
            command.Parameters.AddWithValue("@username", entry.Username ?? "");
            command.Parameters.AddWithValue("@password", entry.Password);
            command.Parameters.AddWithValue("@url", entry.Url ?? "");
            command.ExecuteNonQuery();
        }
    }
}
