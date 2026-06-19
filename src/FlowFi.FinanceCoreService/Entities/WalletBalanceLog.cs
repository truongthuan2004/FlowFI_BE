using System.ComponentModel.DataAnnotations;

namespace FlowFi.FinanceCoreService.Entities;

public class WalletBalanceLog
{
    [Key] public Guid Id { get; set; }
    [Required] public Guid WalletId { get; set; }
    public Guid? TransactionId { get; set; }
    public Guid? TransferId { get; set; }
    public decimal OldBalance { get; set; }
    public decimal ChangeAmount { get; set; }
    public decimal NewBalance { get; set; }
    [Required, MaxLength(50)] public string Reason { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public Wallet Wallet { get; set; } = null!;
    public Transaction? Transaction { get; set; }
    public InternalTransfer? Transfer { get; set; }
}

