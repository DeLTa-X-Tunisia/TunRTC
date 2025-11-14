/**
 * TunRTC JavaScript SDK
 * Open-source WebRTC client for voice and video communication
 * @version 1.0.0
 */

class TunRTCClient {
    constructor(config) {
        this.apiUrl = config.apiUrl || 'https://localhost:7000/api';
        this.hubUrl = config.hubUrl || 'https://localhost:7000/hubs/signaling';
        this.token = null;
        this.connection = null;
        this.localStream = null;
        this.peerConnections = new Map();
        this.iceServers = [];
        this.sessionId = null;
        this.userId = null;
        this.username = null;
        
        // Event callbacks
        this.onLocalStream = null;
        this.onRemoteStream = null;
        this.onParticipantJoined = null;
        this.onParticipantLeft = null;
        this.onParticipantStatusChanged = null;
        this.onMessage = null;
        this.onError = null;
        this.onConnectionStateChanged = null;
    }

    /**
     * Register a new user
     */
    async register(username, email, password) {
        try {
            const response = await fetch(`${this.apiUrl}/auth/register`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username, email, password })
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Registration failed');
            }

            const data = await response.json();
            this.token = data.token;
            this.username = data.username;
            return data;
        } catch (error) {
            this._handleError('Register failed', error);
            throw error;
        }
    }

    /**
     * Login with credentials
     */
    async login(email, password) {
        try {
            const response = await fetch(`${this.apiUrl}/auth/login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password })
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Login failed');
            }

            const data = await response.json();
            this.token = data.token;
            this.username = data.username;
            return data;
        } catch (error) {
            this._handleError('Login failed', error);
            throw error;
        }
    }

    /**
     * Get ICE servers configuration
     */
    async getIceServers() {
        try {
            const response = await fetch(`${this.apiUrl}/iceservers`, {
                headers: { 'Authorization': `Bearer ${this.token}` }
            });

            if (!response.ok) {
                throw new Error('Failed to get ICE servers');
            }

            const config = await response.json();
            this.iceServers = config.iceServers;
            return this.iceServers;
        } catch (error) {
            this._handleError('Get ICE servers failed', error);
            throw error;
        }
    }

    /**
     * Create a new session
     */
    async createSession(name, maxParticipants = 10, type = 1) {
        try {
            const response = await fetch(`${this.apiUrl}/session/create`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${this.token}`
                },
                body: JSON.stringify({ name, maxParticipants, type })
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Create session failed');
            }

            const session = await response.json();
            return session;
        } catch (error) {
            this._handleError('Create session failed', error);
            throw error;
        }
    }

    /**
     * Join an existing session
     */
    async joinSession(sessionId, audioEnabled = true, videoEnabled = true) {
        try {
            this.sessionId = sessionId;

            // Get ICE servers
            await this.getIceServers();

            // Get local media stream
            this.localStream = await navigator.mediaDevices.getUserMedia({
                audio: audioEnabled,
                video: videoEnabled
            });

            if (this.onLocalStream) {
                this.onLocalStream(this.localStream);
            }

            // Connect to SignalR hub
            await this._connectToHub();

            // Join session via API
            const response = await fetch(`${this.apiUrl}/session/join`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${this.token}`
                },
                body: JSON.stringify({ sessionId })
            });

            if (!response.ok) {
                throw new Error('Join session failed');
            }

            const session = await response.json();

            // Join SignalR group
            await this.connection.invoke('JoinSession', sessionId);

            return session;
        } catch (error) {
            this._handleError('Join session failed', error);
            throw error;
        }
    }

    /**
     * Leave current session
     */
    async leaveSession() {
        try {
            if (!this.sessionId) return;

            // Stop local stream
            if (this.localStream) {
                this.localStream.getTracks().forEach(track => track.stop());
                this.localStream = null;
            }

            // Close all peer connections
            this.peerConnections.forEach(pc => pc.close());
            this.peerConnections.clear();

            // Leave SignalR group
            if (this.connection) {
                await this.connection.invoke('LeaveSession', this.sessionId);
            }

            // Leave via API
            await fetch(`${this.apiUrl}/session/leave`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${this.token}`
                },
                body: JSON.stringify({ sessionId: this.sessionId })
            });

            this.sessionId = null;
        } catch (error) {
            this._handleError('Leave session failed', error);
            throw error;
        }
    }

    /**
     * Update participant status
     */
    async updateStatus(isMuted, isVideoEnabled) {
        try {
            if (!this.sessionId || !this.connection) return;

            // Update local tracks
            if (this.localStream) {
                this.localStream.getAudioTracks().forEach(track => {
                    track.enabled = !isMuted;
                });
                this.localStream.getVideoTracks().forEach(track => {
                    track.enabled = isVideoEnabled;
                });
            }

            // Notify others
            await this.connection.invoke('UpdateStatus', this.sessionId, isMuted, isVideoEnabled);
        } catch (error) {
            this._handleError('Update status failed', error);
            throw error;
        }
    }

    /**
     * Send a chat message
     */
    async sendMessage(message) {
        try {
            if (!this.sessionId || !this.connection) return;

            await this.connection.invoke('SendMessage', this.sessionId, message);
        } catch (error) {
            this._handleError('Send message failed', error);
            throw error;
        }
    }

    /**
     * Connect to SignalR hub
     */
    async _connectToHub() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(this.hubUrl, {
                accessTokenFactory: () => this.token
            })
            .withAutomaticReconnect()
            .build();

        // Handle participant joined
        this.connection.on('ParticipantJoined', async (data) => {
            console.log('Participant joined:', data);
            
            if (this.onParticipantJoined) {
                this.onParticipantJoined(data);
            }

            // Create peer connection and send offer
            await this._createPeerConnection(data.connectionId);
            await this._createOffer(data.connectionId);
        });

        // Handle participant left
        this.connection.on('ParticipantLeft', (data) => {
            console.log('Participant left:', data);
            
            if (this.onParticipantLeft) {
                this.onParticipantLeft(data);
            }

            // Close peer connection
            const pc = this.peerConnections.get(data.connectionId);
            if (pc) {
                pc.close();
                this.peerConnections.delete(data.connectionId);
            }
        });

        // Handle WebRTC offer
        this.connection.on('ReceiveOffer', async (data) => {
            console.log('Received offer from:', data.fromConnectionId);
            await this._handleOffer(data.fromConnectionId, data.offer);
        });

        // Handle WebRTC answer
        this.connection.on('ReceiveAnswer', async (data) => {
            console.log('Received answer from:', data.fromConnectionId);
            await this._handleAnswer(data.fromConnectionId, data.answer);
        });

        // Handle ICE candidate
        this.connection.on('ReceiveIceCandidate', async (data) => {
            await this._handleIceCandidate(data.fromConnectionId, data.candidate);
        });

        // Handle participant status change
        this.connection.on('ParticipantStatusChanged', (data) => {
            if (this.onParticipantStatusChanged) {
                this.onParticipantStatusChanged(data);
            }
        });

        // Handle chat messages
        this.connection.on('ReceiveMessage', (data) => {
            if (this.onMessage) {
                this.onMessage(data);
            }
        });

        // Handle errors
        this.connection.on('Error', (data) => {
            this._handleError('SignalR error', data);
        });

        // Connection state changed
        this.connection.onreconnecting(() => {
            if (this.onConnectionStateChanged) {
                this.onConnectionStateChanged('reconnecting');
            }
        });

        this.connection.onreconnected(() => {
            if (this.onConnectionStateChanged) {
                this.onConnectionStateChanged('connected');
            }
        });

        this.connection.onclose(() => {
            if (this.onConnectionStateChanged) {
                this.onConnectionStateChanged('disconnected');
            }
        });

        await this.connection.start();
        console.log('Connected to SignalR hub');
    }

    /**
     * Create peer connection
     */
    async _createPeerConnection(connectionId) {
        const pc = new RTCPeerConnection({ iceServers: this.iceServers });

        // Add local stream
        if (this.localStream) {
            this.localStream.getTracks().forEach(track => {
                pc.addTrack(track, this.localStream);
            });
        }

        // Handle ICE candidates
        pc.onicecandidate = (event) => {
            if (event.candidate) {
                this.connection.invoke('SendIceCandidate', connectionId, event.candidate);
            }
        };

        // Handle remote stream
        pc.ontrack = (event) => {
            console.log('Received remote track from:', connectionId);
            if (this.onRemoteStream) {
                this.onRemoteStream({
                    connectionId,
                    stream: event.streams[0]
                });
            }
        };

        // Handle connection state
        pc.onconnectionstatechange = () => {
            console.log(`Connection state with ${connectionId}:`, pc.connectionState);
        };

        this.peerConnections.set(connectionId, pc);
        return pc;
    }

    /**
     * Create and send offer
     */
    async _createOffer(connectionId) {
        const pc = this.peerConnections.get(connectionId);
        if (!pc) return;

        const offer = await pc.createOffer();
        await pc.setLocalDescription(offer);
        await this.connection.invoke('SendOffer', connectionId, offer);
    }

    /**
     * Handle incoming offer
     */
    async _handleOffer(connectionId, offer) {
        let pc = this.peerConnections.get(connectionId);
        if (!pc) {
            pc = await this._createPeerConnection(connectionId);
        }

        await pc.setRemoteDescription(new RTCSessionDescription(offer));
        const answer = await pc.createAnswer();
        await pc.setLocalDescription(answer);
        await this.connection.invoke('SendAnswer', connectionId, answer);
    }

    /**
     * Handle incoming answer
     */
    async _handleAnswer(connectionId, answer) {
        const pc = this.peerConnections.get(connectionId);
        if (!pc) return;

        await pc.setRemoteDescription(new RTCSessionDescription(answer));
    }

    /**
     * Handle incoming ICE candidate
     */
    async _handleIceCandidate(connectionId, candidate) {
        const pc = this.peerConnections.get(connectionId);
        if (!pc) return;

        await pc.addIceCandidate(new RTCIceCandidate(candidate));
    }

    /**
     * Handle errors
     */
    _handleError(message, error) {
        console.error(message, error);
        if (this.onError) {
            this.onError({ message, error });
        }
    }

    /**
     * Disconnect and cleanup
     */
    async disconnect() {
        await this.leaveSession();
        
        if (this.connection) {
            await this.connection.stop();
            this.connection = null;
        }

        this.token = null;
        this.username = null;
    }
}

// Export for use in Node.js or browser
if (typeof module !== 'undefined' && module.exports) {
    module.exports = TunRTCClient;
}
