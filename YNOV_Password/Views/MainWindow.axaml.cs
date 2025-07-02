using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
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
            }
            
            SetupLogoutEvent();
            this.DataContextChanged += (s, e) => SetupLogoutEvent();
            this.PointerPressed += MainWindow_PointerPressed;
        }

        private void InitializeComponent()
        {
            Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
        }

        private void SetupLogoutEvent()
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.LogoutRequested += HandleLogoutRequest;
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
            if (DataContext is MainWindowViewModel viewModel)
            {
                var dialog = new AddPasswordWindow(viewModel);
                await dialog.ShowDialog(this);
            }
        }
        
        private void CopyPassword_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is PasswordEntry entry && DataContext is MainWindowViewModel viewModel)
            {
                viewModel.CopyPasswordCommand.Execute(entry.Password);
            }
        }

        private void CopyUrl_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is PasswordEntry entry && DataContext is MainWindowViewModel viewModel)
            {
                viewModel.CopyUrlCommand.Execute(entry.Url);
            }
        }

        private void CopyUsername_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is PasswordEntry entry && DataContext is MainWindowViewModel viewModel)
            {
                viewModel.CopyUsernameCommand.Execute(entry.Username);
            }
        }

        private void OpenUrl_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PasswordEntry entry && DataContext is MainWindowViewModel viewModel)
            {
                viewModel.OpenUrlCommand.Execute(entry.Url);
            }
        }

        private void DeletePassword_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is PasswordEntry entry && DataContext is MainWindowViewModel viewModel)
            {
                viewModel.DeletePasswordCommand.Execute(entry);
            }
        }

        private void ShowPassword_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is PasswordEntry entry && DataContext is MainWindowViewModel viewModel)
            {
                viewModel.ShowPasswordCommand.Execute(entry);
            }
        }

        private async void ModifyPassword_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is PasswordEntry entry && DataContext is MainWindowViewModel viewModel)
            {
                var dialog = new AddPasswordWindow(viewModel, entry);
                await dialog.ShowDialog(this);
            }
        }

        private async void DuplicateAlert_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PasswordEntry entry && DataContext is MainWindowViewModel viewModel && viewModel.CurrentUser != null)
            {
                var alertWindow = new DuplicatePasswordAlertWindow(entry, viewModel.CurrentUser)
                {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                // Gérer les événements de la fenêtre d'alerte
                alertWindow.ModifyPasswordRequested += async (passwordEntry) =>
                {
                    // Ouvrir la fenêtre de modification de mot de passe
                    var addPasswordWindow = new AddPasswordWindow(viewModel, passwordEntry)
                    {
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    await addPasswordWindow.ShowDialog(this);
                };

                alertWindow.GeneratePasswordRequested += async (passwordEntry) =>
                {
                    // Ouvrir le générateur de mot de passe et l'appliquer au site
                    var passwordGeneratorWindow = new PasswordGeneratorWindow
                    {
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    
                    await passwordGeneratorWindow.ShowDialog(this);
                    
                    if (passwordGeneratorWindow.WasPasswordSelected && 
                        !string.IsNullOrEmpty(passwordGeneratorWindow.GeneratedPassword))
                    {
                        // Mettre à jour le mot de passe de l'entrée
                        passwordEntry.Password = passwordGeneratorWindow.GeneratedPassword;
                        viewModel.UpdatePassword(passwordEntry);
                    }
                };

                await alertWindow.ShowDialog(this);
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
                    var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                    if (clipboard != null)
                    {
                        await clipboard.SetTextAsync(passwordGeneratorWindow.GeneratedPassword);
                    }
                    
                    if (DataContext is MainWindowViewModel viewModel)
                    {
                        await viewModel.ShowAddPasswordDialogWithGeneratedPassword(passwordGeneratorWindow.GeneratedPassword);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, "Ouverture de la fenêtre de génération de mot de passe");
            }
        }

        private async void HelpButton_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var helpWindow = new HelpWindow
                {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                await helpWindow.ShowDialog(this);
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, "Ouverture de la fenêtre d'aide");
            }
        }

        private void UserMenuButton_Click(object? sender, RoutedEventArgs e)
        {
        }

        private async void LogoutMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            await PerformLogout();
        }

        private async void HandleLogoutRequest()
        {
            await PerformLogout();
        }

        private async System.Threading.Tasks.Task PerformLogout()
        {
            try
            {
                var result = await ShowConfirmationDialog("Êtes-vous sûr de vouloir vous déconnecter ?");

                if (result)
                {
                    var app = Avalonia.Application.Current;
                    if (app?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        this.Close();
                        
                        var loginWindow = new LoginWindow();
                        var viewModel = loginWindow.GetViewModel();
                        
                        if (viewModel != null)
                        {
                            viewModel.LoginCompleted += () =>
                            {
                                if (viewModel.LoggedInUser != null)
                                {
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

                            loginWindow.Closed += (s, args) =>
                            {
                                if (!viewModel.LoginSuccessful)
                                {
                                    desktop.Shutdown();
                                }
                            };
                        }
                        
                        desktop.MainWindow = loginWindow;
                        loginWindow.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, "Déconnexion de l'utilisateur");
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
                LoggingService.LogError(ex, "Affichage de la boîte de dialogue de confirmation");
                return false;
            }
        }

        private async void ImportLibraryMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            
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
                    
                    var wordLibraryService = WordLibraryService.Instance;
                    var success = await wordLibraryService.LoadWordsFromFileAsync(filePath);
                    
                    if (success)
                    {
                        await ShowInfoDialog($"Bibliothèque importée avec succès!\n{wordLibraryService.Words.Count} mots chargés.");
                    }
                    else
                    {
                        await ShowErrorDialog("Erreur lors de l'importation de la bibliothèque.\nVérifiez que le fichier est valide.");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, "Importation de la bibliothèque de mots");
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