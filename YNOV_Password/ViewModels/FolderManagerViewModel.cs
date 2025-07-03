using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using YNOV_Password.Commands;
using YNOV_Password.Models;
using YNOV_Password.Services;

namespace YNOV_Password.ViewModels
{
    public partial class FolderManagerViewModel : ViewModelBase
    {
        private readonly FolderDatabaseService _folderService;
        private readonly User _currentUser;

        public ObservableCollection<PasswordFolder> Folders { get; set; }

        [ObservableProperty]
        private PasswordFolder? _selectedFolder;

        [ObservableProperty]
        private bool _isEditMode = false;

        [ObservableProperty]
        private string _newFolderName = string.Empty;

        [ObservableProperty]
        private string _newFolderDescription = string.Empty;

        [ObservableProperty]
        private string _newFolderColor = "#3B82F6";

        [ObservableProperty]
        private string _newFolderIcon = "📁";

        public ICommand CreateFolderCommand { get; }
        public ICommand EditFolderCommand { get; }
        public ICommand SaveFolderCommand { get; }
        public ICommand DeleteFolderCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand SelectFolderCommand { get; }

        // Couleurs prédéfinies pour les dossiers
        public ObservableCollection<string> PredefinedColors { get; } = new()
        {
            "#3B82F6", // Bleu
            "#10B981", // Vert
            "#F59E0B", // Orange
            "#EF4444", // Rouge
            "#8B5CF6", // Violet
            "#06B6D4", // Cyan
            "#F97316", // Orange foncé
            "#84CC16", // Lime
            "#EC4899", // Rose
            "#6B7280"  // Gris
        };

        // Icônes prédéfinies pour les dossiers
        public ObservableCollection<string> PredefinedIcons { get; } = new()
        {
            "📁", "🔒", "💼", "🏠", "🌐", "💳", "🎮", "📱", "💻", "🛒",
            "🏦", "🎵", "📺", "🚗", "✈️", "🏥", "🎓", "💡", "⚙️", "🔧"
        };

        public FolderManagerViewModel(User user)
        {
            _currentUser = user;
            _folderService = new FolderDatabaseService(user);
            Folders = new ObservableCollection<PasswordFolder>();

            CreateFolderCommand = new RelayCommand<object>(_ => CreateFolder());
            EditFolderCommand = new RelayCommand<PasswordFolder>(EditFolder);
            SaveFolderCommand = new RelayCommand<object>(_ => SaveFolder());
            DeleteFolderCommand = new RelayCommand<PasswordFolder>(DeleteFolder);
            CancelEditCommand = new RelayCommand<object>(_ => CancelEdit());
            SelectFolderCommand = new RelayCommand<PasswordFolder>(SelectFolder);

            LoadFolders();
        }

        private void LoadFolders()
        {
            Folders.Clear();
            var folders = _folderService.GetAllFolders();
            foreach (var folder in folders)
            {
                folder.PasswordCount = _folderService.GetPasswordCountInFolder(folder.Id);
                Folders.Add(folder);
            }
        }

        private void CreateFolder()
        {
            if (string.IsNullOrWhiteSpace(NewFolderName))
                return;

            var newFolder = new PasswordFolder
            {
                Name = NewFolderName.Trim(),
                Description = string.IsNullOrWhiteSpace(NewFolderDescription) ? null : NewFolderDescription.Trim(),
                Color = NewFolderColor,
                Icon = NewFolderIcon,
                UserId = _currentUser.Id
            };

            var folderId = _folderService.CreateFolder(newFolder);
            newFolder.Id = folderId;

            Folders.Add(newFolder);
            ClearNewFolderFields();
        }

        private void EditFolder(PasswordFolder? folder)
        {
            if (folder == null || folder.Name == "Général")
                return;

            SelectedFolder = folder;
            NewFolderName = folder.Name;
            NewFolderDescription = folder.Description ?? string.Empty;
            NewFolderColor = folder.Color;
            NewFolderIcon = folder.Icon;
            IsEditMode = true;
        }

        private void SaveFolder()
        {
            if (SelectedFolder == null || string.IsNullOrWhiteSpace(NewFolderName))
                return;

            SelectedFolder.Name = NewFolderName.Trim();
            SelectedFolder.Description = string.IsNullOrWhiteSpace(NewFolderDescription) ? null : NewFolderDescription.Trim();
            SelectedFolder.Color = NewFolderColor;
            SelectedFolder.Icon = NewFolderIcon;

            _folderService.UpdateFolder(SelectedFolder);
            CancelEdit();
        }

        private void DeleteFolder(PasswordFolder? folder)
        {
            if (folder == null || folder.Name == "Général")
                return;

            if (_folderService.DeleteFolder(folder.Id))
            {
                Folders.Remove(folder);
                // Recharger pour mettre à jour les compteurs de mots de passe
                LoadFolders();
            }
        }

        private void SelectFolder(PasswordFolder? folder)
        {
            SelectedFolder = folder;
            if (!IsEditMode)
            {
                ClearNewFolderFields();
            }
        }

        private void CancelEdit()
        {
            IsEditMode = false;
            SelectedFolder = null;
            ClearNewFolderFields();
        }

        private void ClearNewFolderFields()
        {
            NewFolderName = string.Empty;
            NewFolderDescription = string.Empty;
            NewFolderColor = "#3B82F6";
            NewFolderIcon = "📁";
        }
    }
}
