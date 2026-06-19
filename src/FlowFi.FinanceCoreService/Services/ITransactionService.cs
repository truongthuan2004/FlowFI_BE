using FlowFi.FinanceCoreService.DTOs;

namespace FlowFi.FinanceCoreService.Services;

public interface ITransactionService
{
    Task<CreateTransactionResult> CreateAsync(
        CreateTransactionDto request,
        CancellationToken cancellationToken = default);
}

