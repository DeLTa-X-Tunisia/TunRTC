# TunRTC Test Plan

## 🧪 Plan de Test Complet

### Phase 1: Tests Backend ✅
- [x] Compilation du serveur
- [ ] Démarrage du serveur
- [ ] Test API Health Check
- [ ] Test Swagger UI

### Phase 2: Tests API REST
- [ ] POST /api/auth/register - Créer un utilisateur
- [ ] POST /api/auth/login - Authentification JWT
- [ ] GET /api/auth/me - Vérifier le token
- [ ] POST /api/session/create - Créer une session
- [ ] POST /api/session/join - Rejoindre une session
- [ ] GET /api/session/{id} - Détails de la session
- [ ] GET /api/iceservers - Configuration STUN/TURN

### Phase 3: Tests SignalR Hub
- [ ] Connexion WebSocket avec JWT
- [ ] JoinSession via SignalR
- [ ] Événement ParticipantJoined
- [ ] Événement ParticipantLeft
- [ ] SendOffer / ReceiveOffer
- [ ] SendAnswer / ReceiveAnswer
- [ ] SendIceCandidate / ReceiveIceCandidate
- [ ] SendMessage / ReceiveMessage

### Phase 4: Tests SDK JavaScript
- [ ] Login via SDK
- [ ] Création de session
- [ ] Join session avec média (simulé)
- [ ] Établissement de peer connection
- [ ] Échange de messages

### Phase 5: Tests Intégration WebRTC
- [ ] Négociation SDP (offer/answer)
- [ ] Échange ICE candidates
- [ ] Connexion peer-to-peer
- [ ] Flux audio/vidéo (simulation)

---

## 📝 Notes de Test

### Compte de test par défaut
- Email: demo@tunrtc.com
- Password: demo123

### Ports requis
- API/Swagger: https://localhost:7000
- SignalR Hub: https://localhost:7000/hubs/signaling
- PostgreSQL: localhost:5432 (si disponible)

---

## ⚠️ Mode Test Sans PostgreSQL

Le serveur utilisera InMemory Database pour les tests si PostgreSQL n'est pas disponible.
