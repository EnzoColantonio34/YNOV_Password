using CommunityToolkit.Mvvm.ComponentModel;

namespace YNOV_Password.Models
{
    public partial class User : ObservableObject
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
