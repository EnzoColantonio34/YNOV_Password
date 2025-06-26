using System;
using System.Linq;
using System.Text;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using YNOV_Password.Commands;
using YNOV_Password.Services;

namespace YNOV_Password.ViewModels
{
    public partial class PasswordGeneratorViewModel : ViewModelBase
    {
        [ObservableProperty]
        private int passwordLength = 12;

        [ObservableProperty]
        private bool includeUppercase = true;

        [ObservableProperty]
        private bool includeLowercase = true;

        [ObservableProperty]
        private bool includeNumbers = true;

        [ObservableProperty]
        private bool includeSpecialChars = true;

        [ObservableProperty]
        private string generatedPassword = string.Empty;

        [ObservableProperty]
        private string customSpecialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        // Propriétés pour la génération basée sur les mots
        [ObservableProperty]
        private bool useWordBased = false;

        [ObservableProperty]
        private int wordCount = 3;

        [ObservableProperty]
        private string wordSeparator = "-";

        [ObservableProperty]
        private bool hasWordsAvailable = false;

        private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        private const string NumberChars = "0123456789";

        public PasswordGeneratorViewModel()
        {
            GeneratePasswordCommand = new RelayCommand<object>(_ => GeneratePassword());
            
            // Vérifier si des mots sont disponibles
            UpdateWordAvailability();
            
            GeneratePassword(); // Générer un mot de passe initial
        }

        public ICommand GeneratePasswordCommand { get; }

        private void UpdateWordAvailability()
        {
            HasWordsAvailable = WordLibraryService.Instance.HasWords;
        }

        public void RefreshWordAvailability()
        {
            UpdateWordAvailability();
            OnPropertyChanged(nameof(HasWordsAvailable));
        }

        private void GeneratePassword()
        {
            UpdateWordAvailability();
            
            if (UseWordBased && HasWordsAvailable)
            {
                GenerateWordBasedPassword();
            }
            else
            {
                GenerateCharacterBasedPassword();
            }
        }

        private void GenerateWordBasedPassword()
        {
            if (WordCount < 1)
            {
                GeneratedPassword = string.Empty;
                return;
            }

            var wordLibrary = WordLibraryService.Instance;
            if (!wordLibrary.HasWords)
            {
                GeneratedPassword = "Aucune bibliothèque de mots disponible";
                return;
            }

            GeneratedPassword = wordLibrary.GeneratePasswordFromWords(WordCount, WordSeparator);
        }

        private void GenerateCharacterBasedPassword()
        {
            if (PasswordLength < 1)
            {
                GeneratedPassword = string.Empty;
                return;
            }

            var characterSet = new StringBuilder();

            if (IncludeUppercase)
                characterSet.Append(UppercaseChars);

            if (IncludeLowercase)
                characterSet.Append(LowercaseChars);

            if (IncludeNumbers)
                characterSet.Append(NumberChars);

            if (IncludeSpecialChars)
                characterSet.Append(CustomSpecialChars);

            if (characterSet.Length == 0)
            {
                GeneratedPassword = string.Empty;
                return;
            }

            var random = new Random();
            var password = new StringBuilder();

            // S'assurer qu'au moins un caractère de chaque type sélectionné est inclus
            if (IncludeUppercase && PasswordLength > 0)
                password.Append(UppercaseChars[random.Next(UppercaseChars.Length)]);

            if (IncludeLowercase && PasswordLength > password.Length)
                password.Append(LowercaseChars[random.Next(LowercaseChars.Length)]);

            if (IncludeNumbers && PasswordLength > password.Length)
                password.Append(NumberChars[random.Next(NumberChars.Length)]);

            if (IncludeSpecialChars && PasswordLength > password.Length && !string.IsNullOrEmpty(CustomSpecialChars))
                password.Append(CustomSpecialChars[random.Next(CustomSpecialChars.Length)]);

            // Remplir le reste avec des caractères aléatoires
            var availableChars = characterSet.ToString();
            while (password.Length < PasswordLength)
            {
                password.Append(availableChars[random.Next(availableChars.Length)]);
            }

            // Mélanger le mot de passe pour éviter un ordre prévisible
            var passwordArray = password.ToString().ToCharArray();
            for (int i = passwordArray.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (passwordArray[i], passwordArray[j]) = (passwordArray[j], passwordArray[i]);
            }

            GeneratedPassword = new string(passwordArray);
        }

        partial void OnPasswordLengthChanged(int value)
        {
            if (value > 0 && value <= 128) // Limiter la longueur maximale
            {
                if (!UseWordBased)
                    GeneratePassword();
            }
        }

        partial void OnIncludeUppercaseChanged(bool value)
        {
            if (!UseWordBased)
                GeneratePassword();
        }

        partial void OnIncludeLowercaseChanged(bool value)
        {
            if (!UseWordBased)
                GeneratePassword();
        }

        partial void OnIncludeNumbersChanged(bool value)
        {
            if (!UseWordBased)
                GeneratePassword();
        }

        partial void OnIncludeSpecialCharsChanged(bool value)
        {
            if (!UseWordBased)
                GeneratePassword();
        }

        partial void OnCustomSpecialCharsChanged(string value)
        {
            if (IncludeSpecialChars && !UseWordBased)
            {
                GeneratePassword();
            }
        }

        partial void OnUseWordBasedChanged(bool value)
        {
            GeneratePassword();
        }

        partial void OnWordCountChanged(int value)
        {
            if (UseWordBased && value > 0 && value <= 10)
            {
                GeneratePassword();
            }
        }

        partial void OnWordSeparatorChanged(string value)
        {
            if (UseWordBased)
            {
                GeneratePassword();
            }
        }
    }
}
