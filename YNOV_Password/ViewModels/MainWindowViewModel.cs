using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Diagnostics;

namespace YNOV_Password.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<PasswordEntry> Passwords { get; set; }
    private readonly PasswordDatabaseService _dbService;
    private readonly Dictionary<PasswordEntry, Timer> _passwordTimers = new();
    
    [ObservableProperty]
    private User? _currentUser;

    // Propriété calculée pour la première lettre du nom d'utilisateur
    public string UserInitial => CurrentUser?.Username?.Length > 0 
        ? CurrentUser.Username[0].ToString().ToUpper() 
        : "?";

    partial void OnCurrentUserChanged(User? value)
    {
        OnPropertyChanged(nameof(UserInitial));
    }

    [ObservableProperty]
    private string _searchText = string.Empty;

    public bool HasNoPasswords => !Passwords.Any();
    public bool IsSearchActive => !string.IsNullOrWhiteSpace(SearchText);
    public string NoPasswordsMessageTitle => IsSearchActive 
        ? "Aucun élément correspondant à la recherche..." 
        : "Pas encore de mot de passe créé !";
    public string NoPasswordsMessageSubtitle => IsSearchActive 
        ? $"Aucun résultat trouvé pour \"{SearchText}\"" 
        : "Ne tarde pas pour te sécuriser";

    public ICommand CopyPasswordCommand { get; private set; } = null!;
    public ICommand CopyUsernameCommand { get; private set; } = null!;
    public ICommand CopyUrlCommand { get; private set; } = null!;
    public ICommand DeletePasswordCommand { get; private set; } = null!;
    public ICommand SearchCommand { get; private set; } = null!;
    public ICommand AddPasswordCommand { get; private set; } = null!;
    public ICommand ShowPasswordCommand { get; private set; } = null!;
    public ICommand OpenUrlCommand { get; private set; } = null!;
    public ICommand LogoutCommand { get; private set; } = null!;

    public event Action? LogoutRequested;

    public MainWindowViewModel() : this(0)
    {
    }

    public MainWindowViewModel(int userId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"MainWindowViewModel: Initialisation avec userId = {userId}");
            
            if (userId == 0)
            {
                var userService = new UserDatabaseService();
                var defaultUser = userService.GetUserByEmail("admin@example.com");
                userId = defaultUser?.Id ?? 1;
                CurrentUser = defaultUser;
                System.Diagnostics.Debug.WriteLine($"MainWindowViewModel: Utilisateur par défaut chargé = {CurrentUser?.Username}");
            }
            else
            {
                var userService = new UserDatabaseService();
                var users = userService.GetAllUsers();
                CurrentUser = users.FirstOrDefault(u => u.Id == userId);
                System.Diagnostics.Debug.WriteLine($"MainWindowViewModel: Utilisateur chargé = {CurrentUser?.Username}");
            }
            
            if (CurrentUser != null)
            {
                _dbService = new PasswordDatabaseService(CurrentUser);
            }
            else
            {
                _dbService = new PasswordDatabaseService(userId);
            }
            
            // Initialiser la collection vide d'abord
            Passwords = new ObservableCollection<PasswordEntry>();
            
            // Charger les mots de passe via PerformSearch pour inclure la détection de doublons
            PerformSearch(string.Empty);
            
            System.Diagnostics.Debug.WriteLine($"MainWindowViewModel: {Passwords.Count} mots de passe chargés");

            CopyPasswordCommand = new Commands.RelayCommand<string>(CopyToClipboard);
            CopyUsernameCommand = new Commands.RelayCommand<string>(CopyUsernameToClipboard);
            CopyUrlCommand = new Commands.RelayCommand<string>(CopyUrlToClipboard);
            DeletePasswordCommand = new Commands.RelayCommand<PasswordEntry>(DeletePassword);
            SearchCommand = new Commands.RelayCommand<string>(PerformSearch);
            ShowPasswordCommand = new Commands.RelayCommand<PasswordEntry>(ShowPassword);
            OpenUrlCommand = new Commands.RelayCommand<string>(OpenUrl);
            AddPasswordCommand = new Commands.RelayCommand<object>(_ => ShowAddPasswordDialog());
            LogoutCommand = new Commands.RelayCommand<object>(_ => LogoutRequested?.Invoke());
            
            System.Diagnostics.Debug.WriteLine("MainWindowViewModel: Initialisation terminée avec succès");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MainWindowViewModel: Erreur lors de l'initialisation = {ex.Message}");
            // Initialiser avec des valeurs par défaut pour éviter les crashes
            Passwords = new ObservableCollection<PasswordEntry>();
            _dbService = new PasswordDatabaseService(userId);
            
            CopyPasswordCommand = new Commands.RelayCommand<string>(CopyToClipboard);
            CopyUsernameCommand = new Commands.RelayCommand<string>(CopyUsernameToClipboard);
            CopyUrlCommand = new Commands.RelayCommand<string>(CopyUrlToClipboard);
            DeletePasswordCommand = new Commands.RelayCommand<PasswordEntry>(DeletePassword);
            SearchCommand = new Commands.RelayCommand<string>(PerformSearch);
            ShowPasswordCommand = new Commands.RelayCommand<PasswordEntry>(ShowPassword);
            OpenUrlCommand = new Commands.RelayCommand<string>(OpenUrl);
            AddPasswordCommand = new Commands.RelayCommand<object>(_ => ShowAddPasswordDialog());
            LogoutCommand = new Commands.RelayCommand<object>(_ => LogoutRequested?.Invoke());
        }
    }

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

    private void OpenUrl(string url)
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            try
            {
                // Ajouter https:// si aucun protocole n'est spécifié
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
                OnPropertyChanged(nameof(HasNoPasswords));
                OnPropertyChanged(nameof(IsSearchActive));
                OnPropertyChanged(nameof(NoPasswordsMessageTitle));
                OnPropertyChanged(nameof(NoPasswordsMessageSubtitle));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la suppression: {ex.Message}");
            }
        }
    }

    private void ShowPassword(PasswordEntry? entry)
    {
        if (entry == null) return;

        // Annuler le timer existant s'il y en a un
        if (_passwordTimers.ContainsKey(entry))
        {
            _passwordTimers[entry].Dispose();
            _passwordTimers.Remove(entry);
        }

        // Afficher le mot de passe
        entry.IsPasswordVisible = true;
        entry.RemainingTime = 10;

        // Créer un timer qui se déclenche chaque seconde pour mettre à jour le countdown
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

        // Annuler le timer s'il existe
        if (_passwordTimers.ContainsKey(entry))
        {
            _passwordTimers[entry].Dispose();
            _passwordTimers.Remove(entry);
        }

        // Masquer le mot de passe et réinitialiser le timer
        entry.IsPasswordVisible = false;
        entry.RemainingTime = 0;
    }

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

        // Marquer les mots de passe dupliqués AVANT d'ajouter à la collection
        DuplicatePasswordService.MarkDuplicatePasswords(entries);

        foreach (var entry in entries)
        {
            Passwords.Add(entry);
        }

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
            System.Diagnostics.Debug.WriteLine($"Erreur lors de l'affichage de la boîte de dialogue: {ex}");
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
            System.Diagnostics.Debug.WriteLine($"Erreur lors de l'affichage de la boîte de dialogue: {ex}");
        }
    }

    public void AddPassword(PasswordEntry entry)
    {
        try
        {
            _dbService.Add(entry);
            PerformSearch(SearchText);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de l'ajout: {ex.Message}");
        }
    }

    public void UpdatePassword(PasswordEntry entry)
    {
        try
        {
            _dbService.Update(entry);
            PerformSearch(SearchText);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la mise à jour: {ex.Message}");
        }
    }
}