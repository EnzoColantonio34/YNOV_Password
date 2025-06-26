using Avalonia.Controls;
using Avalonia.Diagnostics;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using YNOV_Password.Models;
using YNOV_Password.ViewModels;
using YNOV_Password.Services;
using Avalonia.Platform.Storage;

namespace YNOV_Password.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (DataContext == null)
            {
                DataContext = new MainWindowViewModel();
                System.Diagnostics.Debug.WriteLine("[DEBUG] DataContext forcé dans MainWindow constructor");
            }
            System.Diagnostics.Debug.WriteLine($"[DEBUG] DataContext type: {DataContext?.GetType().Name}");
            
            // S'abonner à l'événement de déconnexion
            SetupLogoutEvent();
            
            // Écouter les changements de DataContext
            this.DataContextChanged += (s, e) => SetupLogoutEvent();
            
            this.PointerPressed += MainWindow_PointerPressed;
        }

        private void SetupLogoutEvent()
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.LogoutRequested += HandleLogoutRequest;
                System.Diagnostics.Debug.WriteLine("[DEBUG] LogoutRequested event attached");
            }
        }

        private void MainWindow_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                var hitTest = e.Source;
                if (hitTest is not Button button || button.DataContext is not PasswordEntry)
                {
                    foreach (var entry in viewModel.Passwords)
                    {
                        if (entry.IsPasswordVisible)
                        {
                            viewModel.HidePassword(entry);
                        }
                    }
                }
            }
        }

        private async void AddButton_Click(object? sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] AddButton_Click appelé");
            if (DataContext is MainWindowViewModel viewModel)
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] DataContext est bien MainWindowViewModel");
                // Appeler directement la méthode
                var mainWindow = this;
                var dialog = new AddPasswordWindow(viewModel);
                await dialog.ShowDialog(mainWindow);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] DataContext n'est pas MainWindowViewModel: {DataContext?.GetType().Name}");
            }
        }
        
        private void CopyPassword_Click(object? sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] CopyPassword_Click appelé");
            if (sender is MenuItem menuItem && menuItem.Tag is PasswordEntry entry && DataContext is MainWindowViewModel viewModel)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Copie du mot de passe pour: {entry.Site}");
                viewModel.CopyPasswordCommand.Execute(entry.Password);
            }
        }

        private void CopyUrl_Click(object? sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] CopyUrl_Click appelé");
            if (sender is MenuItem menuItem && menuItem.Tag is PasswordEntry entry && DataContext is MainWindowViewModel viewModel)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Copie de l'URL pour: {entry.Site}");
                viewModel.CopyUrlCommand.Execute(entry.Url);
            }
        }

        private void CopyUsername_Click(object? sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] CopyUsername_Click appelé");
            if (sender is MenuItem menuItem && menuItem.Tag is PasswordEntry entry && DataContext is MainWindowViewModel viewModel)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Copie du nom d'utilisateur pour: {entry.Site}");
                viewModel.CopyUsernameCommand.Execute(entry.Username);
            }
        }

        private void OpenUrl_Click(object? sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] OpenUrl_Click appelé");
            if (sender is Button button && button.Tag is PasswordEntry entry && DataContext is MainWindowViewModel viewModel)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Ouverture de l'URL pour: {entry.Site} - {entry.Url}");
                viewModel.OpenUrlCommand.Execute(entry.Url);
            }
        }

        private void DeletePassword_Click(object? sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] DeletePassword_Click appelé");
            if (sender is MenuItem menuItem && menuItem.Tag is PasswordEntry entry && DataContext is MainWindowViewModel viewModel)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Suppression de l'entrée: {entry.Site}");
                viewModel.DeletePasswordCommand.Execute(entry);
            }
        }

        private void ShowPassword_Click(object? sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] ShowPassword_Click appelé");
            if (sender is MenuItem menuItem && menuItem.Tag is PasswordEntry entry && DataContext is MainWindowViewModel viewModel)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Affichage du mot de passe pour: {entry.Site}");
                viewModel.ShowPasswordCommand.Execute(entry);
            }
        }

        private void PasswordField_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            // Masquer tous les mots de passe visibles lorsqu'on clique ailleurs
            if (DataContext is MainWindowViewModel viewModel && sender is Button button && button.DataContext is PasswordEntry clickedEntry)
            {
                // Masquer tous les autres mots de passe sauf celui sur lequel on clique
                foreach (var entry in viewModel.Passwords)
                {
                    if (entry != clickedEntry && entry.IsPasswordVisible)
                    {
                        viewModel.HidePassword(entry);
                    }
                }
            }
        }

        private async void GeneratePasswordButton_Click(object? sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] GeneratePasswordButton_Click appelé");
            
            var passwordGeneratorWindow = new PasswordGeneratorWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            try
            {
                await passwordGeneratorWindow.ShowDialog(this);
                
                if (passwordGeneratorWindow.WasPasswordSelected && 
                    !string.IsNullOrEmpty(passwordGeneratorWindow.GeneratedPassword))
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Mot de passe généré sélectionné: {passwordGeneratorWindow.GeneratedPassword}");
                    
                    // Copier le mot de passe généré dans le presse-papiers
                    var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                    if (clipboard != null)
                    {
                        await clipboard.SetTextAsync(passwordGeneratorWindow.GeneratedPassword);
                        System.Diagnostics.Debug.WriteLine("[DEBUG] Mot de passe copié dans le presse-papiers");
                    }
                    
                    // Optionnel : Ouvrir directement la fenêtre d'ajout avec le mot de passe pré-rempli
                    if (DataContext is MainWindowViewModel viewModel)
                    {
                        await viewModel.ShowAddPasswordDialogWithGeneratedPassword(passwordGeneratorWindow.GeneratedPassword);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Erreur lors de l'ouverture de la fenêtre de génération: {ex.Message}");
            }
        }

        private void UserMenuButton_Click(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("[DEBUG] UserMenuButton_Click appelé - Menu utilisateur ouvert");
        }

        private async void LogoutMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("[DEBUG] LogoutMenuItem_Click appelé directement");
            await PerformLogout();
        }

        private async void HandleLogoutRequest()
        {
            await PerformLogout();
        }

        private async System.Threading.Tasks.Task PerformLogout()
        {
            Console.WriteLine("[DEBUG] PerformLogout appelé");
            
            try
            {
                // Confirmer la déconnexion
                var result = await ShowConfirmationDialog("Êtes-vous sûr de vouloir vous déconnecter ?");

                if (result)
                {
                    Console.WriteLine("[DEBUG] Déconnexion confirmée");
                    
                    // Fermer la fenêtre actuelle et retourner au login
                    var app = Avalonia.Application.Current;
                    if (app?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        // Fermer la fenêtre principale actuelle
                        this.Close();
                        
                        // Ouvrir la fenêtre de login
                        var loginWindow = new LoginWindow();
                        var viewModel = loginWindow.GetViewModel();
                        
                        if (viewModel != null)
                        {
                            // S'abonner à l'événement de completion du login
                            viewModel.LoginCompleted += () =>
                            {
                                if (viewModel.LoggedInUser != null)
                                {
                                    // Login réussi, ouvrir une nouvelle fenêtre principale
                                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                                    {
                                        desktop.MainWindow = new MainWindow
                                        {
                                            DataContext = new MainWindowViewModel(viewModel.LoggedInUser.Id),
                                        };
                                        desktop.MainWindow.Show();
                                    });
                                }
                            };

                            // Gérer la fermeture de la fenêtre de login
                            loginWindow.Closed += (s, args) =>
                            {
                                if (!viewModel.LoginSuccessful)
                                {
                                    // Login annulé, fermer l'application
                                    desktop.Shutdown();
                                }
                            };
                        }
                        
                        // Afficher la fenêtre de login
                        desktop.MainWindow = loginWindow;
                        loginWindow.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Erreur lors de la déconnexion: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task<bool> ShowConfirmationDialog(string message)
        {
            try
            {
                var dialog = new Window
                {
                    Title = "Confirmation",
                    Width = 400,
                    Height = 180,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    CanResize = false,
                    Background = Avalonia.Media.Brushes.White,
                    ExtendClientAreaToDecorationsHint = false
                };

                bool result = false;
                var yesButton = new Button 
                { 
                    Content = "Se déconnecter", 
                    Width = 140,
                    Height = 35,
                    Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#e74c3c")),
                    Foreground = Avalonia.Media.Brushes.White,
                    CornerRadius = new Avalonia.CornerRadius(4),
                    FontWeight = Avalonia.Media.FontWeight.Medium,
                    Margin = new Avalonia.Thickness(5),
                    Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
                };
                
                var cancelButton = new Button 
                { 
                    Content = "Annuler", 
                    Width = 100,
                    Height = 35,
                    Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#059669")),
                    Foreground = Avalonia.Media.Brushes.White,
                    CornerRadius = new Avalonia.CornerRadius(4),
                    FontWeight = Avalonia.Media.FontWeight.Medium,
                    Margin = new Avalonia.Thickness(5),
                    Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
                };

                yesButton.Click += (s, e) => { result = true; dialog.Close(); };
                cancelButton.Click += (s, e) => { result = false; dialog.Close(); };

                var content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(30),
                    Spacing = 20,
                    Children =
                    {
                        new StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            Spacing = 15,
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = "⚠️",
                                    FontSize = 32,
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#f39c12"))
                                },
                                new TextBlock 
                                { 
                                    Text = message,
                                    FontSize = 14,
                                    FontWeight = Avalonia.Media.FontWeight.Medium,
                                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#2c3e50")),
                                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                                    MaxWidth = 280
                                }
                            }
                        },
                        new StackPanel
                        {
                            Orientation = Avalonia.Layout.Orientation.Horizontal,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            Spacing = 15,
                            Children = { cancelButton, yesButton }
                        }
                    }
                };

                dialog.Content = content;
                await dialog.ShowDialog(this);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Erreur dans ShowConfirmationDialog: {ex.Message}");
                return false;
            }
        }

        private async void ImportLibraryMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("[DEBUG] ImportLibraryMenuItem_Click appelé");
            
            try
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel == null) return;

                var filePickerOptions = new FilePickerOpenOptions
                {
                    Title = "Sélectionner un fichier de bibliothèque de mots",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("Fichiers texte")
                        {
                            Patterns = new[] { "*.txt" }
                        },
                        new FilePickerFileType("Tous les fichiers")
                        {
                            Patterns = new[] { "*.*" }
                        }
                    }
                };

                var result = await topLevel.StorageProvider.OpenFilePickerAsync(filePickerOptions);
                
                if (result.Count > 0)
                {
                    var file = result[0];
                    var filePath = file.Path.LocalPath;
                    Console.WriteLine($"[DEBUG] Fichier sélectionné: {filePath}");
                    
                    var wordLibraryService = WordLibraryService.Instance;
                    var success = await wordLibraryService.LoadWordsFromFileAsync(filePath);
                    
                    if (success)
                    {
                        await ShowInfoDialog($"Bibliothèque importée avec succès!\n{wordLibraryService.Words.Count} mots chargés.");
                        Console.WriteLine($"[DEBUG] {wordLibraryService.Words.Count} mots chargés");
                    }
                    else
                    {
                        await ShowErrorDialog("Erreur lors de l'importation de la bibliothèque.\nVérifiez que le fichier est valide.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Erreur lors de l'importation: {ex.Message}");
                await ShowErrorDialog($"Erreur lors de l'importation: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task ShowInfoDialog(string message)
        {
            var dialog = new Window
            {
                Title = "Information",
                Width = 400,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                Background = Avalonia.Media.Brushes.White,
                ExtendClientAreaToDecorationsHint = false
            };

            var okButton = new Button 
            { 
                Content = "OK", 
                Width = 100,
                Height = 35,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#34D399")),
                Foreground = Avalonia.Media.Brushes.White,
                CornerRadius = new Avalonia.CornerRadius(4),
                FontWeight = Avalonia.Media.FontWeight.Medium,
                Margin = new Avalonia.Thickness(5),
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
            };

            okButton.Click += (s, e) => dialog.Close();

            var content = new StackPanel
            {
                Margin = new Avalonia.Thickness(30),
                Spacing = 20,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        Spacing = 15,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "ℹ️",
                                FontSize = 32,
                                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#3498db"))
                            },
                            new TextBlock 
                            { 
                                Text = message,
                                FontSize = 14,
                                FontWeight = Avalonia.Media.FontWeight.Medium,
                                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#2c3e50")),
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                                MaxWidth = 280
                            }
                        }
                    },
                    new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        Children = { okButton }
                    }
                }
            };

            dialog.Content = content;
            await dialog.ShowDialog(this);
        }

        private async System.Threading.Tasks.Task ShowErrorDialog(string message)
        {
            var dialog = new Window
            {
                Title = "Erreur",
                Width = 400,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                Background = Avalonia.Media.Brushes.White,
                ExtendClientAreaToDecorationsHint = false
            };

            var okButton = new Button 
            { 
                Content = "OK", 
                Width = 100,
                Height = 35,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#e74c3c")),
                Foreground = Avalonia.Media.Brushes.White,
                CornerRadius = new Avalonia.CornerRadius(4),
                FontWeight = Avalonia.Media.FontWeight.Medium,
                Margin = new Avalonia.Thickness(5),
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
            };

            okButton.Click += (s, e) => dialog.Close();

            var content = new StackPanel
            {
                Margin = new Avalonia.Thickness(30),
                Spacing = 20,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        Spacing = 15,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "❌",
                                FontSize = 32,
                                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#e74c3c"))
                            },
                            new TextBlock 
                            { 
                                Text = message,
                                FontSize = 14,
                                FontWeight = Avalonia.Media.FontWeight.Medium,
                                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#2c3e50")),
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                                MaxWidth = 280
                            }
                        }
                    },
                    new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        Children = { okButton }
                    }
                }
            };

            dialog.Content = content;
            await dialog.ShowDialog(this);
        }
    }
}