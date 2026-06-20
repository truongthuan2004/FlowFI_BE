using FlowFi.AIProcessingService.DTOs;

namespace FlowFi.AIProcessingService.Interface;

public interface IVoiceTransactionService
{
    Task<VoiceTransactionResponseDto> CreateFromVoiceAsync(
        Guid userId,
        Guid walletId,
        IFormFile voice,
        string? mockTranscribedText,
        string bearerToken,
        CancellationToken cancellationToken);
}
