namespace FlowFi.WebSocketGateway.Sessions;

public sealed class VoiceSession(
    Guid id,
    Guid userId,
    Guid walletId,
    string connectionId,
    string fileName,
    string contentType,
    DateTimeOffset startedAt)
{
    private readonly object _sync = new();
    private readonly MemoryStream _audio = new();
    private int _nextSequence;
    private int _chunkCount;
    private bool _transcriptionInProgress;
    private bool _stopped;
    private string _transcriptText = string.Empty;

    public Guid Id { get; } = id;
    public Guid UserId { get; } = userId;
    public Guid WalletId { get; } = walletId;
    public string ConnectionId { get; } = connectionId;
    public string FileName { get; } = fileName;
    public string ContentType { get; } = contentType;
    public DateTimeOffset StartedAt { get; } = startedAt;
    public DateTimeOffset LastActivityAt { get; private set; } = startedAt;
    public string TranscriptText
    {
        get
        {
            lock (_sync)
            {
                return _transcriptText;
            }
        }
    }

    public VoiceChunkAppendResult Append(
        int sequence,
        byte[] chunk,
        int transcribeEveryChunks,
        int maxSessionSizeBytes)
    {
        lock (_sync)
        {
            if (_stopped)
            {
                throw new InvalidOperationException("VOICE_SESSION_STOPPED");
            }

            if (sequence != _nextSequence)
            {
                throw new InvalidOperationException($"VOICE_CHUNK_OUT_OF_ORDER:{_nextSequence}");
            }

            if (_audio.Length + chunk.Length > maxSessionSizeBytes)
            {
                throw new InvalidOperationException("VOICE_SESSION_TOO_LARGE");
            }

            _audio.Write(chunk);
            _nextSequence++;
            _chunkCount++;
            LastActivityAt = DateTimeOffset.UtcNow;

            var shouldTranscribe = !_transcriptionInProgress &&
                _chunkCount % Math.Max(1, transcribeEveryChunks) == 0;
            if (shouldTranscribe)
            {
                _transcriptionInProgress = true;
            }

            return new VoiceChunkAppendResult(
                _audio.Length,
                _chunkCount,
                shouldTranscribe ? _audio.ToArray() : null);
        }
    }

    public void CompleteTranscription()
    {
        lock (_sync)
        {
            _transcriptionInProgress = false;
        }
    }

    public void UpdateTranscript(string transcriptText)
    {
        lock (_sync)
        {
            _transcriptText = transcriptText.Trim();
            LastActivityAt = DateTimeOffset.UtcNow;
        }
    }

    public byte[] StopAndGetAudio()
    {
        lock (_sync)
        {
            if (_stopped)
            {
                throw new InvalidOperationException("VOICE_SESSION_STOPPED");
            }

            _stopped = true;
            LastActivityAt = DateTimeOffset.UtcNow;
            return _audio.ToArray();
        }
    }
}

public sealed record VoiceChunkAppendResult(
    long ReceivedBytes,
    int ChunkCount,
    byte[]? AudioForTranscription);
