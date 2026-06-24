using System.Collections.Concurrent;

namespace FlowFi.WebSocketGateway.Sessions;

public interface IVoiceSessionStore
{
    VoiceSession Create(Guid userId, Guid walletId, string connectionId, string fileName, string contentType);
    VoiceSession GetOwned(Guid sessionId, Guid userId, string connectionId);
    VoiceSession TakeOwned(Guid sessionId, Guid userId, string connectionId);
    void RemoveForConnection(string connectionId);
    int RemoveExpired(TimeSpan timeout);
}

public sealed class VoiceSessionStore : IVoiceSessionStore
{
    private readonly ConcurrentDictionary<Guid, VoiceSession> _sessions = new();

    public VoiceSession Create(
        Guid userId,
        Guid walletId,
        string connectionId,
        string fileName,
        string contentType)
    {
        var session = new VoiceSession(
            Guid.NewGuid(),
            userId,
            walletId,
            connectionId,
            fileName,
            contentType,
            DateTimeOffset.UtcNow);
        _sessions[session.Id] = session;
        return session;
    }

    public VoiceSession GetOwned(Guid sessionId, Guid userId, string connectionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session) ||
            session.UserId != userId ||
            !string.Equals(session.ConnectionId, connectionId, StringComparison.Ordinal))
        {
            throw new KeyNotFoundException("VOICE_SESSION_NOT_FOUND");
        }

        return session;
    }

    public VoiceSession TakeOwned(Guid sessionId, Guid userId, string connectionId)
    {
        var session = GetOwned(sessionId, userId, connectionId);
        if (!_sessions.TryRemove(new KeyValuePair<Guid, VoiceSession>(sessionId, session)))
        {
            throw new KeyNotFoundException("VOICE_SESSION_NOT_FOUND");
        }

        return session;
    }

    public void RemoveForConnection(string connectionId)
    {
        foreach (var session in _sessions.Values.Where(item => item.ConnectionId == connectionId))
        {
            _sessions.TryRemove(new KeyValuePair<Guid, VoiceSession>(session.Id, session));
        }
    }

    public int RemoveExpired(TimeSpan timeout)
    {
        var threshold = DateTimeOffset.UtcNow - timeout;
        var removed = 0;
        foreach (var session in _sessions.Values.Where(item => item.LastActivityAt < threshold))
        {
            if (_sessions.TryRemove(new KeyValuePair<Guid, VoiceSession>(session.Id, session)))
            {
                removed++;
            }
        }

        return removed;
    }
}
