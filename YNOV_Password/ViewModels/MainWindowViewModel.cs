using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using YNOV_Password.Commands;
using YNOV_Password.Models;
using YNOV_Password.Services;
using YNOV_Password.Views;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace YNOV_Password.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<PasswordEntry> Passwords { get; } = new();
    public ObservableCollection<PasswordFolder> Folders { get; } = new();
    private readonly PasswordDatabaseService _dbService;
    private readonly FolderDatabaseService? _folderService;
    private readonly Dictionary<PasswordEntry, Timer> _passwordTimers = new();
    
    [ObservableProperty]
    private User? _currentUser;

    [ObservableProperty]
    private PasswordFolder? _selectedFolder;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public ICommand? CopyPasswordCommand { get; set; }
    public ICommand? CopyUrlCommand { get; set; }
    public ICommand? CopyUsernameCommand { get; set; }
    public ICommand? DeletePasswordCommand { get; set; }
    public ICommand? SearchCommand { get; set; }
    public ICommand? ShowPasswordCommand { get; set; }
    public ICommand? OpenUrlCommand { get; set; }
    public ICommand? AddPasswordCommand { get; set; }
    public ICommand? LogoutCommand { get; set; }
    public ICommand? ManageFoldersCommand { get; set; }
    public ICommand? SelectFolderCommand { get; set; }

    public event Action? LogoutRequested;

    public string UserInitial => CurrentUser?.Username?.Length > 0 
        ? CurrentUser.Username[0].ToString().ToUpper() 
        : "?";

    public bool HasNoPasswords => !Passwords.Any();
    public bool IsSearchActive => !string.IsNullOrWhiteSpace(SearchText);

    public string NoPasswordsMessageTitle => IsSearchActive
        ? "Aucun résultat trouvé"
        : "Aucun mot de passe enregistré";

    public string NoPasswordsMessageSubtitle => IsSearchActive
        ? "Essayez d'autres termes de recherche"
        : "Ajoutez un nouveau mot de passe pour commencer";

    public MainWindowViewModel() : this(0)
    {
    }

    public MainWindowViewModel(int userId)
    {
        try
        {
            var userService = new UserDatabaseService();
            var user = userService.GetUserById(userId);
            
            if (user != null)
            {
                CurrentUser = user;
                // Créer d'abord le FolderService pour s'assurer que la table Folders existe
                _folderService = new FolderDatabaseService(CurrentUser);
                // Ensuite créer le PasswordService qui dépend de la table Folders
                _dbService = new PasswordDatabaseService(CurrentUser.Id);
                WordLibraryService.Instance.SetUserId(CurrentUser.Id);
            }
            else
            {
                // Si pas d'utilisateur courant, on crée quand même les services dans le bon ordre
                _folderService = new FolderDatabaseService(new User { Id = userId });
                _dbService = new PasswordDatabaseService(userId);
                WordLibraryService.Instance.SetUserId(userId);
            }

            InitializeCommands();
            LoadFolders();
            PerformSearch(string.Empty);
        }
        catch (Exception ex)
        {
            LoggingService.LogError(ex, "Initialisation du MainWindowViewModel");
            // En cas d'erreur, on crée quand même les services dans le bon ordre
            _folderService = new FolderDatabaseService(new User { Id = userId });
            _dbService = new PasswordDatabaseService(userId);
            InitializeCommands();
        }
    }

    private void InitializeCommands()
    {
        CopyPasswordCommand = new RelayCommand<string>(CopyToClipboard);
        CopyUsernameCommand = new RelayCommand<string>(CopyUsernameToClipboard);
        CopyUrlCommand = new RelayCommand<string>(CopyUrlToClipboard);
        DeletePasswordCommand = new RelayCommand<PasswordEntry>(DeletePassword);
        SearchCommand = new RelayCommand<string>(PerformSearch);
        ShowPasswordCommand = new RelayCommand<PasswordEntry>(ShowPassword);
        OpenUrlCommand = new RelayCommand<string>(OpenUrl);
        AddPasswordCommand = new RelayCommand<object>(_ => ShowAddPasswordDialog());
        LogoutCommand = new RelayCommand<object>(_ => LogoutRequested?.Invoke());
        ManageFoldersCommand = new RelayCommand<object>(_ => ShowFolderManager());
        SelectFolderCommand = new RelayCommand<PasswordFolder>(SelectFolder);
    }

    private void LoadFolders()
    {
        if (_folderService == null) return;

        Folders.Clear();
        var folders = _folderService.GetAllFolders();
        foreach (var folder in folders)
        {
            folder.PasswordCount = _folderService.GetPasswordCountInFolder(folder.Id);
            Folders.Add(folder);
        }

        // Sélectionner le dossier "Général" par défaut
        if (SelectedFolder == null)
        {
            SelectedFolder = Folders.FirstOrDefault(f => f.Name == "Général");
        }
    }

    // Méthodes de gestion du presse-papier
    private async void CopyToClipboard(string password)
    {
        if (password != null)
        {
            var topLevel = TopLevel.GetTopLevel((App.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);
            if (topLevel?.Clipboard != null)
            {
                await topLevel.Clipboard.SetTextAsync(password);
            }
        }
    }

    private async void CopyUrlToClipboard(string url)
    {
        if (url != null)
        {
            var topLevel = TopLevel.GetTopLevel((App.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);
            if (topLevel?.Clipboard != null)
            {
                await topLevel.Clipboard.SetTextAsync(url);
            }
        }
    }

    private async void CopyUsernameToClipboard(string username)
    {
        if (username != null)
        {
            var topLevel = TopLevel.GetTopLevel((App.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);
            if (topLevel?.Clipboard != null)
            {
                await topLevel.Clipboard.SetTextAsync(username);
            }
        }
    }

    // Méthodes de gestion des URLs et mots de passe
    private void OpenUrl(string url)
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            try
            {
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "https://" + url;
                }
                
                var psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception)
            {
                // Log l'erreur si nécessaire
            }
        }
    }

    private void DeletePassword(PasswordEntry entry)
    {
        if (entry != null)
        {
            try
            {
                _dbService.Delete(entry.Id);
                Passwords.Remove(entry);
                UpdateFolderPasswordCounts();
                OnPropertyChanged(nameof(HasNoPasswords));
                OnPropertyChanged(nameof(IsSearchActive));
                OnPropertyChanged(nameof(NoPasswordsMessageTitle));
                OnPropertyChanged(nameof(NoPasswordsMessageSubtitle));
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, "Suppression du mot de passe");
            }
        }
    }

    private void ShowPassword(PasswordEntry? entry)
    {
        if (entry == null) return;

        if (_passwordTimers.ContainsKey(entry))
        {
            _passwordTimers[entry].Dispose();
            _passwordTimers.Remove(entry);
        }

        entry.IsPasswordVisible = true;
        entry.RemainingTime = 10;

        var timer = new Timer(_ =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                entry.RemainingTime--;
                
                if (entry.RemainingTime <= 0)
                {
                    entry.IsPasswordVisible = false;
                    entry.RemainingTime = 0;
                    
                    if (_passwordTimers.ContainsKey(entry))
                    {
                        _passwordTimers[entry].Dispose();
                        _passwordTimers.Remove(entry);
                    }
                }
            });
        }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

        _passwordTimers[entry] = timer;
    }

    public void HidePassword(PasswordEntry? entry)
    {
        if (entry == null) return;

        if (_passwordTimers.ContainsKey(entry))
        {
            _passwordTimers[entry].Dispose();
            _passwordTimers.Remove(entry);
        }

        entry.IsPasswordVisible = false;
        entry.RemainingTime = 0;
    }

    // Méthodes de recherche et rafraîchissement
    public void RefreshPasswords()
    {
        PerformSearch(SearchText);
    }

    private void PerformSearch(string? searchTerm)
    {
        Passwords.Clear();

        List<PasswordEntry> entries;
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            entries = _dbService.GetAll();
        }
        else
        {
            entries = _dbService.Search(searchTerm);
        }

        if (SelectedFolder != null)
        {
            entries = entries.Where(p => p.FolderId == SelectedFolder.Id).ToList();
        }

        DuplicatePasswordService.MarkDuplicatePasswords(entries);

        foreach (var entry in entries)
        {
            Passwords.Add(entry);
        }

        UpdateFolderPasswordCounts();

        OnPropertyChanged(nameof(HasNoPasswords));
        OnPropertyChanged(nameof(IsSearchActive));
        OnPropertyChanged(nameof(NoPasswordsMessageTitle));
        OnPropertyChanged(nameof(NoPasswordsMessageSubtitle));
    }

    partial void OnSearchTextChanged(string value)
    {
        PerformSearch(value);
        OnPropertyChanged(nameof(IsSearchActive));
        OnPropertyChanged(nameof(NoPasswordsMessageTitle));
        OnPropertyChanged(nameof(NoPasswordsMessageSubtitle));
    }

    // Méthodes de gestion des fenêtres
    private async void ShowAddPasswordDialog()
    {
        try
        {
            var mainWindow = (App.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow != null)
            {
                var dialog = new AddPasswordWindow(this);
                await dialog.ShowDialog(mainWindow);
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogError(ex, "Affichage de la boîte de dialogue d'ajout de mot de passe");
        }
    }

    public async System.Threading.Tasks.Task ShowAddPasswordDialogWithGeneratedPassword(string generatedPassword)
    {
        try
        {
            var mainWindow = (App.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow != null)
            {
                var dialog = new AddPasswordWindow(this, generatedPassword);
                await dialog.ShowDialog(mainWindow);
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogError(ex, "Affichage de la boîte de dialogue d'ajout de mot de passe avec mot de passe généré");
        }
    }

    private async void ShowFolderManager()
    {
        if (CurrentUser == null || _folderService == null) return;

        var viewModel = new FolderManagerViewModel(CurrentUser);
        var window = new FolderManagerWindow(viewModel);
        
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow != null)
        {
            await window.ShowDialog(desktop.MainWindow);
            LoadFolders();
        }
    }

    private void UpdateFolderPasswordCounts()
    {
        if (_folderService == null) return;

        foreach (var folder in Folders)
        {
            folder.PasswordCount = _folderService.GetPasswordCountInFolder(folder.Id);
        }
    }

    // Méthodes CRUD
    public void AddPassword(PasswordEntry entry)
    {
        try
        {
            _dbService.Add(entry);
            PerformSearch(SearchText);
            UpdateFolderPasswordCounts();
        }
        catch (Exception ex)
        {
            LoggingService.LogError(ex, $"Ajout du mot de passe pour '{entry?.Site}'");
        }
    }

    public void UpdatePassword(PasswordEntry entry)
    {
        try
        {
            _dbService.Update(entry);
            PerformSearch(SearchText);
            UpdateFolderPasswordCounts();
        }
        catch (Exception ex)
        {
            LoggingService.LogError(ex, $"Mise à jour du mot de passe pour '{entry?.Site}'");
        }
    }

    // Méthodes de gestion des dossiers
    private void SelectFolder(PasswordFolder? folder)
    {
        // Désélectionner l'ancien dossier
        if (SelectedFolder != null)
        {
            SelectedFolder.IsSelected = false;
        }

        // Sélectionner le nouveau dossier
        SelectedFolder = folder;
        if (folder != null)
        {
            folder.IsSelected = true;
        }

        // Rafraîchir la liste des mots de passe
        PerformSearch(SearchText);
    }

    partial void OnSelectedFolderChanged(PasswordFolder? oldValue, PasswordFolder? newValue)
    {
        PerformSearch(SearchText);
    }
}