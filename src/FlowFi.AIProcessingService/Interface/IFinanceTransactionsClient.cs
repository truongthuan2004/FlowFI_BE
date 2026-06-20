using FlowFi.AIProcessingService.DTOs;

namespace FlowFi.AIProcessingService.Interface;

public interface IFinanceTransactionsClient
{
    Task<IReadOnlyList<FinanceTagDto>> ListTagsAsync(
        string transactionType,
        string bearerToken,
        CancellationToken cancellationToken);

    Task<FinanceTagDto> CreateTagAsync(
        string name,
        string transactionType,
        string icon,
        string color,
        string bearerToken,
        CancellationToken cancellationToken);

    Task<FinanceTransactionDto> CreateTransactionAsync(
        Guid walletId,
        Guid tagId,
        ParsedAiTransactionDto parsedData,
        string title,
        string bearerToken,
        CancellationToken cancellationToken);
}
