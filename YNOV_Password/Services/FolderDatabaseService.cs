using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.IO;
using YNOV_Password.Models;

namespace YNOV_Password.Services
{
    public class FolderDatabaseService
    {
        private readonly string _connectionString;
        private readonly User _currentUser;

        public FolderDatabaseService(User user)
        {
            _currentUser = user;
            _connectionString = DatabaseHelper.GetConnectionString();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Vérifier si la table existe déjà
            var checkTableCmd = new SqliteCommand(
                "SELECT name FROM sqlite_master WHERE type='table' AND name='Passwords'", 
                connection);
            var hasPasswordsTable = checkTableCmd.ExecuteScalar() != null;

            checkTableCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Folders'";
            var hasFoldersTable = checkTableCmd.ExecuteScalar() != null;

            if (!hasFoldersTable || !hasPasswordsTable)
            {
                // Si l'une des tables n'existe pas, on les recrée toutes les deux
                var dropFoldersCmd = new SqliteCommand("DROP TABLE IF EXISTS Folders", connection);
                dropFoldersCmd.ExecuteNonQuery();

                var dropPasswordsCmd = new SqliteCommand("DROP TABLE IF EXISTS Passwords", connection);
                dropPasswordsCmd.ExecuteNonQuery();

                // Créer d'abord la table Folders
                var createFoldersCmd = new SqliteCommand(@"
                    CREATE TABLE Folders (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Description TEXT,
                        Color TEXT NOT NULL DEFAULT '#3B82F6',
                        Icon TEXT NOT NULL DEFAULT '📁',
                        UserId INTEGER NOT NULL,
                        CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                    )", connection);
                createFoldersCmd.ExecuteNonQuery();

                // Puis créer la table Passwords avec la référence à Folders
                var createPasswordsCmd = new SqliteCommand(@"
                    CREATE TABLE Passwords (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Site TEXT,
                        Username TEXT,
                        Password TEXT,
                        Url TEXT,
                        UserId INTEGER NOT NULL,
                        FolderId INTEGER NOT NULL DEFAULT 1,
                        FOREIGN KEY(FolderId) REFERENCES Folders(Id)
                    )", connection);
                createPasswordsCmd.ExecuteNonQuery();
            }

            // Créer le dossier par défaut si il n'existe pas
            CreateDefaultFolderIfNotExists(connection);
        }

        private void CreateDefaultFolderIfNotExists(SqliteConnection connection)
        {
            // Vérifier si un dossier avec l'ID 1 existe déjà
            var checkCmd = new SqliteCommand(
                "SELECT Id FROM Folders WHERE Id = 1", 
                connection);

            object? result = checkCmd.ExecuteScalar();
            if (result == null || result == DBNull.Value)
            {
                // Si le dossier avec ID 1 n'existe pas, on le crée avec l'ID 1 explicitement
                var insertCmd = new SqliteCommand(@"
                    INSERT INTO Folders (Id, Name, Description, Color, Icon, UserId)
                    VALUES (1, 'Général', 'Dossier par défaut', '#3B82F6', '📁', @userId);", connection);
                insertCmd.Parameters.AddWithValue("@userId", _currentUser.Id);
                insertCmd.ExecuteNonQuery();
                
                // Mettre à jour tous les mots de passe sans dossier vers le dossier Général (ID 1)
                var updateCmd = new SqliteCommand(@"
                    UPDATE Passwords 
                    SET FolderId = 1 
                    WHERE UserId = @userId AND (FolderId IS NULL OR FolderId = 0);", 
                    connection);
                updateCmd.Parameters.AddWithValue("@userId", _currentUser.Id);
                updateCmd.ExecuteNonQuery();
            }
            else
            {
                // Vérifier si le dossier existe pour l'utilisateur actuel
                checkCmd = new SqliteCommand(
                    "SELECT Id FROM Folders WHERE UserId = @userId AND Name = 'Général'", 
                    connection);
                checkCmd.Parameters.AddWithValue("@userId", _currentUser.Id);
                result = checkCmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    // Créer le dossier Général pour cet utilisateur
                    var insertCmd = new SqliteCommand(@"
                        INSERT INTO Folders (Name, Description, Color, Icon, UserId)
                        VALUES ('Général', 'Dossier par défaut', '#3B82F6', '📁', @userId);", connection);
                    insertCmd.Parameters.AddWithValue("@userId", _currentUser.Id);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }

        public List<PasswordFolder> GetAllFolders()
        {
            var folders = new List<PasswordFolder>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = new SqliteCommand(
                "SELECT Id, Name, Description, Color, Icon, UserId, CreatedAt FROM Folders WHERE UserId = @userId ORDER BY Name", 
                connection);
            command.Parameters.AddWithValue("@userId", _currentUser.Id);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var folder = new PasswordFolder
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Color = reader.GetString(3),
                    Icon = reader.GetString(4),
                    UserId = reader.GetInt32(5),
                    CreatedAt = reader.GetDateTime(6)
                };
                folders.Add(folder);
            }

            return folders;
        }

        public PasswordFolder? GetFolderById(int folderId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = new SqliteCommand(
                "SELECT Id, Name, Description, Color, Icon, UserId, CreatedAt FROM Folders WHERE Id = @id AND UserId = @userId", 
                connection);
            command.Parameters.AddWithValue("@id", folderId);
            command.Parameters.AddWithValue("@userId", _currentUser.Id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new PasswordFolder
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Color = reader.GetString(3),
                    Icon = reader.GetString(4),
                    UserId = reader.GetInt32(5),
                    CreatedAt = reader.GetDateTime(6)
                };
            }

            return null;
        }

        public int CreateFolder(PasswordFolder folder)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = new SqliteCommand(@"
                INSERT INTO Folders (Name, Description, Color, Icon, UserId) 
                VALUES (@name, @description, @color, @icon, @userId);
                SELECT last_insert_rowid();", connection);

            command.Parameters.AddWithValue("@name", folder.Name);
            command.Parameters.AddWithValue("@description", folder.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@color", folder.Color);
            command.Parameters.AddWithValue("@icon", folder.Icon);
            command.Parameters.AddWithValue("@userId", _currentUser.Id);

            var id = Convert.ToInt32(command.ExecuteScalar());
            return id;
        }

        public void UpdateFolder(PasswordFolder folder)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = new SqliteCommand(@"
                UPDATE Folders 
                SET Name = @name, Description = @description, Color = @color, Icon = @icon
                WHERE Id = @id AND UserId = @userId", connection);

            command.Parameters.AddWithValue("@name", folder.Name);
            command.Parameters.AddWithValue("@description", folder.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@color", folder.Color);
            command.Parameters.AddWithValue("@icon", folder.Icon);
            command.Parameters.AddWithValue("@id", folder.Id);
            command.Parameters.AddWithValue("@userId", _currentUser.Id);

            command.ExecuteNonQuery();
        }

        public bool DeleteFolder(int folderId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // D'abord, vérifier s'il y a des mots de passe dans ce dossier
            var checkCmd = new SqliteCommand(
                "SELECT COUNT(*) FROM Passwords WHERE FolderId = @folderId", 
                connection);
            checkCmd.Parameters.AddWithValue("@folderId", folderId);

            var passwordCount = Convert.ToInt32(checkCmd.ExecuteScalar());
            if (passwordCount > 0)
            {
                // Déplacer les mots de passe vers le dossier "Général"
                var generalFolder = GetGeneralFolder();
                if (generalFolder != null)
                {
                    var moveCmd = new SqliteCommand(
                        "UPDATE Passwords SET FolderId = @generalFolderId WHERE FolderId = @folderId", 
                        connection);
                    moveCmd.Parameters.AddWithValue("@generalFolderId", generalFolder.Id);
                    moveCmd.Parameters.AddWithValue("@folderId", folderId);
                    moveCmd.ExecuteNonQuery();
                }
            }

            // Supprimer le dossier
            var deleteCmd = new SqliteCommand(
                "DELETE FROM Folders WHERE Id = @id AND UserId = @userId AND Name != 'Général'", 
                connection);
            deleteCmd.Parameters.AddWithValue("@id", folderId);
            deleteCmd.Parameters.AddWithValue("@userId", _currentUser.Id);

            return deleteCmd.ExecuteNonQuery() > 0;
        }

        public PasswordFolder? GetGeneralFolder()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = new SqliteCommand(
                "SELECT Id, Name, Description, Color, Icon, UserId, CreatedAt FROM Folders WHERE UserId = @userId AND Name = 'Général'", 
                connection);
            command.Parameters.AddWithValue("@userId", _currentUser.Id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new PasswordFolder
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Color = reader.GetString(3),
                    Icon = reader.GetString(4),
                    UserId = reader.GetInt32(5),
                    CreatedAt = reader.GetDateTime(6)
                };
            }

            return null;
        }

        public int GetPasswordCountInFolder(int folderId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Vérifier d'abord si la table Passwords existe et a la bonne structure
            var command = new SqliteCommand(@"
                CREATE TABLE IF NOT EXISTS Passwords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Site TEXT,
                    Username TEXT,
                    Password TEXT,
                    Url TEXT,
                    UserId INTEGER NOT NULL DEFAULT 0,
                    FolderId INTEGER NOT NULL DEFAULT 1,
                    FOREIGN KEY(FolderId) REFERENCES Folders(Id)
                );", connection);
            command.ExecuteNonQuery();

            // Vérifier si la colonne FolderId existe
            command = new SqliteCommand("PRAGMA table_info('Passwords');", connection);
            using var reader = command.ExecuteReader();
            var hasFolderIdColumn = false;
            while (reader.Read())
            {
                if (reader.GetString(1) == "FolderId")
                {
                    hasFolderIdColumn = true;
                    break;
                }
            }

            // Si la colonne n'existe pas, l'ajouter
            if (!hasFolderIdColumn)
            {
                command = new SqliteCommand(@"
                    ALTER TABLE Passwords ADD COLUMN FolderId INTEGER NOT NULL DEFAULT 1;
                    UPDATE Passwords SET FolderId = 1 WHERE FolderId IS NULL;", connection);
                command.ExecuteNonQuery();
            }

            // Maintenant on peut compter les mots de passe
            command = new SqliteCommand(
                "SELECT COUNT(*) FROM Passwords WHERE FolderId = @folderId AND UserId = @userId", 
                connection);
            command.Parameters.AddWithValue("@folderId", folderId);
            command.Parameters.AddWithValue("@userId", _currentUser.Id);

            return Convert.ToInt32(command.ExecuteScalar());
        }
    }
}
