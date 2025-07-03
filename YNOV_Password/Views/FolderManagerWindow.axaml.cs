using Avalonia.Controls;
using YNOV_Password.ViewModels;

namespace YNOV_Password.Views
{
    public partial class FolderManagerWindow : Window
    {
        public FolderManagerWindow()
        {
            InitializeComponent();
        }

        public FolderManagerWindow(FolderManagerViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
