using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using YNOV_Password.Models;
using YNOV_Password.Services;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace YNOV_Password.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<PasswordEntry> Passwords { get; set; }
    private readonly PasswordDatabaseService _dbService;

    public ICommand CopyPasswordCommand { get; }

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

        CopyPasswordCommand = new RelayCommand<string>(CopyToClipboard);
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
}