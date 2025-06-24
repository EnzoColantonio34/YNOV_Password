using Avalonia.Controls;
using Avalonia.Diagnostics;
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
    }
}