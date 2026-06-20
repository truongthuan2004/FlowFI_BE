using FlowFi.AIProcessingService.DTOs;

namespace FlowFi.AIProcessingService.Interface;

public interface IFinanceTransactionCreationService
{
    Task<FinanceTransactionCreationResultDto> CreateAsync(
        Guid walletId,
        ParsedAiTransactionDto parsedData,
        string bearerToken,
        CancellationToken cancellationToken);
}
