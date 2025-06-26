using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using YNOV_Password.ViewModels;
using YNOV_Password.Views;

namespace YNOV_Password;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            
            // Afficher la fenêtre de login en premier
            ShowLoginWindow(desktop);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ShowLoginWindow(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var loginWindow = new LoginWindow();
        var viewModel = loginWindow.GetViewModel();
        
        if (viewModel == null)
        {
            desktop.Shutdown();
            return;
        }

        // S'abonner à l'événement de completion du login
        viewModel.LoginCompleted += () =>
        {
            if (viewModel.LoggedInUser != null)
            {
                // Login réussi, ouvrir la fenêtre principale avec l'utilisateur connecté
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
        loginWindow.Closed += (s, e) =>
        {
            if (!viewModel.LoginSuccessful)
            {
                // Login annulé ou échoué, fermer l'application
                desktop.Shutdown();
            }
        };
        
        // Afficher la fenêtre de login comme fenêtre principale
        desktop.MainWindow = loginWindow;
        loginWindow.Show();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}