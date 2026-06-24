using FlowFi.AIProcessingService.DTOs;

namespace FlowFi.AIProcessingService.Interface;

public interface IAiModelClient
{
    Task<AiTextExtractionResultDto> ExtractTextFromImageAsync(
        byte[] imageBytes,
        string contentType,
        string? mockExtractedText,
        CancellationToken cancellationToken);

    Task<AiTextExtractionResultDto> TranscribeVoiceAsync(
        byte[] audioBytes,
        string fileName,
        string contentType,
        string? mockTranscribedText,
        CancellationToken cancellationToken);

    Task<AiTextExtractionResultDto> AnalyzeVoiceTransactionAsync(
        string transcriptText,
        CancellationToken cancellationToken);
}
