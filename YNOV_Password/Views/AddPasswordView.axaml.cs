using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using YNOV_Password.ViewModels;

namespace YNOV_Password.Views
{
    public partial class AddPasswordView : UserControl
    {
        public AddPasswordView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is AddPasswordViewModel viewModel)
            {
                viewModel.SaveEntry();
            }
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is AddPasswordViewModel viewModel)
            {
                viewModel.CancelEntry();
            }
        }
    }
}
