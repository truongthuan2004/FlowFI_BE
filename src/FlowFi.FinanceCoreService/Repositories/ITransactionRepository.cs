using FlowFi.FinanceCoreService.Entities;

namespace FlowFi.FinanceCoreService.Repositories;

public interface ITransactionRepository
{
    Task<TransactionCreationResult> CreateAsync(
        Transaction transaction,
        decimal balanceChange,
        CancellationToken cancellationToken = default);
}

