using System.ComponentModel.DataAnnotations;

namespace FlowFi.AIProcessingService.DTOs;

public sealed class ImageTransactionUploadDto : IValidatableObject
{
    public Guid WalletId { get; set; }
    public IFormFile Image { get; set; } = default!;
    public string? MockExtractedText { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (WalletId == Guid.Empty)
        {
            yield return new ValidationResult(
                "WalletId must be a valid non-empty UUID.",
                [nameof(WalletId)]);
        }
    }
}
