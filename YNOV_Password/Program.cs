using Avalonia;
using System;
using YNOV_Password.Services;

namespace YNOV_Password;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Initialiser le service de logging et nettoyer les anciens logs
        LoggingService.LogInfo("Démarrage de l'application YNOV Password");
        LoggingService.LogSystemInfo(); // Log des informations système
        LoggingService.CleanOldLogs();
        
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            LoggingService.LogError(ex, "Erreur fatale au démarrage de l'application");
            throw;
        }
        finally
        {
            LoggingService.LogInfo("Arrêt de l'application YNOV Password");
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}