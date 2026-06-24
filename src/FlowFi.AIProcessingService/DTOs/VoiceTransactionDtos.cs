using System.ComponentModel.DataAnnotations;

namespace FlowFi.AIProcessingService.DTOs;

public sealed class VoiceTransactionUploadDto : IValidatableObject
{
    public Guid WalletId { get; set; }
    public IFormFile Voice { get; set; } = default!;
    public string? MockTranscribedText { get; set; }

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

public sealed record VoiceAiResponseDto(
    Guid RequestId,
    Guid? ResultId,
    string Status,
    string VoiceUrl,
    string RawText,
    ParsedAiTransactionDto ParsedData,
    VoiceAnalysisDto Analysis);

public sealed record VoiceTransactionResponseDto(
    Guid AiRequestId,
    Guid? AiResultId,
    string VoiceUrl,
    string RawText,
    ParsedAiTransactionDto ParsedData,
    VoiceAnalysisDto Analysis,
    FinanceTagDto Tag,
    bool TagCreated,
    FinanceTransactionDto Transaction);
