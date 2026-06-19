namespace FlowFi.FinanceCoreService.DTOs;

public class InternalTransferDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid FromWalletId { get; set; }
    public Guid ToWalletId { get; set; }
    public decimal Amount { get; set; }
    public string Note { get; set; } = string.Empty;
    public string SyncStatus { get; set; } = string.Empty;
    public DateTimeOffset TransferDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

