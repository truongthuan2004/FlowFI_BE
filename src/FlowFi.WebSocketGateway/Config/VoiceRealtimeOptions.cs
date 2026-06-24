namespace FlowFi.WebSocketGateway.Config;

public sealed class VoiceRealtimeOptions
{
    public const string SectionName = "VoiceRealtime";

    public string AiServiceBaseUrl { get; set; } = "http://localhost:5103";
    public int TranscribeEveryChunks { get; set; } = 4;
    public int MaxChunkSizeBytes { get; set; } = 512 * 1024;
    public int MaxSessionSizeBytes { get; set; } = 20 * 1024 * 1024;
    public int SessionTimeoutMinutes { get; set; } = 10;
}
