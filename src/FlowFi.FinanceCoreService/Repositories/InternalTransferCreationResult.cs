using FlowFi.FinanceCoreService.Entities;

namespace FlowFi.FinanceCoreService.Repositories;

public enum InternalTransferCreationStatus
{
    Success,
    SourceWalletNotFound,
    DestinationWalletNotFound,
    InsufficientBalance
}

public sealed record InternalTransferCreationResult(
    InternalTransferCreationStatus Status,
    InternalTransfer? Transfer = null);

