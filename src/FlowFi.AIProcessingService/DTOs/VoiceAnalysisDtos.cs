namespace FlowFi.AIProcessingService.DTOs;

public sealed record VoiceAnalysisDto(
    string InputType,
    IReadOnlyList<VoiceAnalyzedTransactionDto> Transactions,
    IReadOnlyList<string> Warnings,
    string RawResponse);

public sealed record VoiceAnalyzedTransactionDto(
    string Title,
    decimal? Amount,
    string? TransactionType,
    string TagName,
    string? Tag,
    string Note,
    DateTime? TransactionDate,
    string RawText,
    decimal Confidence)
{
    public ParsedAiTransactionDto ToParsedTransaction()
        => new(Amount, TransactionType, Tag, TransactionDate, RawText);
}

public sealed class VoiceTranscriptionUploadDto
{
    public IFormFile Voice { get; set; } = default!;
}

public sealed record VoiceTranscriptionResponseDto(string TranscriptText);
