using Avalonia.Controls;
using YNOV_Password.ViewModels;

namespace YNOV_Password.Views
{
    public partial class WordLibraryManagerWindow : Window
    {
        public WordLibraryManagerWindow()
        {
            InitializeComponent();
            DataContext = new WordLibraryManagerViewModel();
        }
    }
}
