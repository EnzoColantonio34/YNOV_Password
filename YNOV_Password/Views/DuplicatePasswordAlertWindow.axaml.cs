using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using YNOV_Password.Models;
using YNOV_Password.Views;
using YNOV_Password.ViewModels;

namespace YNOV_Password.Views
{
    public partial class DuplicatePasswordAlertWindow : Window
    {
        private PasswordEntry _passwordEntry = null!;
        private User _currentUser = null!;

        public event Action<PasswordEntry>? ModifyPasswordRequested;
        public event Action<PasswordEntry>? GeneratePasswordRequested;

        public DuplicatePasswordAlertWindow()
        {
            InitializeComponent();
        }

        public DuplicatePasswordAlertWindow(PasswordEntry passwordEntry, User currentUser) : this()
        {
            _passwordEntry = passwordEntry;
            _currentUser = currentUser;
            SiteNameTextBlock.Text = passwordEntry.Site ?? "Site non spécifié";
        }

        private void ModifyPassword_Click(object? sender, RoutedEventArgs e)
        {
            ModifyPasswordRequested?.Invoke(_passwordEntry);
            Close();
        }

        private void GeneratePassword_Click(object? sender, RoutedEventArgs e)
        {
            GeneratePasswordRequested?.Invoke(_passwordEntry);
            Close();
        }

        private void Close_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
