namespace FlowFi.AnalyticsService.Entities;

public sealed class TransactionSnapshot
{
    public Guid TransactionId { get; set; }
    public Guid UserId { get; set; }
    public Guid WalletId { get; set; }
    public Guid? TagId { get; set; }
    public string? TagName { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "VND";
    public DateTimeOffset TransactionDate { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
