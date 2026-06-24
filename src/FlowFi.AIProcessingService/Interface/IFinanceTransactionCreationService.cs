using FlowFi.AIProcessingService.DTOs;

namespace FlowFi.AIProcessingService.Interface;

public interface IFinanceTransactionCreationService
{
    Task<FinanceTransactionCreationResultDto> CreateAsync(
        Guid walletId,
        ParsedAiTransactionDto parsedData,
        string? suggestedTitle,
        string bearerToken,
        CancellationToken cancellationToken);
}
