using Avalonia.Controls;
using Avalonia.Diagnostics;
using Avalonia.Input;
using Avalonia.Interactivity;
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
            // Forcer le DataContext au cas où il ne serait pas défini
            if (DataContext == null)
            {
                DataContext = new MainWindowViewModel();
                System.Diagnostics.Debug.WriteLine("[DEBUG] DataContext forcé dans MainWindow constructor");
            }
            System.Diagnostics.Debug.WriteLine($"[DEBUG] DataContext type: {DataContext?.GetType().Name}");
            
            // Ajouter un gestionnaire d'événement global pour masquer les mots de passe quand on clique ailleurs
            this.PointerPressed += MainWindow_PointerPressed;
        }

        private void MainWindow_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // Masquer tous les mots de passe visibles quand on clique ailleurs dans la fenêtre
            if (DataContext is MainWindowViewModel viewModel)
            {
                // Vérifier si le clic n'est pas sur un bouton de mot de passe ou un menu
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

        // Nous utilisons le binding avec AddPasswordCommand, pas besoin de gestion d'événement supplémentaire
        
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
    }
}