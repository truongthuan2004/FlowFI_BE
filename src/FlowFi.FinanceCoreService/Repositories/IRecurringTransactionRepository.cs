using FlowFi.FinanceCoreService.Entities;

namespace FlowFi.FinanceCoreService.Repositories;

public interface IRecurringTransactionRepository
{
    Task<IReadOnlyList<RecurringTransaction>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<RecurringTransaction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<RecurringTransaction> AddAsync(
        RecurringTransaction transaction,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        RecurringTransaction transaction,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        RecurringTransaction transaction,
        CancellationToken cancellationToken = default);
}

