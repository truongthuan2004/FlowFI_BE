using FlowFi.FinanceCoreService.DTOs;

namespace FlowFi.FinanceCoreService.Interface;

public interface ITransactionService
{
    Task<CreateTransactionResult> CreateAsync(
        Guid userId,
        Guid walletId,
        CreateTransactionDto request,
        DateTimeOffset? transactionDate,
        CancellationToken cancellationToken = default);
}

