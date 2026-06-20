using FlowFi.AIProcessingService.DTOs;

namespace FlowFi.AIProcessingService.Interface;

public interface IImageTransactionService
{
    Task<ImageTransactionResponseDto> CreateFromImageAsync(
        Guid userId,
        Guid walletId,
        IFormFile image,
        string? mockExtractedText,
        string bearerToken,
        CancellationToken cancellationToken);
}
