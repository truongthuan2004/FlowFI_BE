using System.ComponentModel.DataAnnotations;

namespace FlowFi.FinanceCoreService.DTOs;

public class UpdateWalletDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string WalletType { get; set; } = string.Empty;

    [Range(typeof(decimal), "-9999999999999999.99", "9999999999999999.99")]
    public decimal Balance { get; set; }

    [Required, StringLength(10)]
    public string Currency { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}

