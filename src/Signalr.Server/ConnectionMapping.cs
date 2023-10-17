using System.Collections.Concurrent;

namespace Signalr.Server;

internal class ConnectionMapping
{
    private readonly ConcurrentDictionary<string, string> _connectionIdByUserId = new();

    public int GetCount()
    {
        return _connectionIdByUserId.Count;
    }

    public void Add(string key, string newConnectionId)
    {
        _connectionIdByUserId[key] = newConnectionId;
    }

    public bool Remove(string key, string connectionIdToRemove)
    {
        if (!_connectionIdByUserId.TryGetValue(key, out string connectionId) || connectionIdToRemove != connectionId)
        {
            return false;
        }

        return _connectionIdByUserId.TryRemove(key, out _);
    }

    public string GetConnectionIdByUserId(string key)
    {
        if (!_connectionIdByUserId.TryGetValue(key, out string connectionId))
        {
            return null;
        }

        return connectionId;
    }
}