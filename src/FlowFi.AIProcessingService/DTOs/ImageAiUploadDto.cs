namespace FlowFi.AIProcessingService.DTOs;

public sealed class ImageAiUploadDto
{
    public IFormFile Image { get; set; } = default!;
    public string? MockExtractedText { get; set; }
}
