namespace FlowFi.WebSocketGateway.DTOs;

public sealed record StartVoiceSessionRequest(
    Guid WalletId,
    string FileName,
    string ContentType);

public sealed record VoiceSessionStartedDto(
    Guid SessionId,
    Guid WalletId,
    DateTimeOffset StartedAt);

public sealed record AudioChunkRequest(
    Guid SessionId,
    int Sequence,
    string AudioBase64);

public sealed record AudioChunkAcceptedDto(
    Guid SessionId,
    int Sequence,
    long ReceivedBytes);

public sealed record PartialTranscriptDto(
    Guid SessionId,
    string TranscriptText,
    int ProcessedChunks);

public sealed record VoiceSessionFailedDto(
    Guid SessionId,
    string ErrorCode,
    string Message);
