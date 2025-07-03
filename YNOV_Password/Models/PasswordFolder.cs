using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace YNOV_Password.Models
{
    public partial class PasswordFolder : ObservableObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "#3B82F6"; // Couleur par défaut (bleu)
        public string Icon { get; set; } = "📁"; // Icône par défaut
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [ObservableProperty]
        private bool _isExpanded = true;
        
        [ObservableProperty]
        private bool _isSelected = false;
        
        // Propriété pour compter les mots de passe dans ce dossier
        [ObservableProperty]
        private int _passwordCount = 0;
    }
}
