namespace FlowFi.FinanceCoreService.Repositories;

public enum TransactionCreationStatus
{
    Success,
    WalletNotFound,
    WalletInactive,
    TagNotFound,
    TagTypeMismatch,
    InsufficientBalance
}

