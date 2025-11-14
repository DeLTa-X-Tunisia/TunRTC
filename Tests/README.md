# 🧪 TunRTC - Tests & Validation

## 📊 Résumé des Tests

✅ **12 Tests Automatisés** - 100% réussite  
✅ **Suite Interactive SignalR** - Prête à l'emploi  
✅ **Tous les endpoints validés** - REST API + WebSocket

---

## 🚀 Démarrage Rapide

### 1. Démarrer le Serveur

```powershell
# Dans le dossier Server/
cd Server
dotnet run
```

Le serveur démarre sur **http://localhost:5000** avec une base de données InMemory et 3 utilisateurs de test pré-créés.

### 2. Comptes de Test Disponibles

- **demo@tunrtc.com** / demo123
- **alice@tunrtc.com** / alice123
- **bob@tunrtc.com** / bob123

---

## 🧪 Tests Automatisés (PowerShell)

### Exécuter Tous les Tests

```powershell
# Test 1: Health Check
Invoke-RestMethod -Uri 'http://localhost:5000/health'

# Test 2: Login
$loginData = @{ email = 'demo@tunrtc.com'; password = 'demo123' } | ConvertTo-Json
$auth = Invoke-RestMethod -Uri 'http://localhost:5000/api/auth/login' -Method Post -Body $loginData -ContentType 'application/json'
$global:token = $auth.token

# Test 3: Get Current User
$headers = @{ Authorization = "Bearer $global:token" }
Invoke-RestMethod -Uri 'http://localhost:5000/api/auth/me' -Method Get -Headers $headers

# Test 4: Create Session
$headers = @{ Authorization = "Bearer $global:token"; 'Content-Type' = 'application/json' }
$body = @{ name = 'Test Video Call'; maxParticipants = 10 } | ConvertTo-Json
$session = Invoke-RestMethod -Uri 'http://localhost:5000/api/session/create' -Method Post -Headers $headers -Body $body
$global:sessionId = $session.sessionId

# Test 5: Join Session
$body = @{ sessionId = $global:sessionId } | ConvertTo-Json
Invoke-RestMethod -Uri 'http://localhost:5000/api/session/join' -Method Post -Headers $headers -Body $body

# Test 6: List Active Sessions
Invoke-RestMethod -Uri 'http://localhost:5000/api/session/active' -Method Get -Headers $headers

# Test 7: Get ICE Servers
Invoke-RestMethod -Uri 'http://localhost:5000/api/iceservers' -Method Get -Headers $headers

# Test 8: Leave Session
$body = @{ sessionId = $global:sessionId } | ConvertTo-Json
Invoke-RestMethod -Uri 'http://localhost:5000/api/session/leave' -Method Post -Headers $headers -Body $body

# Test 9-12: Multi-user tests (voir TEST_RESULTS.md)
```

---

## 🌐 Tests Interactifs SignalR

### Ouvrir la Suite de Tests

```powershell
Invoke-Item "Tests\signalr-test.html"
```

### Scénario de Test Complet

1. **Onglet 1** (Utilisateur Demo):
   - Login avec `demo@tunrtc.com` / `demo123`
   - Cliquer "Connect to SignalR Hub"
   - Cliquer "Create Session"
   - Observer la connexion établie

2. **Onglet 2** (Utilisateur Alice):
   - Login avec `alice@tunrtc.com` / `alice123`
   - Cliquer "Connect to SignalR Hub"
   - Copier l'ID de session depuis l'onglet 1
   - Coller dans "Session ID to join" et cliquer "Join Session"

3. **Tests WebRTC** (dans les deux onglets):
   - Sélectionner l'autre utilisateur dans "Select target user"
   - Cliquer "Send Test Offer" depuis onglet 1
   - Observer "Received WebRTC Offer" dans onglet 2
   - Cliquer "Send Test Answer" depuis onglet 2
   - Observer "Received WebRTC Answer" dans onglet 1
   - Cliquer "Send Test ICE Candidate"
   - Observer les événements ICE dans les logs

4. **Tests Chat**:
   - Taper un message dans "Message to send"
   - Cliquer "Send Message"
   - Observer le message apparaître dans l'autre onglet

5. **Tests Status**:
   - Cliquer "Toggle Mute" dans un onglet
   - Observer le changement de statut dans l'autre onglet
   - Cliquer "Toggle Video"
   - Observer la mise à jour en temps réel

---

## 📋 Résultats Attendus

### ✅ Tous les tests doivent passer avec:
- Status Code 200 (ou 401 pour endpoints protégés sans token)
- Données JSON valides retournées
- Tokens JWT générés correctement
- Sessions créées et joinées avec succès
- SignalR connecté avec authentification
- Événements temps réel reçus
- Multi-utilisateurs fonctionnel

### 📊 Statistiques en Temps Réel
Dans `signalr-test.html`, vous devriez voir:
- **Participants**: Nombre mis à jour automatiquement
- **Messages Received**: Incrémenté à chaque message
- **Signals Received**: Incrémenté pour chaque Offer/Answer/ICE

---

## 🐛 Dépannage

### Le serveur ne démarre pas
```powershell
# Vérifier que le port 5000 n'est pas utilisé
netstat -ano | findstr :5000

# Restaurer les dépendances
cd Server
dotnet restore
dotnet build
```

### Tests automatisés échouent
```powershell
# Vérifier que le serveur est démarré
Invoke-RestMethod -Uri 'http://localhost:5000/health'

# Si erreur 401: Le token a expiré, refaire le login
$loginData = @{ email = 'demo@tunrtc.com'; password = 'demo123' } | ConvertTo-Json
$auth = Invoke-RestMethod -Uri 'http://localhost:5000/api/auth/login' -Method Post -Body $loginData -ContentType 'application/json'
$global:token = $auth.token
```

### SignalR ne se connecte pas dans le navigateur
1. Vérifier que le serveur est démarré sur http://localhost:5000
2. Ouvrir la console du navigateur (F12) pour voir les erreurs
3. Vérifier que le login a réussi (token présent)
4. Vérifier CORS (devrait être "AllowAll" en mode test)

### Pas de participants dans la session
1. S'assurer d'avoir cliqué "Join Session" ET non seulement "Create Session"
2. Vérifier dans les logs SignalR que `UserJoined` event a été reçu
3. Rafraîchir la liste des sessions actives

---

## 📄 Documentation Détaillée

- **TEST_RESULTS.md** - Résultats complets des 12 tests
- **TEST_PLAN.md** - Plan de tests détaillé
- **PROJECT_SUMMARY.md** - Vue d'ensemble complète du projet
- **Documentation/API.md** - Documentation API REST
- **Documentation/SIGNALR.md** - Documentation SignalR

---

## ✅ Checklist de Validation

Avant de déclarer les tests terminés, vérifier que:

- [ ] Serveur démarre sans erreur
- [ ] Health check retourne 200 OK
- [ ] Login génère un JWT token valide
- [ ] Token permet d'accéder aux endpoints protégés
- [ ] Session peut être créée
- [ ] Utilisateur peut joindre une session
- [ ] Sessions actives listées correctement
- [ ] ICE servers retournent 3 serveurs (2 STUN + 1 TURN)
- [ ] SignalR hub accessible (401 sans token)
- [ ] Multi-utilisateurs peuvent joindre la même session
- [ ] SignalR connecte avec JWT dans le navigateur
- [ ] Événements UserJoined/UserLeft fonctionnent
- [ ] WebRTC Offer/Answer/ICE signaling opérationnel
- [ ] Chat temps réel fonctionne
- [ ] Status updates (mute/video) synchronisés

---

## 🎉 Succès!

Si tous ces tests passent, **TunRTC est prêt à l'emploi!** 🚀

Le serveur est maintenant validé pour:
- ✅ Authentification multi-utilisateurs
- ✅ Gestion de sessions WebRTC
- ✅ Signalisation temps réel via SignalR
- ✅ Communication peer-to-peer
- ✅ Chat et status updates

---

**Note**: Ces tests utilisent une base de données InMemory. Pour production, configurer PostgreSQL dans `appsettings.json` et mettre `UseInMemoryDatabase: false`.
