using System.Windows.Input;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using YNOV_Password.Commands;
using YNOV_Password.Models;

namespace YNOV_Password.ViewModels
{
    public partial class AddPasswordViewModel : ViewModelBase
    {
        private readonly Window _window;
        private readonly MainWindowViewModel _mainViewModel;
        private readonly PasswordEntry? _editingEntry;
        private readonly bool _isEditing;

        [ObservableProperty]
        private string _site = string.Empty;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _url = string.Empty;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public System.Collections.ObjectModel.ObservableCollection<PasswordFolder> Folders { get; }

        [ObservableProperty]
        private PasswordFolder? _selectedFolder;

        public string WindowTitle => _isEditing ? "Modifier le mot de passe" : "Ajouter un mot de passe";

        public AddPasswordViewModel(Window window, MainWindowViewModel mainViewModel)
        {
            _window = window;
            _mainViewModel = mainViewModel;
            _isEditing = false;

            // Initialiser la liste des dossiers
            Folders = mainViewModel.Folders;
            if (mainViewModel.SelectedFolder != null)
            {
                SelectedFolder = mainViewModel.SelectedFolder;
            }
            else
            {
                // Sélectionner le dossier "Général" par défaut
                SelectedFolder = Folders.FirstOrDefault(f => f.Name == "Général");
            }

            SaveCommand = new RelayCommand<object>(_ => Save());
            CancelCommand = new RelayCommand<object>(_ => Cancel());
        }

        public AddPasswordViewModel(Window window, MainWindowViewModel mainViewModel, string preGeneratedPassword) : this(window, mainViewModel)
        {
            Password = preGeneratedPassword;
        }

        public AddPasswordViewModel(Window window, MainWindowViewModel mainViewModel, PasswordEntry editingEntry) : this(window, mainViewModel)
        {
            _editingEntry = editingEntry;
            _isEditing = true;
            
            // Pré-remplir les champs avec les valeurs existantes
            Site = editingEntry.Site ?? string.Empty;
            Username = editingEntry.Username ?? string.Empty;
            Password = editingEntry.Password ?? string.Empty;
            Url = editingEntry.Url ?? string.Empty;
        }

        private void Save()
        {
            if (!string.IsNullOrWhiteSpace(Site) && !string.IsNullOrWhiteSpace(Password))
            {
                if (_isEditing && _editingEntry != null)
                {
                    // Mettre à jour l'entrée existante
                    _editingEntry.Site = Site;
                    _editingEntry.Username = Username;
                    _editingEntry.Password = Password;
                    _editingEntry.Url = Url;
                    _editingEntry.FolderId = SelectedFolder?.Id;
                    
                    _mainViewModel.UpdatePassword(_editingEntry);
                }
                else
                {
                    // Créer une nouvelle entrée
                    var newEntry = new PasswordEntry
                    {
                        Site = Site,
                        Username = Username,
                        Password = Password,
                        Url = Url,
                        FolderId = SelectedFolder?.Id
                    };

                    _mainViewModel.AddPassword(newEntry);
                }
                
                _window.Close();
            }
        }

        private void Cancel()
        {
            _window.Close();
        }

        public void SaveEntry()
        {
            Save();
        }

        public void CancelEntry()
        {
            Cancel();
        }
    }
}
