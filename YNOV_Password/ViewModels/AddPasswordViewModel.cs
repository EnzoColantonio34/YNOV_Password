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

        private void Save()
        {
            if (!string.IsNullOrWhiteSpace(Site) && !string.IsNullOrWhiteSpace(Password))
            {
                var newEntry = new PasswordEntry
                {
                    Site = Site,
                    Username = Username,
                    Password = Password,
                    Url = Url
                };

                _mainViewModel.AddPassword(newEntry);
                _window.Close();
            }
        }

        private void Cancel()
        {
            _window.Close();
        }
    }
}
