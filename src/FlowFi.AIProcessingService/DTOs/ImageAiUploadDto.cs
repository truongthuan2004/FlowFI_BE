namespace FlowFi.AIProcessingService.DTOs;

public sealed class ImageAiUploadDto
{
    public Guid UserId { get; set; }
    public IFormFile Image { get; set; } = default!;
    public string? MockExtractedText { get; set; }
}
