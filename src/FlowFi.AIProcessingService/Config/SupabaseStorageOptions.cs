namespace FlowFi.AIProcessingService.Config;

public sealed class SupabaseStorageOptions
{
    public const string SectionName = "SupabaseStorage";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = "Image";
    public string Folder { get; set; } = "ai-processing";
}
