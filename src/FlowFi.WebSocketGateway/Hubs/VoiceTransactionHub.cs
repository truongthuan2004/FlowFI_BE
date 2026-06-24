using FlowFi.Common.Api;
using FlowFi.WebSocketGateway.Clients;
using FlowFi.WebSocketGateway.Config;
using FlowFi.WebSocketGateway.DTOs;
using FlowFi.WebSocketGateway.Sessions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace FlowFi.WebSocketGateway.Hubs;

[Authorize]
public sealed class VoiceTransactionHub(
    IVoiceSessionStore sessionStore,
    IAiVoiceClient aiVoiceClient,
    IOptions<VoiceRealtimeOptions> options,
    ILogger<VoiceTransactionHub> logger) : Hub
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "audio/mpeg", "audio/mp3", "audio/wav", "audio/x-wav", "audio/mp4",
        "audio/x-m4a", "audio/webm", "audio/ogg"
    };

    private readonly VoiceRealtimeOptions _options = options.Value;

    public async Task<VoiceSessionStartedDto> StartVoiceSession(StartVoiceSessionRequest request)
    {
        var userId = GetUserId();
        ValidateStartRequest(request);
        var session = sessionStore.Create(
            userId,
            request.WalletId,
            Context.ConnectionId,
            Path.GetFileName(request.FileName),
            request.ContentType);
        var result = new VoiceSessionStartedDto(session.Id, session.WalletId, session.StartedAt);
        await Clients.Caller.SendAsync("voice.sessionStarted", result, Context.ConnectionAborted);
        return result;
    }

    public async Task<AudioChunkAcceptedDto> SendAudioChunk(AudioChunkRequest request)
    {
        var userId = GetUserId();
        var session = sessionStore.GetOwned(request.SessionId, userId, Context.ConnectionId);
        byte[] chunk;
        try
        {
            chunk = Convert.FromBase64String(request.AudioBase64);
        }
        catch (FormatException)
        {
            throw new HubException("VOICE_CHUNK_BASE64_INVALID");
        }

        if (chunk.Length == 0 || chunk.Length > _options.MaxChunkSizeBytes)
        {
            throw new HubException("VOICE_CHUNK_SIZE_INVALID");
        }

        VoiceChunkAppendResult appendResult;
        try
        {
            appendResult = session.Append(
                request.Sequence,
                chunk,
                _options.TranscribeEveryChunks,
                _options.MaxSessionSizeBytes);
        }
        catch (InvalidOperationException exception)
        {
            throw new HubException(exception.Message);
        }

        if (appendResult.AudioForTranscription is not null)
        {
            try
            {
                var transcript = await aiVoiceClient.TranscribeAsync(
                    appendResult.AudioForTranscription,
                    session.FileName,
                    session.ContentType,
                    GetBearerToken(),
                    Context.ConnectionAborted);
                session.UpdateTranscript(transcript);
                await Clients.Caller.SendAsync(
                    "voice.partialTranscript",
                    new PartialTranscriptDto(session.Id, transcript, appendResult.ChunkCount),
                    Context.ConnectionAborted);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Partial transcription failed for voice session {SessionId}.", session.Id);
                await Clients.Caller.SendAsync(
                    "voice.partialTranscriptFailed",
                    new VoiceSessionFailedDto(
                        session.Id,
                        "VOICE_PARTIAL_TRANSCRIPTION_FAILED",
                        "Partial transcription failed."),
                    Context.ConnectionAborted);
            }
            finally
            {
                session.CompleteTranscription();
            }
        }

        return new AudioChunkAcceptedDto(
            session.Id,
            request.Sequence,
            appendResult.ReceivedBytes);
    }

    public async Task<object> StopVoiceSession(Guid sessionId)
    {
        var userId = GetUserId();
        VoiceSession session;
        byte[] audio;
        try
        {
            session = sessionStore.TakeOwned(sessionId, userId, Context.ConnectionId);
            audio = session.StopAndGetAudio();
        }
        catch (Exception exception) when (exception is KeyNotFoundException or InvalidOperationException)
        {
            throw new HubException(exception.Message);
        }

        if (audio.Length == 0)
        {
            throw new HubException("VOICE_SESSION_EMPTY");
        }

        try
        {
            var result = await aiVoiceClient.CreateTransactionAsync(
                session.WalletId,
                audio,
                session.FileName,
                session.ContentType,
                GetBearerToken(),
                Context.ConnectionAborted);
            await Clients.Caller.SendAsync("voice.completed", result, Context.ConnectionAborted);
            return result;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Voice transaction creation failed for session {SessionId}.", session.Id);
            await Clients.Caller.SendAsync(
                "voice.failed",
                new VoiceSessionFailedDto(
                    session.Id,
                    "VOICE_TRANSACTION_FAILED",
                    "Voice transaction creation failed."),
                Context.ConnectionAborted);
            throw new HubException("VOICE_TRANSACTION_FAILED");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        sessionStore.RemoveForConnection(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    private Guid GetUserId()
    {
        var userId = Context.User?.UserId() ?? Guid.Empty;
        return userId != Guid.Empty
            ? userId
            : throw new HubException("JWT_USER_ID_INVALID");
    }

    private string GetBearerToken()
    {
        var httpContext = Context.GetHttpContext();
        var authorization = httpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authorization))
        {
            return authorization;
        }

        var accessToken = httpContext?.Request.Query["access_token"].ToString();
        return !string.IsNullOrWhiteSpace(accessToken)
            ? $"Bearer {accessToken}"
            : throw new HubException("JWT_TOKEN_MISSING");
    }

    private static void ValidateStartRequest(StartVoiceSessionRequest request)
    {
        if (request.WalletId == Guid.Empty)
        {
            throw new HubException("WALLET_ID_INVALID");
        }

        if (string.IsNullOrWhiteSpace(request.FileName) ||
            string.IsNullOrWhiteSpace(request.ContentType) ||
            !AllowedContentTypes.Contains(request.ContentType))
        {
            throw new HubException("VOICE_METADATA_INVALID");
        }
    }
}
