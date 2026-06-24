using System.ComponentModel.DataAnnotations;

namespace FlowFi.FinanceCoreService.DTOs;

public class CreateRecurringTransactionDto : IValidatableObject
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid WalletId { get; set; }

    [Required]
    public Guid TagId { get; set; }

    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal Amount { get; set; }

    [Required, RegularExpression("^(INCOME|EXPENSE)$")]
    public string Type { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Note { get; set; } = string.Empty;

    [Required, RegularExpression("^(DAILY|WEEKLY|MONTHLY|YEARLY)$")]
    public string Frequency { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateTimeOffset NextRunAt { get; set; }
    public bool IsActive { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate.HasValue && EndDate.Value < StartDate)
        {
            yield return new ValidationResult(
                "EndDate cannot be earlier than StartDate.",
                new[] { nameof(EndDate) });
        }
    }
}

