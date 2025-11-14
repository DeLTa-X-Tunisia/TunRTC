# Script de déploiement automatisé TunRTC
# Démarre le serveur TunRTC en mode production

param(
    [Parameter(Mandatory=$false)]
    [string]$Environment = "Production",
    
    [Parameter(Mandatory=$false)]
    [int]$Port = 5000,
    
    [Parameter(Mandatory=$false)]
    [switch]$UseDocker
)

$ErrorActionPreference = "Stop"

Write-Host "🚀 Déploiement de TunRTC" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Déterminer le chemin racine du projet
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootPath = Split-Path -Parent $scriptPath
$serverPath = Join-Path $rootPath "Server"

Write-Host "📂 Chemin du projet: $rootPath" -ForegroundColor Yellow
Write-Host "🌍 Environnement: $Environment" -ForegroundColor Yellow
Write-Host "🔌 Port: $Port" -ForegroundColor Yellow
Write-Host ""

if ($UseDocker) {
    Write-Host "🐳 Déploiement avec Docker..." -ForegroundColor Green
    
    # Vérifier que Docker est installé
    try {
        docker --version | Out-Null
    } catch {
        Write-Host "❌ ERREUR: Docker n'est pas installé" -ForegroundColor Red
        exit 1
    }
    
    # Build de l'image Docker
    Write-Host "📦 Construction de l'image Docker..." -ForegroundColor Cyan
    Set-Location $rootPath
    docker build -t tunrtc-server:latest .
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Échec de la construction de l'image" -ForegroundColor Red
        exit 1
    }
    
    # Arrêter le conteneur existant
    Write-Host "🛑 Arrêt du conteneur existant..." -ForegroundColor Yellow
    docker stop tunrtc-server 2>$null
    docker rm tunrtc-server 2>$null
    
    # Lancer le nouveau conteneur
    Write-Host "🚀 Démarrage du conteneur..." -ForegroundColor Green
    docker run -d `
        --name tunrtc-server `
        -p ${Port}:80 `
        -e ASPNETCORE_ENVIRONMENT=$Environment `
        tunrtc-server:latest
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ Serveur démarré avec succès!" -ForegroundColor Green
        Write-Host "🌐 URL: http://localhost:$Port" -ForegroundColor Cyan
        Write-Host "📖 Swagger: http://localhost:$Port/swagger" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "📊 Pour voir les logs:" -ForegroundColor Yellow
        Write-Host "   docker logs -f tunrtc-server" -ForegroundColor White
    } else {
        Write-Host "❌ Échec du démarrage du conteneur" -ForegroundColor Red
        exit 1
    }
    
} else {
    Write-Host "⚙️ Déploiement avec .NET..." -ForegroundColor Green
    
    # Vérifier que .NET est installé
    try {
        dotnet --version | Out-Null
    } catch {
        Write-Host "❌ ERREUR: .NET 8.0 SDK n'est pas installé" -ForegroundColor Red
        exit 1
    }
    
    # Restaurer les dépendances
    Write-Host "📦 Restauration des dépendances..." -ForegroundColor Cyan
    Set-Location $serverPath
    dotnet restore
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Échec de la restauration" -ForegroundColor Red
        exit 1
    }
    
    # Build du projet
    Write-Host "🔨 Compilation du projet..." -ForegroundColor Cyan
    dotnet build --configuration Release
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Échec de la compilation" -ForegroundColor Red
        exit 1
    }
    
    # Publier le projet
    Write-Host "📦 Publication du projet..." -ForegroundColor Cyan
    $publishPath = Join-Path $serverPath "bin\Release\net8.0\publish"
    dotnet publish --configuration Release --output $publishPath
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Échec de la publication" -ForegroundColor Red
        exit 1
    }
    
    # Arrêter les instances existantes
    Write-Host "🛑 Arrêt des instances existantes..." -ForegroundColor Yellow
    Get-Process -Name "TunRTC.Server" -ErrorAction SilentlyContinue | Stop-Process -Force
    Start-Sleep -Seconds 2
    
    # Démarrer le serveur
    Write-Host "🚀 Démarrage du serveur..." -ForegroundColor Green
    $env:ASPNETCORE_ENVIRONMENT = $Environment
    $env:ASPNETCORE_URLS = "http://localhost:$Port"
    
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$publishPath'; dotnet TunRTC.Server.dll" -WindowStyle Normal
    
    Start-Sleep -Seconds 5
    
    # Vérifier que le serveur est démarré
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:$Port/health" -TimeoutSec 5
        Write-Host ""
        Write-Host "✅ Serveur démarré avec succès!" -ForegroundColor Green
        Write-Host "🌐 URL: http://localhost:$Port" -ForegroundColor Cyan
        Write-Host "📖 Swagger: http://localhost:$Port/swagger" -ForegroundColor Cyan
        Write-Host "💚 Statut: $($response.status)" -ForegroundColor Green
    } catch {
        Write-Host ""
        Write-Host "⚠️ Le serveur démarre... (vérifiez dans quelques secondes)" -ForegroundColor Yellow
        Write-Host "🌐 URL attendue: http://localhost:$Port" -ForegroundColor Cyan
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✅ Déploiement terminé!" -ForegroundColor Green
