using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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
    }
}
