namespace FlowFi.AIProcessingService.DTOs;

public sealed record FinanceTagDto(
    Guid Id,
    string Name,
    string TransactionType,
    string Icon,
    string Color);

public sealed record FinanceTransactionDto(
    Guid Id,
    Guid WalletId,
    Guid TagId,
    decimal Amount,
    string TransactionType,
    string Title,
    string Note,
    string Source,
    string SyncStatus,
    DateTimeOffset TransactionDate,
    DateTimeOffset CreatedAt);

public sealed record FinanceTransactionCreationResultDto(
    FinanceTagDto Tag,
    bool TagCreated,
    FinanceTransactionDto Transaction);

public sealed record ImageTransactionResponseDto(
    Guid AiRequestId,
    Guid? AiResultId,
    string ImageUrl,
    string RawText,
    ParsedAiTransactionDto ParsedData,
    FinanceTagDto Tag,
    bool TagCreated,
    FinanceTransactionDto Transaction);
