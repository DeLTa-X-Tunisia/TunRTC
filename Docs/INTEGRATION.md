# Guide d'Intégration TunRTC

Ce guide vous aidera à intégrer TunRTC dans votre application.

## Table des Matières

1. [Configuration Initiale](#configuration-initiale)
2. [Intégration Web (JavaScript)](#intégration-web-javascript)
3. [Intégration Desktop (C#)](#intégration-desktop-c)
4. [Intégration Mobile](#intégration-mobile)
5. [Cas d'Usage Courants](#cas-dusage-courants)

---

## Configuration Initiale

### 1. Créer un Compte

```javascript
const client = new TunRTCClient({
    apiUrl: 'https://your-server.com/api',
    hubUrl: 'https://your-server.com/hubs/signaling'
});

await client.register('username', 'email@example.com', 'password');
```

### 2. Obtenir un Token

```javascript
const auth = await client.login('email@example.com', 'password');
console.log('Token:', auth.token);
// Sauvegardez ce token pour les prochaines sessions
```

---

## Intégration Web (JavaScript)

### Installation

```html
<!-- Via CDN -->
<script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@8.0.0/dist/browser/signalr.min.js"></script>
<script src="https://cdn.jsdelivr.net/gh/yourorg/tunrtc@latest/ClientSDK/JS/tunrtc-client.js"></script>

<!-- Via NPM -->
<script type="module">
import TunRTCClient from 'tunrtc-client';
</script>
```

### Exemple Complet : Appel Vidéo 1-to-1

```html
<!DOCTYPE html>
<html>
<head>
    <title>TunRTC Video Call</title>
    <style>
        video {
            width: 100%;
            max-width: 640px;
            height: auto;
            border: 2px solid #333;
        }
        #local-video { border-color: green; }
    </style>
</head>
<body>
    <h1>TunRTC Video Call</h1>
    
    <div>
        <button id="start-call">Start Call</button>
        <button id="end-call">End Call</button>
        <button id="toggle-mute">Toggle Mute</button>
        <button id="toggle-video">Toggle Video</button>
    </div>

    <div>
        <video id="local-video" autoplay muted></video>
        <video id="remote-video" autoplay></video>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@8.0.0/dist/browser/signalr.min.js"></script>
    <script src="tunrtc-client.js"></script>
    <script>
        const client = new TunRTCClient({
            apiUrl: 'https://localhost:7000/api',
            hubUrl: 'https://localhost:7000/hubs/signaling'
        });

        let isMuted = false;
        let isVideoEnabled = true;

        // Initialize
        async function init() {
            await client.login('demo@tunrtc.com', 'demo123');
            
            client.onLocalStream = (stream) => {
                document.getElementById('local-video').srcObject = stream;
            };

            client.onRemoteStream = (data) => {
                document.getElementById('remote-video').srcObject = data.stream;
            };
        }

        // Start call
        document.getElementById('start-call').onclick = async () => {
            const session = await client.createSession('My Call', 2);
            await client.joinSession(session.sessionId, true, true);
            console.log('Call started:', session.sessionId);
        };

        // End call
        document.getElementById('end-call').onclick = async () => {
            await client.leaveSession();
        };

        // Toggle mute
        document.getElementById('toggle-mute').onclick = async () => {
            isMuted = !isMuted;
            await client.updateStatus(isMuted, isVideoEnabled);
        };

        // Toggle video
        document.getElementById('toggle-video').onclick = async () => {
            isVideoEnabled = !isVideoEnabled;
            await client.updateStatus(isMuted, isVideoEnabled);
        };

        init();
    </script>
</body>
</html>
```

---

## Intégration Desktop (C#)

### Installation

```bash
dotnet add package TunRTC.Client
```

### Exemple : Application WPF

```csharp
using System.Windows;
using TunRTC.Client;

public partial class MainWindow : Window
{
    private TunRTCClient _client;

    public MainWindow()
    {
        InitializeComponent();
        InitializeClient();
    }

    private void InitializeClient()
    {
        _client = new TunRTCClient(
            "https://localhost:7000/api",
            "https://localhost:7000/hubs/signaling"
        );

        _client.ParticipantJoined += OnParticipantJoined;
        _client.OfferReceived += OnOfferReceived;
        _client.MessageReceived += OnMessageReceived;
    }

    private async void StartCall_Click(object sender, RoutedEventArgs e)
    {
        await _client.LoginAsync("demo@tunrtc.com", "demo123");
        var session = await _client.CreateSessionAsync("WPF Call", 10);
        await _client.JoinSessionAsync(session.SessionId);
        
        StatusLabel.Content = $"Session: {session.SessionId}";
    }

    private async void EndCall_Click(object sender, RoutedEventArgs e)
    {
        await _client.LeaveSessionAsync();
        StatusLabel.Content = "Call ended";
    }

    private void OnParticipantJoined(object sender, ParticipantEvent e)
    {
        Dispatcher.Invoke(() => {
            ParticipantsList.Items.Add($"User {e.UserId} joined");
        });
    }

    private void OnOfferReceived(object sender, SignalingMessage e)
    {
        // Handle WebRTC offer
        // Create answer and send back
    }

    private void OnMessageReceived(object sender, ChatMessage e)
    {
        Dispatcher.Invoke(() => {
            ChatBox.AppendText($"{e.Username}: {e.Message}\n");
        });
    }
}
```

---

## Intégration Mobile

### React Native (JavaScript)

```javascript
import TunRTCClient from 'tunrtc-client';
import { mediaDevices } from 'react-native-webrtc';

const client = new TunRTCClient({
    apiUrl: 'https://your-server.com/api',
    hubUrl: 'https://your-server.com/hubs/signaling'
});

// Get local stream
const stream = await mediaDevices.getUserMedia({
    audio: true,
    video: true
});

// Use with TunRTC
client.onLocalStream = (stream) => {
    // Display in RTCView component
};
```

### Xamarin/MAUI (C#)

Utilisez le même SDK C# que pour desktop, compatible avec Xamarin et .NET MAUI.

---

## Cas d'Usage Courants

### 1. Conférence Multi-Participants

```javascript
// Créer une session pour 50 personnes
const session = await client.createSession('Conference', 50);

// Gérer plusieurs streams
const remoteStreams = new Map();

client.onRemoteStream = (data) => {
    remoteStreams.set(data.connectionId, data.stream);
    
    // Créer un élément vidéo pour chaque participant
    const video = document.createElement('video');
    video.srcObject = data.stream;
    video.autoplay = true;
    document.getElementById('participants').appendChild(video);
};

client.onParticipantLeft = (data) => {
    remoteStreams.delete(data.connectionId);
    // Supprimer l'élément vidéo
};
```

### 2. Audio Only (Appel Vocal)

```javascript
// Rejoindre avec audio seulement
await client.joinSession(sessionId, true, false);

// Ou créer une session audio-only
const session = await client.createSession('Voice Call', 10, 0); // 0 = AudioOnly
```

### 3. Screen Sharing

```javascript
// Obtenir le stream de partage d'écran
const screenStream = await navigator.mediaDevices.getDisplayMedia({
    video: true
});

// Remplacer le stream vidéo local
// Note: Nécessite une modification du SDK pour supporter le changement de stream
```

### 4. Enregistrement de Session

```javascript
// Côté client - enregistrement local
const mediaRecorder = new MediaRecorder(localStream);
const chunks = [];

mediaRecorder.ondataavailable = (e) => {
    chunks.push(e.data);
};

mediaRecorder.onstop = () => {
    const blob = new Blob(chunks, { type: 'video/webm' });
    const url = URL.createObjectURL(blob);
    // Télécharger ou uploader
};

mediaRecorder.start();
// ... plus tard
mediaRecorder.stop();
```

### 5. Chat Intégré

```javascript
// Envoyer un message
await client.sendMessage('Hello everyone!');

// Recevoir des messages
client.onMessage = (data) => {
    console.log(`${data.username}: ${data.message}`);
    displayMessage(data);
};

function displayMessage(msg) {
    const div = document.createElement('div');
    div.textContent = `${msg.username} (${msg.timestamp}): ${msg.message}`;
    document.getElementById('chat').appendChild(div);
}
```

### 6. Gestion de la Qualité

```javascript
// Adapter la qualité en fonction de la bande passante
const constraints = {
    video: {
        width: { ideal: 1280 },
        height: { ideal: 720 },
        frameRate: { ideal: 30, max: 60 }
    },
    audio: {
        echoCancellation: true,
        noiseSuppression: true
    }
};

const stream = await navigator.mediaDevices.getUserMedia(constraints);
```

---

## Bonnes Pratiques

### 1. Gestion des Erreurs

```javascript
client.onError = (error) => {
    console.error('TunRTC Error:', error);
    
    // Afficher à l'utilisateur
    showNotification('Connection error. Please try again.');
    
    // Logger côté serveur
    logError(error);
};

client.onConnectionStateChanged = (state) => {
    console.log('Connection state:', state);
    
    if (state === 'reconnecting') {
        showNotification('Reconnecting...');
    }
};
```

### 2. Nettoyage

```javascript
// Toujours nettoyer à la fermeture
window.addEventListener('beforeunload', async () => {
    await client.leaveSession();
    await client.disconnect();
});
```

### 3. Permissions

```javascript
// Vérifier les permissions avant de rejoindre
async function checkPermissions() {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({
            audio: true,
            video: true
        });
        stream.getTracks().forEach(track => track.stop());
        return true;
    } catch (error) {
        console.error('Permission denied:', error);
        return false;
    }
}

if (await checkPermissions()) {
    await client.joinSession(sessionId, true, true);
} else {
    alert('Please allow camera and microphone access');
}
```

---

## Support

Pour plus d'aide, consultez :
- [Documentation API](../README.md#api-endpoints)
- [Examples](../Examples/)
- [GitHub Issues](https://github.com/yourorg/TunRTC/issues)
