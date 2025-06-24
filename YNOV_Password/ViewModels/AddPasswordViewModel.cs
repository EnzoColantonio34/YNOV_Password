using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using YNOV_Password.Commands;
using YNOV_Password.Models;

namespace YNOV_Password.ViewModels
{
    public partial class AddPasswordViewModel : ViewModelBase
    {
        private readonly Window _window;
        private readonly MainWindowViewModel _mainViewModel;

        [ObservableProperty]
        private string _site = string.Empty;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _url = string.Empty;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddPasswordViewModel(Window window, MainWindowViewModel mainViewModel)
        {
            _window = window;
            _mainViewModel = mainViewModel;

            SaveCommand = new RelayCommand<object>(_ => Save());
            CancelCommand = new RelayCommand<object>(_ => Cancel());
        }

        public AddPasswordViewModel(Window window, MainWindowViewModel mainViewModel, string preGeneratedPassword) : this(window, mainViewModel)
        {
            Password = preGeneratedPassword;
        }

        private void Save()
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Save() appelée - Site: '{Site}', Password: '{Password}'");
            
            if (!string.IsNullOrWhiteSpace(Site) && !string.IsNullOrWhiteSpace(Password))
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] Validation OK, création de l'entrée");
                
                var newEntry = new PasswordEntry
                {
                    Site = Site,
                    Username = Username,
                    Password = Password,
                    Url = Url
                };

                System.Diagnostics.Debug.WriteLine("[DEBUG] Appel de AddPassword sur MainViewModel");
                _mainViewModel.AddPassword(newEntry);
                
                System.Diagnostics.Debug.WriteLine("[DEBUG] Fermeture de la fenêtre");
                _window.Close();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] Validation échouée - Site ou Password vide");
            }
        }

        private void Cancel()
        {
            _window.Close();
        }

        public void SaveEntry()
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] SaveEntry() appelée directement - Site: '{Site}', Password: '{Password}'");
            Save();
        }

        public void CancelEntry()
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] CancelEntry() appelée directement");
            Cancel();
        }
    }
}
