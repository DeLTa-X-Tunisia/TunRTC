# TunRTC Server Launcher

Application Windows (.exe) pour démarrer et gérer le serveur TunRTC sans ligne de commande.

## 🚀 Utilisation

### Option 1 : Exécuter depuis le code source
```powershell
cd ServerLauncher
dotnet run --project TunRTC.Launcher.csproj
```

### Option 2 : Créer un exécutable (.exe)
```powershell
cd ServerLauncher
.\publish.ps1
```

L'exécutable sera généré dans `ServerLauncher/Published/TunRTC.Launcher.exe`

## ✨ Fonctionnalités

- **Démarrer/Arrêter/Redémarrer** le serveur TunRTC d'un simple clic
- **Indicateur d'état** en temps réel (serveur en ligne / arrêté)
- **Visualisation des logs** du serveur en direct
- **Accès rapide** à Swagger UI et aux tests SignalR
- **Détection automatique** du projet TunRTC
- **Interface moderne** avec Material Design

## 📋 Prérequis

- .NET 8.0 SDK ou Runtime (pour l'exécutable self-contained, aucun prérequis nécessaire)
- Windows 10 ou supérieur

## 🎨 Interface

L'application affiche :
- **Panneau d'état** : Indicateur visuel (rouge = arrêté, vert = en ligne)
- **Boutons de contrôle** : Démarrer, Arrêter, Redémarrer
- **Console de logs** : Affichage en temps réel des logs du serveur
- **Liens rapides** : Swagger UI, Tests SignalR, Dossier du projet

## 📦 Publication

Le script `publish.ps1` crée un exécutable autonome avec :
- ✅ .NET Runtime inclus (self-contained)
- ✅ Fichier unique (single-file)
- ✅ Optimisé pour Windows x64
- ✅ ~75 MB (avec toutes les dépendances)

## 🔧 Structure technique

```
ServerLauncher/
├── TunRTC.Launcher.csproj  # Configuration du projet WPF
├── App.xaml                # Application WPF
├── App.xaml.cs            
├── MainWindow.xaml         # Interface utilisateur
├── MainWindow.xaml.cs      # Logique métier
├── publish.ps1             # Script de publication
└── Published/              # Dossier de sortie (généré)
    └── TunRTC.Launcher.exe
```

## 🎯 Utilisation de l'exécutable

1. Double-cliquez sur `TunRTC.Launcher.exe`
2. Cliquez sur **"Démarrer"** pour lancer le serveur
3. Le serveur démarre sur `http://localhost:5000`
4. Utilisez les boutons pour gérer le serveur
5. Les logs s'affichent en temps réel dans la console intégrée

## 🛡️ Sécurité

- L'application demande confirmation avant de quitter si le serveur est en cours d'exécution
- Le serveur est arrêté proprement lors de la fermeture
- Gestion d'erreurs complète avec messages explicites

## 📝 Notes

- L'application détecte automatiquement le dossier du projet TunRTC
- Le serveur doit être situé dans `../Server/TunRTC.Server.csproj` (par rapport au launcher)
- Les logs sont horodatés et affichés en temps réel
- La vérification de santé du serveur se fait toutes les 3 secondes

## 🐛 Dépannage

**Erreur "Projet serveur introuvable"** :
- Vérifiez que le launcher est bien dans le dossier `ServerLauncher/`
- Vérifiez que le dossier `Server/` existe avec `TunRTC.Server.csproj`

**Le serveur ne démarre pas** :
- Vérifiez que .NET 8.0 SDK est installé : `dotnet --version`
- Vérifiez les logs dans la console de l'application
- Assurez-vous que le port 5000 n'est pas déjà utilisé

**L'exécutable ne fonctionne pas** :
- Utilisez le mode self-contained avec `publish.ps1`
- Vérifiez que vous utilisez Windows 10 ou supérieur
