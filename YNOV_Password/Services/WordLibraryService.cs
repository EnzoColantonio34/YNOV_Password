using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace YNOV_Password.Services
{
    public class WordLibraryService
    {
        private static WordLibraryService? _instance;
        private List<string> _words = new();
        private WordDatabaseService? _dbService;
        private int _currentUserId;
        
        public static WordLibraryService Instance => _instance ??= new WordLibraryService();

        public IReadOnlyList<string> Words => _words.AsReadOnly();
        
        public bool HasWords => _words.Count > 0;

        public void SetUserId(int userId)
        {
            _currentUserId = userId;
            _dbService = new WordDatabaseService(userId);
            LoadWordsFromDatabase();
        }

        private void LoadWordsFromDatabase()
        {
            if (_dbService == null) return;
            
            try
            {
                _words = _dbService.GetAllWords();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des mots depuis la BDD: {ex.Message}");
                _words = new List<string>();
            }
        }

        public async Task<bool> LoadWordsFromFileAsync(string filePath, string libraryName = "Default")
        {
            try
            {
                var content = await File.ReadAllTextAsync(filePath);
                var words = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(word => word.Trim())
                    .Where(word => !string.IsNullOrWhiteSpace(word))
                    .ToList();

                // Sauvegarder en base de données
                if (_dbService != null)
                {
                    _dbService.SaveWords(words, libraryName);
                    LoadWordsFromDatabase(); // Recharger depuis la BDD
                }
                else
                {
                    _words = words; // Fallback si pas de BDD
                }
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement du fichier: {ex.Message}");
                return false;
            }
        }

        public List<string> GetRandomWords(int count)
        {
            if (!HasWords || count <= 0)
                return new List<string>();

            var random = new Random();
            var selectedWords = new List<string>();
            
            for (int i = 0; i < count; i++)
            {
                var randomIndex = random.Next(_words.Count);
                selectedWords.Add(_words[randomIndex]);
            }

            return selectedWords;
        }

        public string GeneratePasswordFromWords(int wordCount, string separator = "-")
        {
            var words = GetRandomWords(wordCount);
            return string.Join(separator, words);
        }

        public void ClearWords()
        {
            _words.Clear();
            
            // Supprimer aussi de la base de données si disponible
            if (_dbService != null)
            {
                try
                {
                    var libraries = _dbService.GetLibraryNames();
                    foreach (var library in libraries)
                    {
                        _dbService.DeleteLibrary(library);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur lors de la suppression des mots en BDD: {ex.Message}");
                }
            }
        }

        public List<string> GetLibraryNames()
        {
            return _dbService?.GetLibraryNames() ?? new List<string>();
        }

        public void LoadSpecificLibrary(string libraryName)
        {
            if (_dbService == null) return;
            
            try
            {
                _words = _dbService.GetWords(libraryName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement de la bibliothèque {libraryName}: {ex.Message}");
                _words = new List<string>();
            }
        }

        public void DeleteLibrary(string libraryName)
        {
            _dbService?.DeleteLibrary(libraryName);
            LoadWordsFromDatabase(); // Recharger après suppression
        }

        public int GetWordCount(string libraryName = "Default")
        {
            return _dbService?.GetWordCount(libraryName) ?? 0;
        }
    }
}