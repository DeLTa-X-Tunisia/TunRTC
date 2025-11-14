using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TunRTC.Client.Models;

namespace TunRTC.Client;

public class TunRTCClient : IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;
    private readonly string _hubUrl;
    private HubConnection? _hubConnection;
    private string? _token;
    private string? _sessionId;

    // Events
    public event EventHandler<ParticipantEvent>? ParticipantJoined;
    public event EventHandler<ParticipantEvent>? ParticipantLeft;
    public event EventHandler<ParticipantStatusEvent>? ParticipantStatusChanged;
    public event EventHandler<SignalingMessage>? OfferReceived;
    public event EventHandler<SignalingMessage>? AnswerReceived;
    public event EventHandler<SignalingMessage>? IceCandidateReceived;
    public event EventHandler<ChatMessage>? MessageReceived;
    public event EventHandler<Exception>? ErrorOccurred;
    public event EventHandler<string>? ConnectionStateChanged;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
    public string? SessionId => _sessionId;
    public string? Token => _token;

    public TunRTCClient(string apiUrl, string hubUrl)
    {
        _apiUrl = apiUrl.TrimEnd('/');
        _hubUrl = hubUrl;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_apiUrl)
        };
    }

    #region Authentication

    public async Task<AuthResponse> RegisterAsync(string username, string email, string password)
    {
        var request = new RegisterRequest
        {
            Username = username,
            Email = email,
            Password = password
        };

        var response = await _httpClient.PostAsJsonAsync("/auth/register", request);
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>()
            ?? throw new Exception("Failed to deserialize auth response");

        _token = authResponse.Token;
        SetAuthHeader();

        return authResponse;
    }

    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        var request = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var response = await _httpClient.PostAsJsonAsync("/auth/login", request);
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>()
            ?? throw new Exception("Failed to deserialize auth response");

        _token = authResponse.Token;
        SetAuthHeader();

        return authResponse;
    }

    private void SetAuthHeader()
    {
        if (!string.IsNullOrEmpty(_token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);
        }
    }

    #endregion

    #region Session Management

    public async Task<SessionResponse> CreateSessionAsync(string name, int maxParticipants = 10, SessionType type = SessionType.VideoCall)
    {
        EnsureAuthenticated();

        var request = new CreateSessionRequest
        {
            Name = name,
            MaxParticipants = maxParticipants,
            Type = type
        };

        var response = await _httpClient.PostAsJsonAsync("/session/create", request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SessionResponse>()
            ?? throw new Exception("Failed to deserialize session response");
    }

    public async Task<SessionDetailResponse> JoinSessionAsync(string sessionId)
    {
        EnsureAuthenticated();

        _sessionId = sessionId;

        // Join via API
        var request = new JoinSessionRequest { SessionId = sessionId };
        var response = await _httpClient.PostAsJsonAsync("/session/join", request);
        response.EnsureSuccessStatusCode();

        var session = await response.Content.ReadFromJsonAsync<SessionDetailResponse>()
            ?? throw new Exception("Failed to deserialize session response");

        // Connect to SignalR if not already connected
        if (_hubConnection == null || _hubConnection.State == HubConnectionState.Disconnected)
        {
            await ConnectToHubAsync();
        }

        // Join SignalR group
        await _hubConnection!.InvokeAsync("JoinSession", sessionId);

        return session;
    }

    public async Task<SessionDetailResponse> GetSessionAsync(string sessionId)
    {
        EnsureAuthenticated();

        var response = await _httpClient.GetAsync($"/session/{sessionId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SessionDetailResponse>()
            ?? throw new Exception("Failed to deserialize session response");
    }

    public async Task<List<SessionResponse>> GetActiveSessionsAsync()
    {
        EnsureAuthenticated();

        var response = await _httpClient.GetAsync("/session/active");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<SessionResponse>>()
            ?? throw new Exception("Failed to deserialize sessions response");
    }

    public async Task LeaveSessionAsync()
    {
        if (string.IsNullOrEmpty(_sessionId)) return;

        EnsureAuthenticated();

        // Leave SignalR group
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("LeaveSession", _sessionId);
        }

        // Leave via API
        var request = new JoinSessionRequest { SessionId = _sessionId };
        await _httpClient.PostAsJsonAsync("/session/leave", request);

        _sessionId = null;
    }

    public async Task EndSessionAsync(string sessionId)
    {
        EnsureAuthenticated();

        var request = new JoinSessionRequest { SessionId = sessionId };
        var response = await _httpClient.PostAsJsonAsync("/session/end", request);
        response.EnsureSuccessStatusCode();

        _sessionId = null;
    }

    #endregion

    #region ICE Servers

    public async Task<WebRTCConfig> GetIceServersAsync()
    {
        EnsureAuthenticated();

        var response = await _httpClient.GetAsync("/iceservers");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<WebRTCConfig>()
            ?? throw new Exception("Failed to deserialize ICE servers response");
    }

    #endregion

    #region SignalR Hub

    public async Task ConnectToHubAsync()
    {
        EnsureAuthenticated();

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_token);
            })
            .WithAutomaticReconnect()
            .Build();

        RegisterHubHandlers();

        await _hubConnection.StartAsync();
        ConnectionStateChanged?.Invoke(this, "connected");
    }

    private void RegisterHubHandlers()
    {
        if (_hubConnection == null) return;

        _hubConnection.On<ParticipantEvent>("ParticipantJoined", (data) =>
        {
            ParticipantJoined?.Invoke(this, data);
        });

        _hubConnection.On<ParticipantEvent>("ParticipantLeft", (data) =>
        {
            ParticipantLeft?.Invoke(this, data);
        });

        _hubConnection.On<ParticipantStatusEvent>("ParticipantStatusChanged", (data) =>
        {
            ParticipantStatusChanged?.Invoke(this, data);
        });

        _hubConnection.On<SignalingMessage>("ReceiveOffer", (data) =>
        {
            OfferReceived?.Invoke(this, data);
        });

        _hubConnection.On<SignalingMessage>("ReceiveAnswer", (data) =>
        {
            AnswerReceived?.Invoke(this, data);
        });

        _hubConnection.On<SignalingMessage>("ReceiveIceCandidate", (data) =>
        {
            IceCandidateReceived?.Invoke(this, data);
        });

        _hubConnection.On<ChatMessage>("ReceiveMessage", (data) =>
        {
            MessageReceived?.Invoke(this, data);
        });

        _hubConnection.On<object>("Error", (data) =>
        {
            ErrorOccurred?.Invoke(this, new Exception(data.ToString()));
        });

        _hubConnection.Reconnecting += (error) =>
        {
            ConnectionStateChanged?.Invoke(this, "reconnecting");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += (connectionId) =>
        {
            ConnectionStateChanged?.Invoke(this, "connected");
            return Task.CompletedTask;
        };

        _hubConnection.Closed += (error) =>
        {
            ConnectionStateChanged?.Invoke(this, "disconnected");
            return Task.CompletedTask;
        };
    }

    public async Task SendOfferAsync(string targetConnectionId, object offer)
    {
        EnsureHubConnected();
        await _hubConnection!.InvokeAsync("SendOffer", targetConnectionId, offer);
    }

    public async Task SendAnswerAsync(string targetConnectionId, object answer)
    {
        EnsureHubConnected();
        await _hubConnection!.InvokeAsync("SendAnswer", targetConnectionId, answer);
    }

    public async Task SendIceCandidateAsync(string targetConnectionId, object candidate)
    {
        EnsureHubConnected();
        await _hubConnection!.InvokeAsync("SendIceCandidate", targetConnectionId, candidate);
    }

    public async Task UpdateStatusAsync(bool isMuted, bool isVideoEnabled)
    {
        if (string.IsNullOrEmpty(_sessionId)) return;

        EnsureHubConnected();
        await _hubConnection!.InvokeAsync("UpdateStatus", _sessionId, isMuted, isVideoEnabled);
    }

    public async Task SendMessageAsync(string message)
    {
        if (string.IsNullOrEmpty(_sessionId)) return;

        EnsureHubConnected();
        await _hubConnection!.InvokeAsync("SendMessage", _sessionId, message);
    }

    #endregion

    #region Helpers

    private void EnsureAuthenticated()
    {
        if (string.IsNullOrEmpty(_token))
        {
            throw new InvalidOperationException("Not authenticated. Please login first.");
        }
    }

    private void EnsureHubConnected()
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
        {
            throw new InvalidOperationException("SignalR hub is not connected.");
        }
    }

    #endregion

    #region Dispose

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
        }

        _httpClient.Dispose();
    }

    #endregion
}
