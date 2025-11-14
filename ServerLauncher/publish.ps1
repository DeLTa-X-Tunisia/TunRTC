# Script de publication de TunRTC Launcher
# Crée un exécutable Windows autonome (.exe)

Write-Host "🚀 Publication de TunRTC Launcher..." -ForegroundColor Cyan

# Chemin du projet
$projectPath = "C:\Users\User\Desktop\Tunisia\TunRtc\ServerLauncher\TunRTC.Launcher.csproj"
$outputPath = "C:\Users\User\Desktop\Tunisia\TunRtc\ServerLauncher\Published"

# Suppression du dossier de sortie existant
if (Test-Path $outputPath) {
    Write-Host "🗑️ Nettoyage du dossier de sortie..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $outputPath
}

# Publication en mode Release avec .NET inclus (self-contained)
Write-Host "📦 Compilation et publication..." -ForegroundColor Green
dotnet publish $projectPath `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $outputPath `
    -p:PublishSingleFile=true `
    -p:PublishTrimmed=false `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugType=None `
    -p:DebugSymbols=false

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✅ Publication réussie !" -ForegroundColor Green
    Write-Host "📂 Emplacement: $outputPath" -ForegroundColor Cyan
    Write-Host "📌 Fichier exécutable: TunRTC.Launcher.exe" -ForegroundColor Cyan
    Write-Host ""
    
    $exePath = Join-Path $outputPath "TunRTC.Launcher.exe"
    if (Test-Path $exePath) {
        $fileSize = [math]::Round((Get-Item $exePath).Length / 1MB, 2)
        Write-Host "💾 Taille: $fileSize MB" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "🎯 Pour lancer l'application:" -ForegroundColor Magenta
        Write-Host "   $exePath" -ForegroundColor White
        Write-Host ""
        
        # Demander si on veut ouvrir le dossier
        $openFolder = Read-Host "Voulez-vous ouvrir le dossier de publication ? (O/N)"
        if ($openFolder -eq "O" -or $openFolder -eq "o") {
            explorer.exe $outputPath
        }
    }
}
else {
    Write-Host ""
    Write-Host "❌ Échec de la publication" -ForegroundColor Red
    Write-Host "Vérifiez les erreurs ci-dessus" -ForegroundColor Yellow
}
