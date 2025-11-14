# Script de publication automatique sur GitHub
# Prépare et pousse le projet TunRTC sur GitHub

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "📤 Publication de TunRTC sur GitHub" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$repoUrl = "https://github.com/DeLTa-X-Tunisia/TunRTC.git"
$repoName = "TunRTC"
$orgName = "DeLTa-X-Tunisia"

# Vérifier qu'on est dans le bon dossier
$currentPath = Get-Location
if (-not (Test-Path "Server") -or -not (Test-Path "SDKs")) {
    Write-Host "❌ ERREUR: Ce script doit être exécuté depuis le dossier racine de TunRTC" -ForegroundColor Red
    exit 1
}

Write-Host "📂 Dossier du projet: $currentPath" -ForegroundColor Yellow
Write-Host ""

# Étape 1: Vérifier Git
Write-Host "🔍 Vérification de Git..." -ForegroundColor Cyan
try {
    $gitVersion = git --version
    Write-Host "  ✅ Git installé: $gitVersion" -ForegroundColor Green
} catch {
    Write-Host "  ❌ Git n'est pas installé" -ForegroundColor Red
    Write-Host "  Télécharger Git: https://git-scm.com/download/win" -ForegroundColor Yellow
    exit 1
}
Write-Host ""

# Étape 2: Vérifier l'état du dépôt
Write-Host "🔍 Vérification de l'état Git..." -ForegroundColor Cyan
$gitStatus = git status --porcelain
if ($gitStatus) {
    Write-Host "  ⚠️ Il y a des changements non commités" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Voulez-vous commiter ces changements ? (O/N)" -ForegroundColor Yellow
    $commit = Read-Host
    
    if ($commit -eq "O" -or $commit -eq "o") {
        Write-Host "📝 Ajout des fichiers..." -ForegroundColor Cyan
        git add .
        
        Write-Host "💬 Message de commit:" -ForegroundColor Yellow
        $commitMessage = Read-Host
        
        git commit -m $commitMessage
        Write-Host "  ✅ Changements commités" -ForegroundColor Green
    }
} else {
    Write-Host "  ✅ Aucun changement en attente" -ForegroundColor Green
}
Write-Host ""

# Étape 3: Instructions pour créer le dépôt GitHub
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "📋 ÉTAPE MANUELLE REQUISE" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Vous devez maintenant créer le dépôt sur GitHub :" -ForegroundColor White
Write-Host ""
Write-Host "1. Ouvrir votre navigateur et aller sur:" -ForegroundColor Yellow
Write-Host "   https://github.com/organizations/$orgName/repositories/new" -ForegroundColor Cyan
Write-Host ""
Write-Host "2. Remplir le formulaire :" -ForegroundColor Yellow
Write-Host "   - Repository name: $repoName" -ForegroundColor White
Write-Host "   - Description: Open-Source WebRTC Signaling Server - Alternative to Agora.io" -ForegroundColor White
Write-Host "   - Public ✅" -ForegroundColor White
Write-Host "   - NE PAS cocher 'Add a README file'" -ForegroundColor Red
Write-Host "   - NE PAS cocher 'Add .gitignore'" -ForegroundColor Red
Write-Host "   - NE PAS cocher 'Choose a license'" -ForegroundColor Red
Write-Host ""
Write-Host "3. Cliquer sur 'Create repository'" -ForegroundColor Yellow
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Avez-vous créé le dépôt sur GitHub ? (O/N)" -ForegroundColor Yellow
$created = Read-Host

if ($created -ne "O" -and $created -ne "o") {
    Write-Host ""
    Write-Host "⏸️ Publication annulée" -ForegroundColor Yellow
    Write-Host "Relancez ce script après avoir créé le dépôt" -ForegroundColor White
    Write-Host ""
    exit 0
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🚀 Publication du code sur GitHub" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Étape 4: Ajouter le remote (si pas déjà fait)
Write-Host "🔗 Configuration du remote..." -ForegroundColor Cyan
$remotes = git remote -v
if ($remotes -match "origin") {
    Write-Host "  ℹ️ Remote 'origin' existe déjà" -ForegroundColor Yellow
    Write-Host "  Voulez-vous le remplacer ? (O/N)" -ForegroundColor Yellow
    $replace = Read-Host
    
    if ($replace -eq "O" -or $replace -eq "o") {
        git remote remove origin
        git remote add origin $repoUrl
        Write-Host "  ✅ Remote mis à jour" -ForegroundColor Green
    }
} else {
    git remote add origin $repoUrl
    Write-Host "  ✅ Remote ajouté: $repoUrl" -ForegroundColor Green
}
Write-Host ""

# Étape 5: Renommer la branche en main
Write-Host "🌿 Vérification de la branche..." -ForegroundColor Cyan
$currentBranch = git branch --show-current
if ($currentBranch -ne "main") {
    Write-Host "  🔄 Renommage de '$currentBranch' en 'main'..." -ForegroundColor Yellow
    git branch -M main
    Write-Host "  ✅ Branche renommée" -ForegroundColor Green
} else {
    Write-Host "  ✅ Branche déjà sur 'main'" -ForegroundColor Green
}
Write-Host ""

# Étape 6: Push vers GitHub
Write-Host "📤 Envoi du code vers GitHub..." -ForegroundColor Cyan
Write-Host "  Cela peut prendre quelques instants..." -ForegroundColor Yellow
Write-Host ""

try {
    # Essayer de pousser
    git push -u origin main 2>&1 | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Cyan
        Write-Host "✅ PUBLICATION RÉUSSIE !" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "🌐 Votre projet est maintenant en ligne :" -ForegroundColor White
        Write-Host "   https://github.com/$orgName/$repoName" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "📋 Prochaines étapes recommandées :" -ForegroundColor Yellow
        Write-Host "   1. Ajouter des topics au dépôt (webrtc, signaling-server, etc.)" -ForegroundColor White
        Write-Host "   2. Configurer GitHub Pages si nécessaire" -ForegroundColor White
        Write-Host "   3. Inviter des collaborateurs" -ForegroundColor White
        Write-Host ""
        
        # Ouvrir le dépôt dans le navigateur
        Write-Host "Voulez-vous ouvrir le dépôt dans votre navigateur ? (O/N)" -ForegroundColor Yellow
        $open = Read-Host
        
        if ($open -eq "O" -or $open -eq "o") {
            Start-Process "https://github.com/$orgName/$repoName"
        }
        
    } else {
        Write-Host ""
        Write-Host "⚠️ Une erreur s'est produite lors du push" -ForegroundColor Yellow
        Write-Host "Vérifiez vos identifiants GitHub et réessayez" -ForegroundColor White
        Write-Host ""
        Write-Host "💡 Conseil: Configurez un Personal Access Token (PAT)" -ForegroundColor Yellow
        Write-Host "   https://github.com/settings/tokens" -ForegroundColor Cyan
    }
    
} catch {
    Write-Host ""
    Write-Host "❌ ERREUR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Assurez-vous d'être connecté à GitHub" -ForegroundColor Yellow
    Write-Host "   Utilisez: git config --global credential.helper manager" -ForegroundColor White
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✅ Script terminé" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
