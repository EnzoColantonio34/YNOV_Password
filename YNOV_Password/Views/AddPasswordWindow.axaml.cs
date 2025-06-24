using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using YNOV_Password.ViewModels;
using System.Diagnostics;

namespace YNOV_Password.Views
{
    public partial class AddPasswordWindow : Window
    {
        public AddPasswordWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public AddPasswordWindow(MainWindowViewModel mainViewModel) : this()
        {
            var viewModel = new AddPasswordViewModel(this, mainViewModel);
            DataContext = viewModel;
        }

        public AddPasswordWindow(MainWindowViewModel mainViewModel, string preGeneratedPassword) : this()
        {
            var viewModel = new AddPasswordViewModel(this, mainViewModel, preGeneratedPassword);
            DataContext = viewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
