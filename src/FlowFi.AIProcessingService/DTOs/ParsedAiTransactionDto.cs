namespace FlowFi.AIProcessingService.DTOs;

public sealed record ParsedAiTransactionDto(
    decimal? Amount,
    string? TransactionType,
    string? Tag,
    DateTime? TransactionDate,
    string RawText);
