# TunRTC JavaScript SDK

Easy-to-use JavaScript SDK for integrating WebRTC voice and video into your web applications.

## Installation

### NPM
```bash
npm install tunrtc-client @microsoft/signalr
```

### CDN
```html
<script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@8.0.0/dist/browser/signalr.min.js"></script>
<script src="tunrtc-client.js"></script>
```

## Quick Start

```javascript
// Initialize client
const client = new TunRTCClient({
    apiUrl: 'https://your-server.com/api',
    hubUrl: 'https://your-server.com/hubs/signaling'
});

// Login
await client.login('user@example.com', 'password');

// Create session
const session = await client.createSession('My Video Call', 10);
console.log('Session created:', session.sessionId);

// Or join existing session
await client.joinSession('session-id-here', true, true);

// Handle local stream
client.onLocalStream = (stream) => {
    const videoElement = document.getElementById('local-video');
    videoElement.srcObject = stream;
};

// Handle remote streams
client.onRemoteStream = (data) => {
    const videoElement = document.createElement('video');
    videoElement.srcObject = data.stream;
    videoElement.autoplay = true;
    document.getElementById('remote-videos').appendChild(videoElement);
};

// Mute/unmute
await client.updateStatus(true, true); // muted, video enabled

// Send message
await client.sendMessage('Hello everyone!');

// Leave session
await client.leaveSession();
```

## Complete Example

```html
<!DOCTYPE html>
<html>
<head>
    <title>TunRTC Demo</title>
    <style>
        video { width: 300px; height: 200px; margin: 10px; }
        #local-video { border: 2px solid green; }
    </style>
</head>
<body>
    <h1>TunRTC Video Call</h1>
    
    <div id="login-form">
        <input type="email" id="email" placeholder="Email">
        <input type="password" id="password" placeholder="Password">
        <button onclick="login()">Login</button>
    </div>

    <div id="session-controls" style="display:none;">
        <input type="text" id="session-name" placeholder="Session Name">
        <button onclick="createSession()">Create Session</button>
        <br>
        <input type="text" id="join-session-id" placeholder="Session ID">
        <button onclick="joinSession()">Join Session</button>
        <button onclick="leaveSession()">Leave</button>
        <button onclick="toggleMute()">Toggle Mute</button>
        <button onclick="toggleVideo()">Toggle Video</button>
    </div>

    <div>
        <h3>Local Video</h3>
        <video id="local-video" autoplay muted></video>
    </div>

    <div>
        <h3>Remote Videos</h3>
        <div id="remote-videos"></div>
    </div>

    <div>
        <h3>Chat</h3>
        <div id="messages" style="height: 200px; overflow-y: auto; border: 1px solid #ccc; padding: 10px;"></div>
        <input type="text" id="message-input" placeholder="Type a message">
        <button onclick="sendMessage()">Send</button>
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

        // Event handlers
        client.onLocalStream = (stream) => {
            document.getElementById('local-video').srcObject = stream;
        };

        client.onRemoteStream = (data) => {
            let videoElement = document.getElementById(`remote-${data.connectionId}`);
            if (!videoElement) {
                videoElement = document.createElement('video');
                videoElement.id = `remote-${data.connectionId}`;
                videoElement.autoplay = true;
                document.getElementById('remote-videos').appendChild(videoElement);
            }
            videoElement.srcObject = data.stream;
        };

        client.onParticipantLeft = (data) => {
            const videoElement = document.getElementById(`remote-${data.connectionId}`);
            if (videoElement) {
                videoElement.remove();
            }
        };

        client.onMessage = (data) => {
            const messagesDiv = document.getElementById('messages');
            const messageEl = document.createElement('div');
            messageEl.textContent = `${data.username}: ${data.message}`;
            messagesDiv.appendChild(messageEl);
            messagesDiv.scrollTop = messagesDiv.scrollHeight;
        };

        client.onError = (error) => {
            console.error('Error:', error);
            alert(error.message);
        };

        // Functions
        async function login() {
            const email = document.getElementById('email').value;
            const password = document.getElementById('password').value;
            
            try {
                await client.login(email, password);
                document.getElementById('login-form').style.display = 'none';
                document.getElementById('session-controls').style.display = 'block';
                alert('Logged in successfully!');
            } catch (error) {
                alert('Login failed: ' + error.message);
            }
        }

        async function createSession() {
            const name = document.getElementById('session-name').value;
            try {
                const session = await client.createSession(name, 10);
                alert('Session created! ID: ' + session.sessionId);
                await joinSessionById(session.sessionId);
            } catch (error) {
                alert('Create session failed: ' + error.message);
            }
        }

        async function joinSession() {
            const sessionId = document.getElementById('join-session-id').value;
            await joinSessionById(sessionId);
        }

        async function joinSessionById(sessionId) {
            try {
                await client.joinSession(sessionId, true, true);
                alert('Joined session successfully!');
            } catch (error) {
                alert('Join session failed: ' + error.message);
            }
        }

        async function leaveSession() {
            try {
                await client.leaveSession();
                document.getElementById('remote-videos').innerHTML = '';
                alert('Left session');
            } catch (error) {
                alert('Leave session failed: ' + error.message);
            }
        }

        async function toggleMute() {
            isMuted = !isMuted;
            await client.updateStatus(isMuted, isVideoEnabled);
        }

        async function toggleVideo() {
            isVideoEnabled = !isVideoEnabled;
            await client.updateStatus(isMuted, isVideoEnabled);
        }

        async function sendMessage() {
            const input = document.getElementById('message-input');
            const message = input.value;
            if (message) {
                await client.sendMessage(message);
                input.value = '';
            }
        }
    </script>
</body>
</html>
```

## API Reference

### Constructor

```javascript
const client = new TunRTCClient(config);
```

**Config options:**
- `apiUrl` - Base URL for REST API (default: 'https://localhost:7000/api')
- `hubUrl` - SignalR hub URL (default: 'https://localhost:7000/hubs/signaling')

### Methods

#### `register(username, email, password)`
Register a new user account.

#### `login(email, password)`
Login with credentials.

#### `createSession(name, maxParticipants, type)`
Create a new WebRTC session.

#### `joinSession(sessionId, audioEnabled, videoEnabled)`
Join an existing session with media.

#### `leaveSession()`
Leave the current session.

#### `updateStatus(isMuted, isVideoEnabled)`
Update audio/video status.

#### `sendMessage(message)`
Send a chat message to all participants.

#### `disconnect()`
Disconnect and cleanup all resources.

### Event Callbacks

Set these properties to handle events:

```javascript
client.onLocalStream = (stream) => { /* ... */ };
client.onRemoteStream = (data) => { /* ... */ };
client.onParticipantJoined = (data) => { /* ... */ };
client.onParticipantLeft = (data) => { /* ... */ };
client.onParticipantStatusChanged = (data) => { /* ... */ };
client.onMessage = (data) => { /* ... */ };
client.onError = (error) => { /* ... */ };
client.onConnectionStateChanged = (state) => { /* ... */ };
```

## Browser Support

- Chrome/Edge 74+
- Firefox 66+
- Safari 12.1+
- Opera 62+

## License

MIT
