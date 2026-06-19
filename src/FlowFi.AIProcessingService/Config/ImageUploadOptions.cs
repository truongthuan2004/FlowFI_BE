namespace FlowFi.AIProcessingService.Config;

public sealed class ImageUploadOptions
{
    public string DataFolder { get; set; } = "data/ai-images";
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
    public string[] AllowedContentTypes { get; set; } = ["image/jpeg", "image/png", "image/webp"];
}
