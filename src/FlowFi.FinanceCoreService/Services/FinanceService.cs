using FlowFi.FinanceCoreService.Entities;
using FlowFi.FinanceCoreService.Interfaces;
using FlowFi.Contracts.Events;
using FlowFi.EventBus.Messaging;

namespace FlowFi.FinanceCoreService.Services;

public sealed class FinanceService(IFinanceRepository repository, RabbitMqPublisher publisher) : IFinanceService
{
    public Task<IReadOnlyList<Wallet>> GetWalletsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return repository.GetWalletsAsync(userId, cancellationToken);
    }

    public Task<Wallet> CreateWalletAsync(Wallet wallet, CancellationToken cancellationToken)
    {
        var entity = wallet with { Id = wallet.Id == Guid.Empty ? Guid.NewGuid() : wallet.Id };
        return repository.AddWalletAsync(entity, cancellationToken);
    }

    public Task<IReadOnlyList<Transaction>> GetTransactionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return repository.GetTransactionsAsync(userId, cancellationToken);
    }

    public async Task<Transaction> CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        var entity = transaction with
        {
            Id = transaction.Id == Guid.Empty ? Guid.NewGuid() : transaction.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var created = await repository.AddTransactionAsync(entity, cancellationToken);
        await publisher.PublishAsync(
            "transaction.created",
            new TransactionCreated(created.UserId, created.Id, created.Amount, created.Currency),
            cancellationToken);

        return created;
    }
}
