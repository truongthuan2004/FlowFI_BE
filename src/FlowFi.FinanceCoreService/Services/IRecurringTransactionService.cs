using FlowFi.FinanceCoreService.DTOs;

namespace FlowFi.FinanceCoreService.Services;

public interface IRecurringTransactionService
{
    Task<IReadOnlyList<RecurringTransactionDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<RecurringTransactionDto> CreateAsync(
        CreateRecurringTransactionDto request,
        CancellationToken cancellationToken = default);

    Task<RecurringTransactionDto?> UpdateAsync(
        Guid id,
        UpdateRecurringTransactionDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

