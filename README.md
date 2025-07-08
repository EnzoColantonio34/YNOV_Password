# YNOV Password Manager

Gestionnaire de mots de passe multiplateforme, sécurisé et moderne, développé en C# (.NET 8, Avalonia UI). Ce projet répond à un cahier des charges pédagogique et propose une expérience utilisateur complète, adaptée à un usage quotidien ou à une évaluation académique.

---

## Présentation

YNOV Password Manager permet de stocker, organiser et générer des mots de passe de façon sécurisée. L’application prend en charge plusieurs utilisateurs, propose une organisation par dossiers, un générateur de mots de passe basé sur un dictionnaire français, et intègre des fonctionnalités avancées comme la détection de doublons et la gestion de bibliothèques de mots personnalisées.

---

## Fonctionnalités principales

- **Stockage sécurisé** : chiffrement AES-256 de tous les mots de passe, clé dérivée du mot de passe principal de l’utilisateur.
- **Multi-utilisateurs** : chaque utilisateur dispose de son propre espace, aucune donnée n’est partagée.
- **Organisation par dossiers** : création, modification et suppression de dossiers thématiques, personnalisation couleur et icône.
- **Générateur de mots de passe** : mode aléatoire ou basé sur des mots français, indicateur de robustesse intégré.
- **Détection de doublons** : alerte en cas de réutilisation d’un mot de passe.
- **Recherche instantanée** : filtrage en temps réel par site, identifiant, dossier, etc.
- **Aide intégrée** : documentation accessible depuis l’application.
- **Interface moderne** : thème clair/sombre, styles centralisés dans `main.axaml`, design cohérent sur toutes les fenêtres.
- **Gestion de bibliothèques de mots** : import/export de listes personnalisées pour le générateur.
- **Affichage temporaire** : masquage automatique des mots de passe après 10 secondes.

---

## Installation et lancement

### Prérequis
- .NET 8.0 SDK ou supérieur
- Rider, Visual Studio 2022 ou VS Code

### Clonage et compilation
```bash
git clone https://github.com/EnzoColantonio34/YNOV_Password.git
cd YNOV_Password
dotnet restore
dotnet build
```

### Lancement
- **Depuis l’IDE** : ouvrez la solution `.sln`, sélectionnez le projet principal, lancez l’exécution.
- **En ligne de commande** :
```bash
dotnet run --project YNOV_Password/YNOV_Password.csproj
```

Au premier lancement, un utilisateur de test est créé automatiquement :
- Email : `admin@example.com`
- Mot de passe : `admin123`

La base de données SQLite (`passwords.db`) est générée automatiquement.

---

## Utilisation

### Connexion / Inscription
- Créez un compte ou connectez-vous avec l’utilisateur de test.
- Chaque utilisateur a accès uniquement à ses propres données.

### Gestion des dossiers
- Ajoutez, modifiez ou supprimez des dossiers pour organiser vos mots de passe.
- Personnalisez chaque dossier (nom, couleur, icône).

### Ajout et gestion des mots de passe
- Ajoutez un mot de passe via le bouton principal.
- Renseignez site, identifiant, mot de passe, URL, dossier.
- Copiez, affichez, modifiez ou supprimez chaque entrée.
- Les mots de passe sont masqués par défaut et s’affichent temporairement.

### Générateur de mots de passe
- Accédez au générateur depuis la barre d’outils ou lors de l’ajout d’un mot de passe.
- Choisissez entre génération aléatoire ou basée sur des mots français.
- Personnalisez la longueur, les types de caractères, le nombre de mots, etc.
- L’indicateur de force s’actualise en temps réel.

### Détection de doublons
- Si un mot de passe est déjà utilisé, une alerte s’affiche.
- Possibilité de modifier, régénérer ou ignorer.

### Bibliothèques de mots
- Importez vos propres listes de mots (`.txt`), ou utilisez le dictionnaire fourni.
- Gérez plusieurs bibliothèques thématiques.

### Recherche
- Utilisez la barre de recherche pour filtrer instantanément vos mots de passe.
- Filtrage possible par dossier, site, identifiant, etc.

### Aide intégrée
- Cliquez sur le bouton "?" pour accéder à la documentation utilisateur.
- Recherche par mot-clé et navigation par section.

---

## Sécurité

- Chiffrement AES-256 pour tous les mots de passe.
- Clé dérivée du mot de passe principal de l’utilisateur (jamais stockée en clair).
- Hashage SHA-256 des mots de passe de connexion.
- Isolation stricte des données entre utilisateurs.
- Masquage automatique des mots de passe affichés.
- Détection de réutilisation de mots de passe.

---

## Architecture technique

- **.NET 8.0** / **Avalonia UI** (cross-platform)
- **Entity Framework Core** (migrations automatiques, requêtes LINQ)
- **MVVM** (CommunityToolkit.MVVM)
- **SQLite** (base locale, portable)
- **Styles centralisés** dans `Styles/main.axaml` (light mode)

Structure des dossiers :
```
YNOV_Password/
├── Commands/
├── Converters/
├── Models/
├── Services/
├── Styles/
├── ViewModels/
├── Views/
└── Assets/
```

---

- L’utilisateur de test permet de valider rapidement toutes les fonctionnalités.
- L’aide intégrée couvre l’ensemble des cas d’usage.
- La gestion multi-utilisateurs et l’isolation des données peuvent être testées en créant plusieurs comptes.
- Le générateur de mots de passe propose deux modes (aléatoire et phrase française).
- La détection de doublons s’active lors de la saisie d’un mot de passe déjà existant.

---

## Contact & contribution

Projet développé sur macOS, testé sur Windows. Pour toute suggestion ou retour, ouvrir une issue ou proposer une pull request.

---

YNOV Password Manager – Projet pédagogique, sécurisé et prêt à l’emploi.
