using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using YNOV_Password.Commands;
using YNOV_Password.Models;
using YNOV_Password.Services;
using YNOV_Password.Views;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;

namespace YNOV_Password.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<PasswordEntry> Passwords { get; set; }
    private readonly PasswordDatabaseService _dbService;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public ICommand CopyPasswordCommand { get; private set; } = null!;
    public ICommand CopyUrlCommand { get; private set; } = null!;
    public ICommand DeletePasswordCommand { get; private set; } = null!;
    public ICommand SearchCommand { get; private set; } = null!;
    public ICommand AddPasswordCommand { get; private set; } = null!;

    public MainWindowViewModel()
    {
        _dbService = new PasswordDatabaseService();
        Passwords = new ObservableCollection<PasswordEntry>(_dbService.GetAll());

        // Si la base de données est vide, ajouter des exemples
        if (Passwords.Count == 0)
        {
            var example1 = new PasswordEntry { Site = "example.com", Username = "user1", Password = "pass1", Url = "http://example.com" };
            var example2 = new PasswordEntry { Site = "google.com", Username = "user2", Password = "pass2", Url = "http://google.com" };
            
            _dbService.Add(example1);
            _dbService.Add(example2);
            
            // Recharger les données depuis la base
            Passwords.Clear();
            foreach (var entry in _dbService.GetAll())
            {
                Passwords.Add(entry);
            }
        }

        CopyPasswordCommand = new Commands.RelayCommand<string>(CopyToClipboard);
        CopyUrlCommand = new Commands.RelayCommand<string>(CopyUrlToClipboard);
        DeletePasswordCommand = new Commands.RelayCommand<PasswordEntry>(DeletePassword);
        SearchCommand = new Commands.RelayCommand<string>(PerformSearch);
        AddPasswordCommand = new Commands.RelayCommand<object>(_ => {
            System.Diagnostics.Debug.WriteLine("[DEBUG] AddPasswordCommand exécutée");
            ShowAddPasswordDialog();
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