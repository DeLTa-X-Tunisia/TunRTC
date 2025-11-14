using System.Collections.Concurrent;

namespace TunRTC.Server.Services;

public interface IConnectionManager
{
    void AddConnection(string connectionId, int userId, string sessionId);
    void RemoveConnection(string connectionId);
    (int userId, string sessionId)? GetConnectionInfo(string connectionId);
    List<string> GetConnectionsBySession(string sessionId);
    int GetActiveConnectionsCount();
}

public class ConnectionManager : IConnectionManager
{
    private readonly ConcurrentDictionary<string, (int userId, string sessionId)> _connections = new();

    public void AddConnection(string connectionId, int userId, string sessionId)
    {
        _connections[connectionId] = (userId, sessionId);
    }

    public void RemoveConnection(string connectionId)
    {
        _connections.TryRemove(connectionId, out _);
    }

    public (int userId, string sessionId)? GetConnectionInfo(string connectionId)
    {
        if (_connections.TryGetValue(connectionId, out var info))
        {
            return info;
        }
        return null;
    }

    public List<string> GetConnectionsBySession(string sessionId)
    {
        return _connections
            .Where(kvp => kvp.Value.sessionId == sessionId)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    public int GetActiveConnectionsCount()
    {
        return _connections.Count;
    }
}
