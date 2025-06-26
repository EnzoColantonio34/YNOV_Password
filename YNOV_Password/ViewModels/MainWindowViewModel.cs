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

    // Méthode appelée quand CurrentUser change
    partial void OnCurrentUserChanged(User? value)
    {
        OnPropertyChanged(nameof(UserInitial));
    }

    [ObservableProperty]
    private string _searchText = string.Empty;

    // Propriété pour afficher un message quand il n'y a pas de mots de passe
    public bool HasNoPasswords => !Passwords.Any();
    public string NoPasswordsMessageTitle => "Pas encore de mot de passe créé !";
    public string NoPasswordsMessageSubtitle => "Ne tarde pas pour te sécuriser";

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
        // Si aucun userId n'est fourni, utiliser l'utilisateur par défaut (ID = 1)
        if (userId == 0)
        {
            var userService = new UserDatabaseService();
            var defaultUser = userService.GetUserByEmail("admin@example.com");
            userId = defaultUser?.Id ?? 1;
            CurrentUser = defaultUser;
        }
        else
        {
            // Récupérer les informations de l'utilisateur connecté
            var userService = new UserDatabaseService();
            var users = userService.GetAllUsers();
            CurrentUser = users.FirstOrDefault(u => u.Id == userId);
        }
        
        // Utiliser le constructeur qui prend l'utilisateur pour le chiffrement
        if (CurrentUser != null)
        {
            _dbService = new PasswordDatabaseService(CurrentUser);
        }
        else
        {
            // Fallback vers l'ancien constructeur
            _dbService = new PasswordDatabaseService(userId);
        }
        
        Passwords = new ObservableCollection<PasswordEntry>(_dbService.GetAll());
        
        // Notifier le changement initial pour HasNoPasswords
        OnPropertyChanged(nameof(HasNoPasswords));

        CopyPasswordCommand = new Commands.RelayCommand<string>(CopyToClipboard);
        CopyUsernameCommand = new Commands.RelayCommand<string>(CopyUsernameToClipboard);
        CopyUrlCommand = new Commands.RelayCommand<string>(CopyUrlToClipboard);
        DeletePasswordCommand = new Commands.RelayCommand<PasswordEntry>(DeletePassword);
        SearchCommand = new Commands.RelayCommand<string>(PerformSearch);
        ShowPasswordCommand = new Commands.RelayCommand<PasswordEntry>(ShowPassword);
        OpenUrlCommand = new Commands.RelayCommand<string>(OpenUrl);
        AddPasswordCommand = new Commands.RelayCommand<object>(_ => {
            System.Diagnostics.Debug.WriteLine("[DEBUG] AddPasswordCommand exécutée");
            ShowAddPasswordDialog();
        });
        LogoutCommand = new Commands.RelayCommand<object>(_ => {
            System.Diagnostics.Debug.WriteLine("[DEBUG] LogoutCommand exécutée");
            LogoutRequested?.Invoke();
        });
        
        System.Diagnostics.Debug.WriteLine("[DEBUG] Toutes les commandes ont été initialisées");
        System.Diagnostics.Debug.WriteLine($"[DEBUG] CopyPasswordCommand: {CopyPasswordCommand != null}");
        System.Diagnostics.Debug.WriteLine($"[DEBUG] CopyUrlCommand: {CopyUrlCommand != null}");
        System.Diagnostics.Debug.WriteLine($"[DEBUG] DeletePasswordCommand: {DeletePasswordCommand != null}");
    }

    private async void CopyToClipboard(string password)
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] CopyToClipboard appelée avec: {password}");
        if (password != null)
        {
            var topLevel = TopLevel.GetTopLevel((App.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);
            if (topLevel?.Clipboard != null)
            {
                await topLevel.Clipboard.SetTextAsync(password);
                System.Diagnostics.Debug.WriteLine("[DEBUG] Mot de passe copié dans le presse-papiers");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] Impossible d'accéder au presse-papiers");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] Mot de passe est null");
        }
    }

    private async void CopyUrlToClipboard(string url)
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] CopyUrlToClipboard appelée avec: {url}");
        if (url != null)
        {
            var topLevel = TopLevel.GetTopLevel((App.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);
            if (topLevel?.Clipboard != null)
            {
                await topLevel.Clipboard.SetTextAsync(url);
                System.Diagnostics.Debug.WriteLine("[DEBUG] URL copiée dans le presse-papiers");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] Impossible d'accéder au presse-papiers");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] URL est null");
        }
    }

    private async void CopyUsernameToClipboard(string username)
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] CopyUsernameToClipboard appelée avec: {username}");
        if (username != null)
        {
            var topLevel = TopLevel.GetTopLevel((App.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);
            if (topLevel?.Clipboard != null)
            {
                await topLevel.Clipboard.SetTextAsync(username);
                System.Diagnostics.Debug.WriteLine("[DEBUG] Nom d'utilisateur copié dans le presse-papiers");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] Impossible d'accéder au presse-papiers");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] Nom d'utilisateur est null");
        }
    }

    private void OpenUrl(string url)
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] OpenUrl appelée avec: {url}");
        if (!string.IsNullOrWhiteSpace(url))
        {
            try
            {
                // Ajouter https:// si aucun protocole n'est spécifié
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "https://" + url;
                }
                
                // Utiliser Process.Start pour ouvrir l'URL dans le navigateur par défaut
                var psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
                System.Diagnostics.Debug.WriteLine($"[DEBUG] URL ouverte: {url}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Erreur lors de l'ouverture de l'URL: {ex.Message}");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] URL est vide ou null");
        }
    }

    private void DeletePassword(PasswordEntry entry)
    {
        System.Diagnostics.Debug.WriteLine($"[DEBUG] DeletePassword appelée avec: {entry?.Site}");
        if (entry != null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Suppression de l'entrée ID: {entry.Id}");
                // Supprimer de la base de données
                _dbService.Delete(entry.Id);
                
                // Supprimer de la collection affichée
                Passwords.Remove(entry);
                
                // Notifier le changement pour la propriété HasNoPasswords
                OnPropertyChanged(nameof(HasNoPasswords));
                
                System.Diagnostics.Debug.WriteLine("[DEBUG] Entrée supprimée avec succès");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la suppression: {ex.Message}");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] Entry est null");
        }
    }

    private void ShowPassword(PasswordEntry? entry)
    {
        if (entry == null) 
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] ShowPassword: entry est null");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"[DEBUG] ShowPassword appelée pour {entry.Site}");
        System.Diagnostics.Debug.WriteLine($"[DEBUG] Password avant: {entry.Password}");
        System.Diagnostics.Debug.WriteLine($"[DEBUG] IsPasswordVisible avant: {entry.IsPasswordVisible}");

        // Annuler le timer existant s'il y en a un
        if (_passwordTimers.ContainsKey(entry))
        {
            _passwordTimers[entry].Dispose();
            _passwordTimers.Remove(entry);
            System.Diagnostics.Debug.WriteLine("[DEBUG] Timer existant supprimé");
        }

        // Afficher le mot de passe
        entry.IsPasswordVisible = true;
        entry.RemainingTime = 10; // Initialiser le temps restant à 10 secondes
        System.Diagnostics.Debug.WriteLine($"[DEBUG] IsPasswordVisible après: {entry.IsPasswordVisible}");

        // Créer un timer qui se déclenche chaque seconde pour mettre à jour le countdown
        var timer = new Timer(_ =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                entry.RemainingTime--;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Temps restant pour {entry.Site}: {entry.RemainingTime}s");
                
                if (entry.RemainingTime <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Timer expiré pour {entry.Site}, masquage du mot de passe");
                    entry.IsPasswordVisible = false;
                    entry.RemainingTime = 0;
                    
                    if (_passwordTimers.ContainsKey(entry))
                    {
                        _passwordTimers[entry].Dispose();
                        _passwordTimers.Remove(entry);
                    }
                }
            });
        }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)); // Se déclenche chaque seconde

        _passwordTimers[entry] = timer;
        System.Diagnostics.Debug.WriteLine("[DEBUG] Timer de 10 secondes démarré");
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

    private void PerformSearch(string? searchTerm)
    {
        Passwords.Clear();

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            // Si la recherche est vide, afficher tous les mots de passe
            foreach (var entry in _dbService.GetAll())
            {
                Passwords.Add(entry);
            }
        }
        else
        {
            // Sinon, effectuer la recherche
            foreach (var entry in _dbService.Search(searchTerm))
            {
                Passwords.Add(entry);
            }
        }

        // Notifier le changement pour la propriété HasNoPasswords
        OnPropertyChanged(nameof(HasNoPasswords));
    }

    partial void OnSearchTextChanged(string value)
    {
        PerformSearch(value);
    }

    private async void ShowAddPasswordDialog()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] Clic sur Ajouter - ouverture de la fenêtre d'ajout...");
            var mainWindow = (App.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow != null)
            {
                await MessageBox(mainWindow, "Commande AddPasswordCommand appelée !");
                var dialog = new AddPasswordWindow(this);
                await dialog.ShowDialog(mainWindow);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("MainWindow est null, impossible d'afficher la boîte de dialogue");
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de l'affichage de la boîte de dialogue: {ex}");
        }
    }

    public async System.Threading.Tasks.Task ShowAddPasswordDialogWithGeneratedPassword(string generatedPassword)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] Ouverture de la fenêtre d'ajout avec mot de passe généré...");
            var mainWindow = (App.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow != null)
            {
                var dialog = new AddPasswordWindow(this, generatedPassword);
                await dialog.ShowDialog(mainWindow);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("MainWindow est null, impossible d'afficher la boîte de dialogue");
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de l'affichage de la boîte de dialogue: {ex}");
        }
    }

    private async System.Threading.Tasks.Task MessageBox(Window parent, string message)
    {
        var msgBox = new Window
        {
            Title = "Info",
            Width = 300,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = message, Margin = new Avalonia.Thickness(20), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center },
                    new Button { Content = "OK", Width = 60, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness(0,10,0,0) }
                }
            }
        };
        ((Button)((StackPanel)msgBox.Content).Children[1]).Click += (_, _) => msgBox.Close();
        await msgBox.ShowDialog(parent);
    }

    public void AddPassword(PasswordEntry entry)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Ajout du mot de passe pour {entry.Site}");
            
            // Ajouter à la base de données
            _dbService.Add(entry);
            
            System.Diagnostics.Debug.WriteLine("[DEBUG] Mot de passe ajouté à la base de données");
            
            // Rafraîchir la liste affichée
            PerformSearch(SearchText);
            
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Liste rafraîchie, {Passwords.Count} éléments");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Erreur lors de l'ajout: {ex.Message}");
        }
    }
}