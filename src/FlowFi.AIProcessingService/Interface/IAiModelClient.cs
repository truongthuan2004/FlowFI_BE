using FlowFi.AIProcessingService.DTOs;

namespace FlowFi.AIProcessingService.Interface;

public interface IAiModelClient
{
    Task<AiTextExtractionResultDto> ExtractTextFromImageAsync(
        byte[] imageBytes,
        string contentType,
        string? mockExtractedText,
        CancellationToken cancellationToken);
}
