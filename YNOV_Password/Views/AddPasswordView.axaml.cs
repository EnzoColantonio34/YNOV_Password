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
            System.Diagnostics.Debug.WriteLine("[DEBUG] SaveButton_Click appelé");
            if (DataContext is AddPasswordViewModel viewModel)
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] DataContext est AddPasswordViewModel, appel direct de SaveEntry()");
                viewModel.SaveEntry();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] DataContext n'est pas AddPasswordViewModel: {DataContext?.GetType().Name}");
            }
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] CancelButton_Click appelé");
            if (DataContext is AddPasswordViewModel viewModel)
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] DataContext est AddPasswordViewModel, appel direct de CancelEntry()");
                viewModel.CancelEntry();
            }
        }
    }
}
