namespace FlowFi.AIProcessingService.DTOs;

public sealed record CreateAiProcessingRequestDto(
    Guid UserId,
    string InputType,
    string RequestType,
    string? InputUrl);
