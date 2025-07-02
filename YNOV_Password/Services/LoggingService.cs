using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace YNOV_Password.Services
{
    public static class LoggingService
    {
        private static readonly string LogDirectory = GetPlatformSpecificLogDirectory();

        /// <summary>
        /// Détermine le répertoire de logs en fonction de l'OS
        /// </summary>
        /// <returns>Le chemin du répertoire de logs approprié pour l'OS</returns>
        private static string GetPlatformSpecificLogDirectory()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // macOS: /Users/<user>/Library/Application Support/YNOV_Password/Logs/
                var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return Path.Combine(userProfile, "Library", "Application Support", "YNOV_Password", "Logs");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows: C:\Users\NomUtilisateur\AppData\Local\YNOV_Password\Logs\
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YNOV_Password", "Logs");
            }
            else
            {
                // Linux ou autre: utilise LocalApplicationData par défaut
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YNOV_Password", "Logs");
            }
        }
        private static readonly string LogFileName = $"app_log_{DateTime.Now:yyyy-MM-dd}.txt";
        private static readonly string LogFilePath = Path.Combine(LogDirectory, LogFileName);

        static LoggingService()
        {
            // Créer le répertoire de logs s'il n'existe pas
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
        }

        /// <summary>
        /// Exécute une action et gère automatiquement les exceptions avec logging
        /// </summary>
        /// <param name="action">L'action à exécuter</param>
        /// <param name="operationName">Nom de l'opération pour le log</param>
        /// <param name="onError">Action optionnelle à exécuter en cas d'erreur</param>
        /// <returns>True si l'opération s'est bien déroulée, False sinon</returns>
        public static bool ExecuteWithLogging(Action action, string operationName, Action<Exception>? onError = null)
        {
            try
            {
                LogInfo($"Début de l'opération: {operationName}");
                action.Invoke();
                LogInfo($"Opération réussie: {operationName}");
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, operationName);
                onError?.Invoke(ex);
                return false;
            }
        }

        /// <summary>
        /// Exécute une fonction et gère automatiquement les exceptions avec logging
        /// </summary>
        /// <typeparam name="T">Type de retour de la fonction</typeparam>
        /// <param name="func">La fonction à exécuter</param>
        /// <param name="operationName">Nom de l'opération pour le log</param>
        /// <param name="defaultValue">Valeur par défaut à retourner en cas d'erreur</param>
        /// <param name="onError">Action optionnelle à exécuter en cas d'erreur</param>
        /// <returns>Le résultat de la fonction ou la valeur par défaut en cas d'erreur</returns>
        public static T ExecuteWithLogging<T>(Func<T> func, string operationName, T defaultValue = default!, Action<Exception>? onError = null)
        {
            try
            {
                LogInfo($"Début de l'opération: {operationName}");
                var result = func.Invoke();
                LogInfo($"Opération réussie: {operationName}");
                return result;
            }
            catch (Exception ex)
            {
                LogError(ex, operationName);
                onError?.Invoke(ex);
                return defaultValue;
            }
        }

        /// <summary>
        /// Exécute une action asynchrone et gère automatiquement les exceptions avec logging
        /// </summary>
        /// <param name="task">La tâche à exécuter</param>
        /// <param name="operationName">Nom de l'opération pour le log</param>
        /// <param name="onError">Action optionnelle à exécuter en cas d'erreur</param>
        /// <returns>True si l'opération s'est bien déroulée, False sinon</returns>
        public static async Task<bool> ExecuteWithLoggingAsync(Func<Task> task, string operationName, Action<Exception>? onError = null)
        {
            try
            {
                LogInfo($"Début de l'opération asynchrone: {operationName}");
                await task.Invoke();
                LogInfo($"Opération asynchrone réussie: {operationName}");
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, operationName);
                onError?.Invoke(ex);
                return false;
            }
        }

        /// <summary>
        /// Exécute une fonction asynchrone et gère automatiquement les exceptions avec logging
        /// </summary>
        /// <typeparam name="T">Type de retour de la fonction</typeparam>
        /// <param name="task">La tâche à exécuter</param>
        /// <param name="operationName">Nom de l'opération pour le log</param>
        /// <param name="defaultValue">Valeur par défaut à retourner en cas d'erreur</param>
        /// <param name="onError">Action optionnelle à exécuter en cas d'erreur</param>
        /// <returns>Le résultat de la fonction ou la valeur par défaut en cas d'erreur</returns>
        public static async Task<T> ExecuteWithLoggingAsync<T>(Func<Task<T>> task, string operationName, T defaultValue = default!, Action<Exception>? onError = null)
        {
            try
            {
                LogInfo($"Début de l'opération asynchrone: {operationName}");
                var result = await task.Invoke();
                LogInfo($"Opération asynchrone réussie: {operationName}");
                return result;
            }
            catch (Exception ex)
            {
                LogError(ex, operationName);
                onError?.Invoke(ex);
                return defaultValue;
            }
        }

        /// <summary>
        /// Enregistre une information dans le fichier de log
        /// </summary>
        /// <param name="message">Message à enregistrer</param>
        public static void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        /// <summary>
        /// Enregistre un avertissement dans le fichier de log
        /// </summary>
        /// <param name="message">Message à enregistrer</param>
        public static void LogWarning(string message)
        {
            WriteLog("WARNING", message);
        }

        /// <summary>
        /// Enregistre une erreur dans le fichier de log
        /// </summary>
        /// <param name="exception">Exception à enregistrer</param>
        /// <param name="operationName">Nom de l'opération qui a causé l'erreur</param>
        public static void LogError(Exception exception, string operationName = "")
        {
            var message = string.IsNullOrEmpty(operationName) 
                ? $"Exception: {exception.Message}\nStack Trace: {exception.StackTrace}"
                : $"Erreur dans l'opération '{operationName}': {exception.Message}\nStack Trace: {exception.StackTrace}";
                
            WriteLog("ERROR", message);
        }

        /// <summary>
        /// Enregistre une erreur personnalisée dans le fichier de log
        /// </summary>
        /// <param name="message">Message d'erreur à enregistrer</param>
        public static void LogError(string message)
        {
            WriteLog("ERROR", message);
        }

        /// <summary>
        /// Écrit un message dans le fichier de log
        /// </summary>
        /// <param name="level">Niveau de log (INFO, WARNING, ERROR)</param>
        /// <param name="message">Message à enregistrer</param>
        private static void WriteLog(string level, string message)
        {
            try
            {
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}{Environment.NewLine}";
                File.AppendAllText(LogFilePath, logEntry);
            }
            catch (Exception ex)
            {
                // En cas d'erreur lors de l'écriture du log, on peut essayer d'écrire dans la console
                Console.WriteLine($"Erreur lors de l'écriture du log: {ex.Message}");
                Console.WriteLine($"Message original: [{level}] {message}");
            }
        }

        /// <summary>
        /// Nettoie les anciens fichiers de log (garde seulement les 30 derniers jours)
        /// </summary>
        public static void CleanOldLogs()
        {
            ExecuteWithLogging(() =>
            {
                if (!Directory.Exists(LogDirectory))
                    return;

                var files = Directory.GetFiles(LogDirectory, "app_log_*.txt");
                var cutoffDate = DateTime.Now.AddDays(-30);

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                        LogInfo($"Fichier de log supprimé: {fileInfo.Name}");
                    }
                }
            }, "Nettoyage des anciens logs");
        }

        /// <summary>
        /// Retourne le chemin du fichier de log actuel
        /// </summary>
        /// <returns>Chemin du fichier de log</returns>
        public static string GetLogFilePath()
        {
            return LogFilePath;
        }

        /// <summary>
        /// Retourne le répertoire des logs
        /// </summary>
        /// <returns>Répertoire des logs</returns>
        public static string GetLogDirectory()
        {
            return LogDirectory;
        }

        /// <summary>
        /// Wrapper spécialisé pour les actions UI qui ne doivent pas bloquer l'interface
        /// </summary>
        /// <param name="action">L'action UI à exécuter</param>
        /// <param name="operationName">Nom de l'opération pour le log</param>
        /// <param name="showErrorToUser">Action optionnelle pour afficher l'erreur à l'utilisateur</param>
        public static void ExecuteUIAction(Action action, string operationName, Action<string>? showErrorToUser = null)
        {
            try
            {
                LogInfo($"Début de l'action UI: {operationName}");
                action.Invoke();
                LogInfo($"Action UI réussie: {operationName}");
            }
            catch (Exception ex)
            {
                LogError(ex, operationName);
                showErrorToUser?.Invoke($"Erreur: {ex.Message}");
            }
        }

        /// <summary>
        /// Wrapper spécialisé pour les fonctions UI avec valeur de retour
        /// </summary>
        /// <typeparam name="T">Type de retour de la fonction</typeparam>
        /// <param name="func">La fonction UI à exécuter</param>
        /// <param name="operationName">Nom de l'opération pour le log</param>
        /// <param name="defaultValue">Valeur par défaut à retourner en cas d'erreur</param>
        /// <param name="showErrorToUser">Action optionnelle pour afficher l'erreur à l'utilisateur</param>
        /// <returns>Le résultat de la fonction ou la valeur par défaut en cas d'erreur</returns>
        public static T ExecuteUIFunction<T>(Func<T> func, string operationName, T defaultValue = default!, Action<string>? showErrorToUser = null)
        {
            try
            {
                LogInfo($"Début de la fonction UI: {operationName}");
                var result = func.Invoke();
                LogInfo($"Fonction UI réussie: {operationName}");
                return result;
            }
            catch (Exception ex)
            {
                LogError(ex, operationName);
                showErrorToUser?.Invoke($"Erreur: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Log les informations sur l'OS détecté et le répertoire de logs utilisé
        /// </summary>
        public static void LogSystemInfo()
        {
            var osInfo = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "macOS"
                       : RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows"
                       : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux"
                       : "Autre";
            
            LogInfo($"Système d'exploitation détecté: {osInfo}");
            LogInfo($"Répertoire de logs: {LogDirectory}");
            LogInfo($"Architecture: {RuntimeInformation.OSArchitecture}");
            LogInfo($"Framework: {RuntimeInformation.FrameworkDescription}");
        }
    }
}
