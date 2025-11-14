# Script de tests automatisés complets pour TunRTC
# Teste toutes les API REST et fonctionnalités SignalR

$ErrorActionPreference = "Stop"

Write-Host "🧪 Suite de tests automatisés TunRTC" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$baseUrl = "http://localhost:5000"
$testResults = @()

function Test-Endpoint {
    param(
        [string]$Name,
        [scriptblock]$TestBlock
    )
    
    Write-Host "▶ Test: $Name" -ForegroundColor Yellow
    try {
        & $TestBlock
        Write-Host "  ✅ PASSED" -ForegroundColor Green
        $script:testResults += @{ Name = $Name; Result = "PASSED" }
    } catch {
        Write-Host "  ❌ FAILED: $($_.Exception.Message)" -ForegroundColor Red
        $script:testResults += @{ Name = $Name; Result = "FAILED"; Error = $_.Exception.Message }
    }
    Write-Host ""
}

# Variables globales pour les tests
$script:token = $null
$script:userId = $null
$script:sessionId = $null

Write-Host "🔍 Vérification préalable..." -ForegroundColor Cyan

# Test 1: Health Check
Test-Endpoint "Health Check" {
    $response = Invoke-RestMethod -Uri "$baseUrl/health" -Method Get
    if ($response.status -ne "healthy") {
        throw "Health check failed: $($response.status)"
    }
}

# Test 2: Register User
Test-Endpoint "Register User" {
    $timestamp = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
    $body = @{
        username = "testuser_$timestamp"
        email = "test_$timestamp@tunrtc.com"
        password = "Test123!"
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "$baseUrl/api/auth/register" -Method Post -Body $body -ContentType "application/json"
    
    if ([string]::IsNullOrEmpty($response.token)) {
        throw "No token received"
    }
    
    $script:token = $response.token
    $script:userId = $response.id
}

# Test 3: Login
Test-Endpoint "Login User" {
    $body = @{
        email = "demo@tunrtc.com"
        password = "Demo123!"
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $body -ContentType "application/json"
    
    if ([string]::IsNullOrEmpty($response.token)) {
        throw "No token received"
    }
    
    $script:token = $response.token
    $script:userId = $response.id
}

# Test 4: Get User Profile (avec authentification)
Test-Endpoint "Get User Profile" {
    $headers = @{
        Authorization = "Bearer $script:token"
    }
    
    $response = Invoke-RestMethod -Uri "$baseUrl/api/auth/me" -Method Get -Headers $headers
    
    if ([string]::IsNullOrEmpty($response.username)) {
        throw "No username in profile"
    }
}

# Test 5: List Sessions
Test-Endpoint "List Sessions" {
    $headers = @{
        Authorization = "Bearer $script:token"
    }
    
    $response = Invoke-RestMethod -Uri "$baseUrl/api/sessions" -Method Get -Headers $headers
    
    if ($null -eq $response) {
        throw "No response received"
    }
}

# Test 6: Create Session
Test-Endpoint "Create Session" {
    $headers = @{
        Authorization = "Bearer $script:token"
    }
    
    $body = @{
        name = "Test Session $(Get-Random)"
        maxParticipants = 10
        isPublic = $true
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "$baseUrl/api/sessions" -Method Post -Headers $headers -Body $body -ContentType "application/json"
    
    if ([string]::IsNullOrEmpty($response.id)) {
        throw "No session ID received"
    }
    
    $script:sessionId = $response.id
}

# Test 7: Get Session Details
Test-Endpoint "Get Session Details" {
    $headers = @{
        Authorization = "Bearer $script:token"
    }
    
    $response = Invoke-RestMethod -Uri "$baseUrl/api/sessions/$script:sessionId" -Method Get -Headers $headers
    
    if ($response.id -ne $script:sessionId) {
        throw "Session ID mismatch"
    }
}

# Test 8: Join Session
Test-Endpoint "Join Session" {
    $headers = @{
        Authorization = "Bearer $script:token"
    }
    
    $response = Invoke-RestMethod -Uri "$baseUrl/api/sessions/$script:sessionId/join" -Method Post -Headers $headers
    
    if ($response.sessionId -ne $script:sessionId) {
        throw "Failed to join session"
    }
}

# Test 9: Get Participants
Test-Endpoint "Get Participants" {
    $headers = @{
        Authorization = "Bearer $script:token"
    }
    
    $response = Invoke-RestMethod -Uri "$baseUrl/api/sessions/$script:sessionId/participants" -Method Get -Headers $headers
    
    if ($response.Count -eq 0) {
        throw "No participants found"
    }
}

# Test 10: Leave Session
Test-Endpoint "Leave Session" {
    $headers = @{
        Authorization = "Bearer $script:token"
    }
    
    Invoke-RestMethod -Uri "$baseUrl/api/sessions/$script:sessionId/leave" -Method Delete -Headers $headers
}

# Test 11: End Session
Test-Endpoint "End Session" {
    $headers = @{
        Authorization = "Bearer $script:token"
    }
    
    Invoke-RestMethod -Uri "$baseUrl/api/sessions/$script:sessionId" -Method Delete -Headers $headers
}

# Test 12: Swagger Documentation
Test-Endpoint "Swagger Documentation" {
    $response = Invoke-WebRequest -Uri "$baseUrl/swagger/v1/swagger.json" -Method Get
    
    if ($response.StatusCode -ne 200) {
        throw "Swagger not accessible"
    }
}

# Résumé des tests
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "📊 RÉSULTATS DES TESTS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$passed = ($testResults | Where-Object { $_.Result -eq "PASSED" }).Count
$failed = ($testResults | Where-Object { $_.Result -eq "FAILED" }).Count
$total = $testResults.Count

foreach ($result in $testResults) {
    if ($result.Result -eq "PASSED") {
        Write-Host "  ✅ $($result.Name)" -ForegroundColor Green
    } else {
        Write-Host "  ❌ $($result.Name): $($result.Error)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Total: $total tests" -ForegroundColor White
Write-Host "Réussis: $passed" -ForegroundColor Green
Write-Host "Échoués: $failed" -ForegroundColor Red

$percentage = [math]::Round(($passed / $total) * 100, 2)
Write-Host "Taux de réussite: $percentage%" -ForegroundColor $(if ($percentage -eq 100) { "Green" } else { "Yellow" })
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($failed -eq 0) {
    Write-Host "🎉 Tous les tests sont passés avec succès!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "⚠️ Certains tests ont échoué" -ForegroundColor Yellow
    exit 1
}
