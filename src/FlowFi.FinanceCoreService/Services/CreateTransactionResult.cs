using FlowFi.FinanceCoreService.DTOs;

namespace FlowFi.FinanceCoreService.Services;

public enum CreateTransactionStatus
{
    Success,
    WalletNotFound,
    TagNotFound
}

public sealed record CreateTransactionResult(
    CreateTransactionStatus Status,
    TransactionDto? Transaction = null);

