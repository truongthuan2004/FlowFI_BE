using FlowFi.AIProcessingService.DTOs;
using FlowFi.AIProcessingService.Entities;

namespace FlowFi.AIProcessingService.Interface;

public interface IAiProcessingService
{
    Task<IReadOnlyList<AiProcessingRequest>> GetRequestsAsync(Guid? userId, CancellationToken cancellationToken);
    Task<AiProcessingRequest?> GetRequestAsync(Guid id, CancellationToken cancellationToken);
    Task<AiProcessingRequest> CreateRequestAsync(CreateAiProcessingRequestDto dto, CancellationToken cancellationToken);
    Task<AiProcessingResult?> GetResultByRequestIdAsync(Guid requestId, CancellationToken cancellationToken);
    Task<AiProcessingResult> CreateResultAsync(CreateAiProcessingResultDto dto, CancellationToken cancellationToken);
    Task<ImageAiResponseDto> ProcessImageAsync(Guid userId, IFormFile image, string? mockExtractedText, CancellationToken cancellationToken);
    Task<string> TranscribeVoiceAsync(IFormFile voice, CancellationToken cancellationToken);
    Task<VoiceAiResponseDto> ProcessVoiceAsync(Guid userId, IFormFile voice, string? mockTranscribedText, CancellationToken cancellationToken);
}
