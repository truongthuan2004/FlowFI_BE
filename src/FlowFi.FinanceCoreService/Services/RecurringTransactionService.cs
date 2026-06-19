using FlowFi.FinanceCoreService.DTOs;
using FlowFi.FinanceCoreService.Entities;
using FlowFi.FinanceCoreService.Repositories;

namespace FlowFi.FinanceCoreService.Services;

public class RecurringTransactionService : IRecurringTransactionService
{
    private readonly IRecurringTransactionRepository _repository;

    public RecurringTransactionService(IRecurringTransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<RecurringTransactionDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var transactions = await _repository.GetAllAsync(cancellationToken);
        return transactions.Select(MapToDto).ToList();
    }

    public async Task<RecurringTransactionDto> CreateAsync(
        CreateRecurringTransactionDto request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var transaction = new RecurringTransaction
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            WalletId = request.WalletId,
            TagId = request.TagId,
            Amount = request.Amount,
            Type = request.Type.Trim().ToUpperInvariant(),
            Title = request.Title.Trim(),
            Note = request.Note.Trim(),
            Frequency = request.Frequency.Trim().ToUpperInvariant(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            NextRunAt = request.NextRunAt,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _repository.AddAsync(transaction, cancellationToken);
        return MapToDto(created);
    }

    public async Task<RecurringTransactionDto?> UpdateAsync(
        Guid id,
        UpdateRecurringTransactionDto request,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _repository.GetByIdAsync(id, cancellationToken);
        if (transaction is null)
        {
            return null;
        }

        transaction.UserId = request.UserId;
        transaction.WalletId = request.WalletId;
        transaction.TagId = request.TagId;
        transaction.Amount = request.Amount;
        transaction.Type = request.Type.Trim().ToUpperInvariant();
        transaction.Title = request.Title.Trim();
        transaction.Note = request.Note.Trim();
        transaction.Frequency = request.Frequency.Trim().ToUpperInvariant();
        transaction.StartDate = request.StartDate;
        transaction.EndDate = request.EndDate;
        transaction.NextRunAt = request.NextRunAt;
        transaction.IsActive = request.IsActive;
        transaction.UpdatedAt = DateTimeOffset.UtcNow;

        await _repository.UpdateAsync(transaction, cancellationToken);
        return MapToDto(transaction);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _repository.GetByIdAsync(id, cancellationToken);
        if (transaction is null)
        {
            return false;
        }

        await _repository.DeleteAsync(transaction, cancellationToken);
        return true;
    }

    private static RecurringTransactionDto MapToDto(RecurringTransaction transaction)
    {
        return new RecurringTransactionDto
        {
            Id = transaction.Id,
            UserId = transaction.UserId,
            WalletId = transaction.WalletId,
            TagId = transaction.TagId,
            Amount = transaction.Amount,
            Type = transaction.Type,
            Title = transaction.Title,
            Note = transaction.Note,
            Frequency = transaction.Frequency,
            StartDate = transaction.StartDate,
            EndDate = transaction.EndDate,
            NextRunAt = transaction.NextRunAt,
            IsActive = transaction.IsActive,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt
        };
    }
}

