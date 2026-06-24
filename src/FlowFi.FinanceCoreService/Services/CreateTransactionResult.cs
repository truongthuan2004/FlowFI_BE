using FlowFi.FinanceCoreService.DTOs;

namespace FlowFi.FinanceCoreService.Services;

public enum CreateTransactionStatus
{
    Success,
    InvalidWalletId,
    InvalidAmount,
    InvalidTransactionType,
    WalletNotFound,
    WalletInactive,
    TagNotFound,
    TagTypeMismatch,
    InsufficientBalance
}

public sealed record CreateTransactionResult(
    CreateTransactionStatus Status,
    TransactionDto? Transaction = null);

