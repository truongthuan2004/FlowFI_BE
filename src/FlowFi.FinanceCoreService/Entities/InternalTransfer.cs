using System.ComponentModel.DataAnnotations;

namespace FlowFi.FinanceCoreService.Entities;

public class InternalTransfer
{
    [Key] public Guid Id { get; set; }
    [Required] public Guid UserId { get; set; }
    [Required] public Guid FromWalletId { get; set; }
    [Required] public Guid ToWalletId { get; set; }
    public decimal Amount { get; set; }
    [Required] public string Note { get; set; } = string.Empty;
    [Required, MaxLength(20)] public string SyncStatus { get; set; } = string.Empty;
    public DateTimeOffset TransferDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Wallet FromWallet { get; set; } = null!;
    public Wallet ToWallet { get; set; } = null!;
    public ICollection<WalletBalanceLog> BalanceLogs { get; set; } = new List<WalletBalanceLog>();
}

