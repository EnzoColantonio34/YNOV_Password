using System;
using System.Windows.Input;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using YNOV_Password.Commands;
using YNOV_Password.Models;
using YNOV_Password.Services;

namespace YNOV_Password.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly UserDatabaseService _userService;
        
        [ObservableProperty]
        private string _email = string.Empty;
        
        [ObservableProperty]
        private string _password = string.Empty;
        
        [ObservableProperty]
        private string _username = string.Empty;
        
        [ObservableProperty]
        private string _confirmPassword = string.Empty;
        
        [ObservableProperty]
        private bool _isRegistering = false;
        
        [ObservableProperty]
        private string _errorMessage = string.Empty;
        
        [ObservableProperty]
        private string _successMessage = string.Empty;
        
        public ICommand LoginCommand { get; private set; }
        public ICommand RegisterCommand { get; private set; }
        public ICommand SwitchToRegisterCommand { get; private set; }
        public ICommand SwitchToLoginCommand { get; private set; }
        
        public User? LoggedInUser { get; private set; }
        public bool LoginSuccessful { get; private set; }
        
        public event System.Action? LoginCompleted;

        public LoginViewModel()
        {
            _userService = new UserDatabaseService();
            
            LoginCommand = new RelayCommand<object>(_ => PerformLogin());
            RegisterCommand = new RelayCommand<object>(_ => PerformRegister());
            SwitchToRegisterCommand = new RelayCommand<object>(_ => SwitchToRegister());
            SwitchToLoginCommand = new RelayCommand<object>(_ => SwitchToLogin());
        }

        private void PerformLogin()
        {
            try
            {
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;
                
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Veuillez remplir tous les champs.";
                    LoggingService.LogInfo("Tentative de connexion avec des champs vides");
                    return;
                }

                if (!IsValidEmail(Email))
                {
                    ErrorMessage = "L'adresse email entrée ne respecte pas le format valide (exemple: nom@domaine.com).";
                    LoggingService.LogInfo($"Tentative de connexion avec un email invalide : {Email}");
                    return;
                }

                LoggingService.LogInfo($"Tentative de connexion pour l'email : {Email}");
                try
                {
                    var user = _userService.Login(Email, Password);
                    
                    if (user != null)
                    {
                        LoggingService.LogInfo($"Connexion réussie pour l'utilisateur : {user.Username}");
                        LoggedInUser = user;
                        LoginSuccessful = true;
                        LoginCompleted?.Invoke();
                    }
                    else
                    {
                        LoggingService.LogInfo("Échec de la connexion : identifiants incorrects");
                        ErrorMessage = "Email ou mot de passe incorrect.";
                    }
                }
                catch (Exception loginEx)
                {
                    LoggingService.LogError(loginEx, "Erreur lors de la tentative de connexion");
                    ErrorMessage = "Une erreur s'est produite lors de la connexion. Vérifiez les logs pour plus de détails.";
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, "Erreur inattendue lors de la connexion");
                ErrorMessage = $"Erreur inattendue : {ex.Message}";
            }
        }

        private void PerformRegister()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Veuillez remplir tous les champs.";
                return;
            }
            
            if (!IsValidEmail(Email))
            {
                ErrorMessage = "L'adresse email entrée ne respecte pas le format valide (exemple: nom@domaine.com).";
                return;
            }
            
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Les mots de passe ne correspondent pas.";
                return;
            }
            
            if (Password.Length < 6)
            {
                ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères.";
                return;
            }

            bool success = _userService.Register(Username, Email, Password);
            if (success)
            {
                SwitchToLogin();
                SuccessMessage = "Inscription réussie ! Vous pouvez maintenant vous connecter.";
            }
            else
            {
                ErrorMessage = "Erreur lors de l'inscription. Cet email existe peut-être déjà.";
            }
        }

        private void SwitchToRegister()
        {
            IsRegistering = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            ClearFields();
        }

        private void SwitchToLogin()
        {
            IsRegistering = false;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            ClearFields();
        }

        private void ClearFields()
        {
            Email = string.Empty;
            Password = string.Empty;
            Username = string.Empty;
            ConfirmPassword = string.Empty;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Pattern regex pour valider le format email
                string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                return Regex.IsMatch(email, pattern);
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, $"Validation de l'email '{email}'");
                return false;
            }
        }
    }
}
