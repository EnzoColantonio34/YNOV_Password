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

    public ICommand CopyPasswordCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand AddPasswordCommand { get; }

    public MainWindowViewModel()
    {
        _dbService = new PasswordDatabaseService();
        Passwords = new ObservableCollection<PasswordEntry>(_dbService.GetAll());

        // Si la base de données est vide, ajouter des exemples
        if (Passwords.Count == 0)
        {
            Passwords.Add(new PasswordEntry { Site = "example.com", Username = "user1", Password = "pass1", Url = "http://example.com" });
            Passwords.Add(new PasswordEntry { Site = "google.com", Username = "user2", Password = "pass2", Url = "http://google.com" });
        }

        CopyPasswordCommand = new Commands.RelayCommand<string>(CopyToClipboard);
        SearchCommand = new Commands.RelayCommand<string>(PerformSearch);
        AddPasswordCommand = new Commands.RelayCommand<object>(_ => {
            System.Diagnostics.Debug.WriteLine("[DEBUG] AddPasswordCommand exécutée");
            ShowAddPasswordDialog();
        });
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