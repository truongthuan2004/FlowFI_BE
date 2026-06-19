using FlowFi.FinanceCoreService.DTOs;

namespace FlowFi.FinanceCoreService.Services;

public enum CreateInternalTransferStatus
{
    Success,
    SameWallet,
    SourceWalletNotFound,
    DestinationWalletNotFound,
    InsufficientBalance
}

public sealed record CreateInternalTransferResult(
    CreateInternalTransferStatus Status,
    InternalTransferDto? Transfer = null);

