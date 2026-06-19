namespace FlowFi.FinanceCoreService.DTOs;

public class TransactionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid WalletId { get; set; }
    public Guid TagId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string SyncStatus { get; set; } = string.Empty;
    public DateTimeOffset TransactionDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

