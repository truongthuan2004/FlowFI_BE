namespace FlowFi.AIProcessingService.DTOs;

public sealed record CreateAiProcessingResultDto(
    Guid RequestId,
    decimal? Amount,
    string? TransactionType,
    string? Tag,
    DateTime? TransactionDate,
    string? RawResponse);
