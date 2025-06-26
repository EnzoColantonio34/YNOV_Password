using Avalonia.Controls;
using YNOV_Password.ViewModels;

namespace YNOV_Password.Views
{
    public partial class LoginWindow : Window
    {
        private LoginViewModel? _viewModel;

        public LoginWindow()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();
            DataContext = _viewModel;
            
            // Setup UI based on current mode
            UpdateUI();
            
            // Subscribe to property changes
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(LoginViewModel.IsRegistering))
                {
                    UpdateUI();
                }
                else if (e.PropertyName == nameof(LoginViewModel.ErrorMessage))
                {
                    UpdateErrorMessage();
                }
            };
            
            // Subscribe to login completion
            _viewModel.LoginCompleted += () =>
            {
                // Fermer la fenêtre après un court délai pour permettre la transition
                Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
                {
                    await System.Threading.Tasks.Task.Delay(500);
                    Close();
                });
            };
        }

        private void UpdateUI()
        {
            if (_viewModel == null) return;

            var modeText = this.FindControl<TextBlock>("ModeText");
            var switchButton = this.FindControl<Button>("SwitchButton");
            var loginButton = this.FindControl<Button>("LoginButton");
            var registerButton = this.FindControl<Button>("RegisterButton");

            if (_viewModel.IsRegistering)
            {
                if (modeText != null) modeText.Text = "Créer un compte";
                if (switchButton != null)
                {
                    switchButton.Content = "Déjà un compte ? Se connecter";
                    switchButton.Command = _viewModel.SwitchToLoginCommand;
                }
                if (loginButton != null) loginButton.IsVisible = false;
                if (registerButton != null) registerButton.IsVisible = true;
            }
            else
            {
                if (modeText != null) modeText.Text = "Se connecter";
                if (switchButton != null)
                {
                    switchButton.Content = "Pas de compte ? S'inscrire";
                    switchButton.Command = _viewModel.SwitchToRegisterCommand;
                }
                if (loginButton != null) loginButton.IsVisible = true;
                if (registerButton != null) registerButton.IsVisible = false;
            }
        }

        public LoginViewModel? GetViewModel() => _viewModel;
        
        private void UpdateErrorMessage()
        {
            var errorText = this.FindControl<TextBlock>("ErrorText");
            if (errorText != null && _viewModel != null)
            {
                errorText.IsVisible = !string.IsNullOrWhiteSpace(_viewModel.ErrorMessage);
            }
        }
    }
}
