namespace FlowFi.AIProcessingService.DTOs;

public sealed record ImageAiResponseDto(
    Guid RequestId,
    Guid? ResultId,
    string Status,
    string ImageUrl,
    string RawText,
    ParsedAiTransactionDto ParsedData);
