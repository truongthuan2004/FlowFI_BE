namespace FlowFi.AIProcessingService.Entities;

public sealed class AiProcessingResult
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public decimal? Amount { get; set; }
    public string? TransactionType { get; set; }
    public string? Tag { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string? RawResponse { get; set; }

    public AiProcessingRequest? Request { get; set; }
}
