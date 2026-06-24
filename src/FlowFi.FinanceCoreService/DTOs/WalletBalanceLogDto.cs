namespace FlowFi.FinanceCoreService.DTOs;

public class WalletBalanceLogDto
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public Guid? TransactionId { get; set; }
    public Guid? TransferId { get; set; }
    public decimal OldBalance { get; set; }
    public decimal ChangeAmount { get; set; }
    public decimal NewBalance { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
