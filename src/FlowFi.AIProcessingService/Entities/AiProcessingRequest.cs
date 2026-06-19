namespace FlowFi.AIProcessingService.Entities;

public sealed class AiProcessingRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string InputType { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;
    public string? InputUrl { get; set; }
    public string Status { get; set; } = "PENDING";
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public AiProcessingResult? Result { get; set; }
}
