using System.ComponentModel.DataAnnotations;

namespace FlowFi.FinanceCoreService.Entities;

public class Transaction
{
    [Key] public Guid Id { get; set; }
    [Required] public Guid UserId { get; set; }
    [Required] public Guid WalletId { get; set; }
    [Required] public Guid TagId { get; set; }
    public decimal Amount { get; set; }
    [Required, MaxLength(20)] public string Type { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string Title { get; set; } = string.Empty;
    [Required] public string Note { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string Source { get; set; } = string.Empty;
    [Required, MaxLength(20)] public string SyncStatus { get; set; } = string.Empty;
    public DateTimeOffset TransactionDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Wallet Wallet { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
    public ICollection<WalletBalanceLog> BalanceLogs { get; set; } = new List<WalletBalanceLog>();
}

