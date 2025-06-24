using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using System.Linq;
using YNOV_Password.ViewModels;

namespace YNOV_Password.Views
{
    public partial class PasswordGeneratorWindow : Window
    {
        public string? GeneratedPassword { get; private set; }
        public bool WasPasswordSelected { get; private set; }

        public PasswordGeneratorWindow()
        {
            InitializeComponent();
            DataContext = new PasswordGeneratorViewModel();
            
            if (DataContext is PasswordGeneratorViewModel viewModel)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                UpdatePasswordStrength(viewModel.GeneratedPassword);
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PasswordGeneratorViewModel.GeneratedPassword) && 
                DataContext is PasswordGeneratorViewModel viewModel)
            {
                UpdatePasswordStrength(viewModel.GeneratedPassword);
            }
        }

        private void UpdatePasswordStrength(string password)
        {
            var strengthBar = this.FindControl<ProgressBar>("PasswordStrengthBar");
            var strengthText = this.FindControl<TextBlock>("PasswordStrengthText");
            
            if (strengthBar == null || strengthText == null) return;

            var (strength, color, text) = CalculatePasswordStrength(password);
            
            strengthBar.Value = strength;
            strengthBar.Foreground = new SolidColorBrush(color);
            strengthText.Text = text;
            strengthText.Foreground = new SolidColorBrush(color);
        }

        private static (double strength, Color color, string text) CalculatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
                return (0, Colors.Gray, "Aucun");

            double score = 0;
            double maxScore = 100;

            // Longueur (max 30 points)
            score += Math.Min(password.Length * 2, 30);

            // Variété de caractères (max 40 points)
            bool hasLower = password.Any(char.IsLower);
            bool hasUpper = password.Any(char.IsUpper);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

            int charTypes = (hasLower ? 1 : 0) + (hasUpper ? 1 : 0) + (hasDigit ? 1 : 0) + (hasSpecial ? 1 : 0);
            score += charTypes * 10;

            // Complexité supplémentaire (max 30 points)
            if (password.Length >= 12) score += 10;
            if (password.Length >= 16) score += 10;
            if (charTypes >= 3) score += 10;

            double strengthPercentage = (score / maxScore) * 100;

            return strengthPercentage switch
            {
                < 30 => (strengthPercentage, Color.FromRgb(239, 68, 68), "Faible"),
                < 60 => (strengthPercentage, Color.FromRgb(245, 158, 11), "Moyen"),
                < 80 => (strengthPercentage, Color.FromRgb(59, 130, 246), "Fort"),
                _ => (strengthPercentage, Color.FromRgb(34, 197, 94), "Très fort")
            };
        }

        private async void CopyPassword_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is PasswordGeneratorViewModel viewModel && 
                !string.IsNullOrEmpty(viewModel.GeneratedPassword))
            {
                var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                if (clipboard != null)
                {
                    await clipboard.SetTextAsync(viewModel.GeneratedPassword);
                    
                    // Optionnel : Afficher une notification temporaire
                    var button = sender as Button;
                    if (button != null)
                    {
                        var originalContent = button.Content;
                        button.Content = "✓";
                        await System.Threading.Tasks.Task.Delay(1000);
                        button.Content = originalContent;
                    }
                }
            }
        }

        private void UsePassword_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is PasswordGeneratorViewModel viewModel)
            {
                GeneratedPassword = viewModel.GeneratedPassword;
                WasPasswordSelected = true;
                Close();
            }
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e)
        {
            WasPasswordSelected = false;
            Close();
        }
    }
}
