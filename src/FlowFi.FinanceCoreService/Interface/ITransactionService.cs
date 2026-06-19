using FlowFi.FinanceCoreService.DTOs;

namespace FlowFi.FinanceCoreService.Interface;

public interface ITransactionService
{
    Task<CreateTransactionResult> CreateAsync(
        CreateTransactionDto request,
        CancellationToken cancellationToken = default);
}

