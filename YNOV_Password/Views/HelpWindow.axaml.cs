using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;

namespace YNOV_Password.Views
{
    public partial class HelpWindow : Window
    {
        private string _searchText = string.Empty;
        private readonly Dictionary<Border, bool> _sectionVisibility = new();
        private readonly Dictionary<Border, string> _sectionContent = new();

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value ?? string.Empty;
                PerformSearch();
            }
        }

        public HelpWindow()
        {
            InitializeComponent();
            this.Opened += HelpWindow_Opened;
        }

        private void HelpWindow_Opened(object? sender, EventArgs e)
        {
            InitializeSectionContent();
            PerformSearch(); // Afficher le nombre de sections initial
        }

        private void InitializeComponent()
        {
            Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
        }

        private void InitializeSectionContent()
        {
            // Initialiser le contenu de recherche pour chaque section
            var passwordManagementSection = this.FindControl<Border>("PasswordManagementSection");
            if (passwordManagementSection != null)
                _sectionContent[passwordManagementSection] = 
                    "gestion mots de passe ajouter voir copier modifier supprimer entrée bouton vert masquer révéler œil presse-papiers double-clic menu contextuel suppression";
            
            var passwordGeneratorSection = this.FindControl<Border>("PasswordGeneratorSection");
            if (passwordGeneratorSection != null)
                _sectionContent[passwordGeneratorSection] = 
                    "générateur mot de passe sécurisé générer personnalisation longueur caractères aléatoire mémorable français phrase robuste piratage";
            
            var searchSection = this.FindControl<Border>("SearchSection");
            if (searchSection != null)
                _sectionContent[searchSection] = 
                    "recherche filtrer instantané temps réel barre site web nom utilisateur email notes intelligent flexible partiel casse";
            
            var securitySection = this.FindControl<Border>("SecuritySection");
            if (securitySection != null)
                _sectionContent[securitySection] = 
                    "sécurité chiffrement militaire local algorithme banque gouvernement doublon détection principal protection déconnexion partagé";
            
            var wordLibrarySection = this.FindControl<Border>("WordLibrarySection");
            if (wordLibrarySection != null)
                _sectionContent[wordLibrarySection] = 
                    "bibliothèque mots personnaliser français importer uploader fichier texte créativité vocabulaire professionnel";
            
            var tipsSection = this.FindControl<Border>("TipsSection");
            if (tipsSection != null)
                _sectionContent[tipsSection] = 
                    "conseils experts sécurité optimale principal fort unique site audit mensuel sauvegarde backup vigilance phishing";
            
            var supportSection = this.FindControl<Border>("SupportSection");
            if (supportSection != null)
                _sectionContent[supportSection] = 
                    "aide support contact équipe question problème formation continue guide recommandation";

            // Initialiser la visibilité
            foreach (var section in _sectionContent.Keys)
            {
                _sectionVisibility[section] = true;
            }
        }

        private void PerformSearch()
        {
            var searchTerms = _searchText.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var visibleSections = 0;
            var totalSections = _sectionContent.Count;

            foreach (var kvp in _sectionContent)
            {
                var section = kvp.Key;
                var content = kvp.Value.ToLower();
                
                bool isVisible = string.IsNullOrEmpty(_searchText) || 
                               searchTerms.Any(term => content.Contains(term));

                section.IsVisible = isVisible;
                _sectionVisibility[section] = isVisible;

                if (isVisible) visibleSections++;
            }

            // Mettre à jour le texte des résultats
            var resultsText = this.FindControl<TextBlock>("SearchResultsText");
            if (resultsText != null)
            {
                if (string.IsNullOrEmpty(_searchText))
                {
                    resultsText.Text = $"{totalSections} sections disponibles";
                }
                else
                {
                    resultsText.Text = visibleSections == 0 
                        ? "Aucun résultat trouvé" 
                        : $"{visibleSections}/{totalSections} section(s) trouvée(s)";
                }
            }
        }

        private void SearchTextBox_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                SearchText = textBox.Text ?? string.Empty;
            }
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
