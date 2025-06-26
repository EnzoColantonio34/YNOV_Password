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
        
        public static WordLibraryService Instance => _instance ??= new WordLibraryService();

        public IReadOnlyList<string> Words => _words.AsReadOnly();
        
        public bool HasWords => _words.Count > 0;

        public async Task<bool> LoadWordsFromFileAsync(string filePath)
        {
            try
            {
                var content = await File.ReadAllTextAsync(filePath);
                var words = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(word => word.Trim())
                    .Where(word => !string.IsNullOrWhiteSpace(word))
                    .ToList();

                _words = words;
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
        }
    }
}