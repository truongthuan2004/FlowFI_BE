using System.ComponentModel.DataAnnotations;

namespace FlowFi.FinanceCoreService.DTOs;

public class CreateWalletDto
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string WalletType { get; set; } = string.Empty;

    [Range(
        typeof(decimal),
        "-9999999999999999.99",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal Balance { get; set; }

    [Required, StringLength(10)]
    public string Currency { get; set; } = string.Empty;
}

