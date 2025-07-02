using CommunityToolkit.Mvvm.ComponentModel;

namespace YNOV_Password.Models
{
    public partial class PasswordEntry : ObservableObject
    {
        public int Id { get; set; }
        public string? Site { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Url { get; set; }
        
        [ObservableProperty]
        private bool _isPasswordVisible = false;
        
        [ObservableProperty]
        private int _remainingTime = 0;
        
        [ObservableProperty]
        private bool _isDuplicate = false;
    }
}