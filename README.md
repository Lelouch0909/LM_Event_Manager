# LM Event Manager - Gestion d'Événements

![Bannière DriverSolution](screenshots/banner.png)

## 📋 Description

Application WPF complète pour la gestion professionnelle d'événements avec :

- 🎯 Planification d'activités
- 👥 Gestion des participants
- 📈 Analyse statistique
- 🔗 Intégration réseaux sociaux

## ✨ Fonctionnalités Principales

### 🗂 Gestion des Événements
- Création/modification d'événements
- Calendrier interactif
- Gestion multi-lieux (présentiel/distanciel)

### 📊 Tableaux de Bord
```mermaid
pie title Participation
    "Présents" : 75
    "Absents" : 25
🤖 Fonctionnalités Avancées
Partage automatique sur Twitter/Facebook

Génération de QR codes d'accès

Synchronisation cloud via Appwrite

🛠 Stack Technique
Composant	Technologie
Frontend	WPF (.NET 6)
UI Toolkit	MaterialDesignInXAML
Backend	Appwrite
Graphiques	LiveCharts
CI/CD	GitHub Actions
🚀 Guide d'Installation
Prérequis :

bash
dotnet --version # >= 6.0
Configuration :

bash
git clone https://github.com/votre-repo/DriverSolution.git
cd DriverSolution
Variables d'environnement :

ini
APWRITE_ENDPOINT=https://your.appwrite.io/v1
APWRITE_PROJECT=your-project-id
Lancement :

bash
dotnet run --project DriverSolution.csproj
📸 Gallerie
Écran Principal	Détails Activité
Dashboard	Details
📂 Structure du Code
text
DriverSolution/
├── Models/
│   ├── Event.cs
│   └── Activity.cs
├── Services/
│   └── AppwriteService.cs
├── Views/
│   ├── MainWindow.xaml
│   └── StatisticsView.xaml
└── Converters/
    └── NullToVisibilityConverter.cs
🤝 Comment Contribuer
Créez un fork du projet

Initialisez vos modifications :

bash
git checkout -b feature/nouvelle-fonctionnalite
Soumettez vos changements :

bash
git commit -m "feat: ajout nouvelle fonctionnalité"
git push origin feature/nouvelle-fonctionnalite
📜 Licence
MIT License - Voir LICENSE.md pour plus d'informations.
