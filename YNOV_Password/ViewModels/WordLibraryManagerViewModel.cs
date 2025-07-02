using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using YNOV_Password.Commands;
using YNOV_Password.Services;

namespace YNOV_Password.ViewModels
{
    public partial class WordLibraryManagerViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<WordLibraryInfo> libraries = new();

        [ObservableProperty]
        private WordLibraryInfo? selectedLibrary;

        [ObservableProperty]
        private string newLibraryName = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        public ICommand LoadLibrariesCommand { get; }
        public ICommand ImportWordsCommand { get; }
        public ICommand DeleteLibraryCommand { get; }
        public ICommand RefreshCommand { get; }

        public WordLibraryManagerViewModel()
        {
            LoadLibrariesCommand = new RelayCommand<object>(_ => LoadLibraries());
            ImportWordsCommand = new RelayCommand<object>(_ => ImportWords());
            DeleteLibraryCommand = new RelayCommand<WordLibraryInfo>(DeleteLibrary);
            RefreshCommand = new RelayCommand<object>(_ => LoadLibraries());

            LoadLibraries();
        }

        private void LoadLibraries()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Chargement des bibliothèques...";

                Libraries.Clear();
                var libraryNames = WordLibraryService.Instance.GetLibraryNames();

                foreach (var name in libraryNames)
                {
                    var wordCount = WordLibraryService.Instance.GetWordCount(name);
                    Libraries.Add(new WordLibraryInfo
                    {
                        Name = name,
                        WordCount = wordCount
                    });
                }

                StatusMessage = $"{Libraries.Count} bibliothèque(s) trouvée(s)";
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, "Chargement des bibliothèques de mots");
                StatusMessage = $"Erreur lors du chargement : {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void ImportWords()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Import en cours...";

                // Pour l'instant, utilisons le fichier français par défaut
                var defaultFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "liste_mots_francais.txt");
                
                if (!File.Exists(defaultFilePath))
                {
                    StatusMessage = "Fichier de mots français non trouvé. Placez 'liste_mots_francais.txt' dans le dossier du projet.";
                    return;
                }

                var libraryName = string.IsNullOrWhiteSpace(NewLibraryName) 
                    ? "Français" 
                    : NewLibraryName.Trim();

                var success = await WordLibraryService.Instance.LoadWordsFromFileAsync(defaultFilePath, libraryName);
                
                if (success)
                {
                    StatusMessage = $"Bibliothèque '{libraryName}' importée avec succès !";
                    NewLibraryName = string.Empty;
                    LoadLibraries(); // Rafraîchir la liste
                }
                else
                {
                    StatusMessage = "Erreur lors de l'import du fichier";
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, $"Importation du fichier de mots");
                StatusMessage = $"Erreur lors de l'import : {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void DeleteLibrary(WordLibraryInfo? library)
        {
            if (library == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = $"Suppression de '{library.Name}'...";

                WordLibraryService.Instance.DeleteLibrary(library.Name);
                
                StatusMessage = $"Bibliothèque '{library.Name}' supprimée";
                LoadLibraries(); // Rafraîchir la liste
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, $"Suppression de la bibliothèque '{library?.Name}'");
                StatusMessage = $"Erreur lors de la suppression : {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    public class WordLibraryInfo
    {
        public string Name { get; set; } = string.Empty;
        public int WordCount { get; set; }
        public string DisplayText => $"{Name} ({WordCount} mots)";
    }
}
