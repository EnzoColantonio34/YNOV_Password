using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;
using YNOV_Password.Models;
using YNOV_Password.ViewModels;

namespace YNOV_Password.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Ne pas définir DataContext ici, il est défini dans App.axaml.cs
        }
    }
}