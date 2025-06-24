using Avalonia.Controls;
using Avalonia.Diagnostics;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using YNOV_Password.Models;
using YNOV_Password.ViewModels;

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
            
            this.PointerPressed += MainWindow_PointerPressed;
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
    }
}