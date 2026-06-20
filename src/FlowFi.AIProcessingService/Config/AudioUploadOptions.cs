namespace FlowFi.AIProcessingService.Config;

public sealed class AudioUploadOptions
{
    public long MaxFileSizeBytes { get; set; } = 20 * 1024 * 1024;
    public string[] AllowedContentTypes { get; set; } =
    [
        "audio/mpeg",
        "audio/mp3",
        "audio/wav",
        "audio/x-wav",
        "audio/mp4",
        "audio/x-m4a",
        "audio/webm",
        "audio/ogg"
    ];
}
