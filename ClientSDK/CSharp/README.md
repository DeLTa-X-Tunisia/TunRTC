# TunRTC C# SDK

Official C# client library for TunRTC WebRTC server.

## Installation

### NuGet Package Manager
```bash
Install-Package TunRTC.Client
```

### .NET CLI
```bash
dotnet add package TunRTC.Client
```

## Quick Start

```csharp
using TunRTC.Client;
using TunRTC.Client.Models;

// Initialize client
var client = new TunRTCClient(
    "https://your-server.com/api",
    "https://your-server.com/hubs/signaling"
);

// Login
var auth = await client.LoginAsync("user@example.com", "password");
Console.WriteLine($"Logged in as {auth.Username}");

// Create session
var session = await client.CreateSessionAsync("My Video Call", 10);
Console.WriteLine($"Session created: {session.SessionId}");

// Or join existing session
await client.JoinSessionAsync("session-id-here");

// Get ICE servers for WebRTC
var iceConfig = await client.GetIceServersAsync();

// Send signaling messages
await client.SendOfferAsync(targetConnectionId, offer);
await client.SendAnswerAsync(targetConnectionId, answer);
await client.SendIceCandidateAsync(targetConnectionId, candidate);

// Update status
await client.UpdateStatusAsync(isMuted: true, isVideoEnabled: true);

// Send chat message
await client.SendMessageAsync("Hello everyone!");

// Leave session
await client.LeaveSessionAsync();

// Cleanup
await client.DisposeAsync();
```

## Complete Example

```csharp
using TunRTC.Client;
using TunRTC.Client.Models;

class Program
{
    static async Task Main(string[] args)
    {
        var client = new TunRTCClient(
            "https://localhost:7000/api",
            "https://localhost:7000/hubs/signaling"
        );

        try
        {
            // Register event handlers
            client.ParticipantJoined += (sender, data) =>
            {
                Console.WriteLine($"Participant joined: {data.ConnectionId}");
            };

            client.ParticipantLeft += (sender, data) =>
            {
                Console.WriteLine($"Participant left: {data.ConnectionId}");
            };

            client.OfferReceived += async (sender, data) =>
            {
                Console.WriteLine($"Offer received from: {data.FromConnectionId}");
                // Handle WebRTC offer and send answer
                // var answer = await CreateAnswer(data.Data);
                // await client.SendAnswerAsync(data.FromConnectionId, answer);
            };

            client.AnswerReceived += (sender, data) =>
            {
                Console.WriteLine($"Answer received from: {data.FromConnectionId}");
                // Handle WebRTC answer
            };

            client.IceCandidateReceived += (sender, data) =>
            {
                Console.WriteLine($"ICE candidate received from: {data.FromConnectionId}");
                // Add ICE candidate to peer connection
            };

            client.MessageReceived += (sender, data) =>
            {
                Console.WriteLine($"{data.Username}: {data.Message}");
            };

            client.ParticipantStatusChanged += (sender, data) =>
            {
                Console.WriteLine($"Participant {data.UserId} - Muted: {data.IsMuted}, Video: {data.IsVideoEnabled}");
            };

            client.ErrorOccurred += (sender, error) =>
            {
                Console.WriteLine($"Error: {error.Message}");
            };

            client.ConnectionStateChanged += (sender, state) =>
            {
                Console.WriteLine($"Connection state: {state}");
            };

            // Login
            Console.Write("Email: ");
            var email = Console.ReadLine();
            Console.Write("Password: ");
            var password = Console.ReadLine();

            var auth = await client.LoginAsync(email!, password!);
            Console.WriteLine($"Logged in as {auth.Username}");

            // Create or join session
            Console.Write("Create new session? (y/n): ");
            var createNew = Console.ReadLine()?.ToLower() == "y";

            string sessionId;
            if (createNew)
            {
                Console.Write("Session name: ");
                var sessionName = Console.ReadLine();
                var session = await client.CreateSessionAsync(sessionName!, 10);
                sessionId = session.SessionId;
                Console.WriteLine($"Session created: {sessionId}");
            }
            else
            {
                Console.Write("Session ID: ");
                sessionId = Console.ReadLine()!;
            }

            // Join session
            var sessionDetail = await client.JoinSessionAsync(sessionId);
            Console.WriteLine($"Joined session: {sessionDetail.Name}");
            Console.WriteLine($"Participants: {sessionDetail.CurrentParticipants}/{sessionDetail.MaxParticipants}");

            // Get ICE servers
            var iceConfig = await client.GetIceServersAsync();
            Console.WriteLine($"ICE servers configured: {iceConfig.IceServers.Count}");

            // Main loop
            Console.WriteLine("\nCommands:");
            Console.WriteLine("  msg <text> - Send message");
            Console.WriteLine("  mute - Toggle mute");
            Console.WriteLine("  video - Toggle video");
            Console.WriteLine("  leave - Leave session");
            Console.WriteLine("  quit - Exit");

            bool isMuted = false;
            bool isVideoEnabled = true;

            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(input)) continue;

                var parts = input.Split(' ', 2);
                var command = parts[0].ToLower();

                switch (command)
                {
                    case "msg":
                        if (parts.Length > 1)
                        {
                            await client.SendMessageAsync(parts[1]);
                        }
                        break;

                    case "mute":
                        isMuted = !isMuted;
                        await client.UpdateStatusAsync(isMuted, isVideoEnabled);
                        Console.WriteLine($"Muted: {isMuted}");
                        break;

                    case "video":
                        isVideoEnabled = !isVideoEnabled;
                        await client.UpdateStatusAsync(isMuted, isVideoEnabled);
                        Console.WriteLine($"Video enabled: {isVideoEnabled}");
                        break;

                    case "leave":
                        await client.LeaveSessionAsync();
                        Console.WriteLine("Left session");
                        return;

                    case "quit":
                        return;

                    default:
                        Console.WriteLine("Unknown command");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            await client.DisposeAsync();
        }
    }
}
```

## API Reference

### Constructor

```csharp
var client = new TunRTCClient(string apiUrl, string hubUrl);
```

### Authentication Methods

- `Task<AuthResponse> RegisterAsync(string username, string email, string password)`
- `Task<AuthResponse> LoginAsync(string email, string password)`

### Session Methods

- `Task<SessionResponse> CreateSessionAsync(string name, int maxParticipants, SessionType type)`
- `Task<SessionDetailResponse> JoinSessionAsync(string sessionId)`
- `Task<SessionDetailResponse> GetSessionAsync(string sessionId)`
- `Task<List<SessionResponse>> GetActiveSessionsAsync()`
- `Task LeaveSessionAsync()`
- `Task EndSessionAsync(string sessionId)`

### WebRTC Methods

- `Task<WebRTCConfig> GetIceServersAsync()`
- `Task SendOfferAsync(string targetConnectionId, object offer)`
- `Task SendAnswerAsync(string targetConnectionId, object answer)`
- `Task SendIceCandidateAsync(string targetConnectionId, object candidate)`

### Communication Methods

- `Task UpdateStatusAsync(bool isMuted, bool isVideoEnabled)`
- `Task SendMessageAsync(string message)`

### Hub Methods

- `Task ConnectToHubAsync()`

### Events

```csharp
event EventHandler<ParticipantEvent> ParticipantJoined;
event EventHandler<ParticipantEvent> ParticipantLeft;
event EventHandler<ParticipantStatusEvent> ParticipantStatusChanged;
event EventHandler<SignalingMessage> OfferReceived;
event EventHandler<SignalingMessage> AnswerReceived;
event EventHandler<SignalingMessage> IceCandidateReceived;
event EventHandler<ChatMessage> MessageReceived;
event EventHandler<Exception> ErrorOccurred;
event EventHandler<string> ConnectionStateChanged;
```

## Models

### SessionType Enum
```csharp
public enum SessionType
{
    AudioOnly,
    VideoCall,
    ScreenShare,
    Mixed
}
```

### SessionStatus Enum
```csharp
public enum SessionStatus
{
    Active,
    Ended,
    Paused
}
```

## Requirements

- .NET 8.0 or higher
- Microsoft.AspNetCore.SignalR.Client 8.0+

## License

MIT
