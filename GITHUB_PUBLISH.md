# 📤 Instructions de publication GitHub

## Étape 1 : Créer le dépôt sur GitHub

1. **Aller sur GitHub** : https://github.com/new
2. **Sélectionner l'organisation** : DeLTa-X-Tunisia
3. **Nom du dépôt** : `TunRTC`
4. **Description** : `Open-Source WebRTC Signaling Server - Alternative to Agora.io and Twilio`
5. **Visibilité** : Public ✅
6. **NE PAS** cocher "Add a README file" (déjà créé)
7. **NE PAS** cocher "Add .gitignore" (déjà créé)
8. **NE PAS** cocher "Choose a license" (déjà créé)
9. Cliquer sur **"Create repository"**

## Étape 2 : Pousser le code

Une fois le dépôt créé, exécuter ces commandes :

```powershell
cd C:\Users\User\Desktop\Tunisia\TunRtc

# Ajouter le dépôt distant
git remote add origin https://github.com/DeLTa-X-Tunisia/TunRTC.git

# Renommer la branche en main
git branch -M main

# Pousser le code
git push -u origin main
```

## Étape 3 : Vérifier la publication

1. Aller sur : https://github.com/DeLTa-X-Tunisia/TunRTC
2. Vérifier que tous les fichiers sont présents :
   - ✅ README.md avec badges
   - ✅ LICENSE (MIT)
   - ✅ Server/ (code serveur)
   - ✅ SDKs/ (JavaScript et C#)
   - ✅ Tests/ (tests HTML et PowerShell)
   - ✅ Scripts/ (deploy.ps1, test-all.ps1)
   - ✅ ServerLauncher/ (application WPF)
   - ✅ Docs/ (documentation)
   - ✅ docker-compose.yml
   - ✅ coturn.conf
   - ✅ .gitignore

## Étape 4 : Configuration optionnelle

### Ajouter des topics au dépôt

Sur la page GitHub du projet, cliquer sur la roue dentée à côté de "About" et ajouter :
- `webrtc`
- `signaling-server`
- `aspnet-core`
- `signalr`
- `real-time`
- `video-conferencing`
- `open-source`
- `dotnet`
- `csharp`
- `javascript-sdk`

### Activer GitHub Pages (optionnel)

Settings → Pages → Source : Deploy from branch → Branch : main → /docs

---

## 🚀 Commandes complètes (copier-coller)

```powershell
# Se placer dans le dossier du projet
cd "C:\Users\User\Desktop\Tunisia\TunRtc"

# Ajouter le remote
git remote add origin https://github.com/DeLTa-X-Tunisia/TunRTC.git

# Renommer la branche
git branch -M main

# Pousser le code
git push -u origin main
```

---

## ✅ Vérification finale

Après le push, vérifier :
1. Le README s'affiche correctement
2. Les badges sont visibles
3. La licence MIT est détectée par GitHub
4. Le dépôt est bien public
5. Les fichiers sont tous présents

---

## 🔗 Lien du dépôt

Une fois publié, le lien sera :
**https://github.com/DeLTa-X-Tunisia/TunRTC**
