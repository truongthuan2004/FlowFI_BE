namespace FlowFi.AIProcessingService.DTOs;

public sealed record ImageAnalysisDto(
    string ImageType,
    decimal Confidence,
    IReadOnlyList<ImageAnalyzedTransactionDto> Transactions,
    IReadOnlyList<string> Warnings,
    string RawResponse);

public sealed record ImageAnalyzedTransactionDto(
    string Title,
    decimal? Amount,
    string? TransactionType,
    string TagName,
    string? Tag,
    string Note,
    DateTime? TransactionDate,
    string? MerchantName,
    string RawText,
    decimal Confidence)
{
    public ParsedAiTransactionDto ToParsedTransaction()
        => new(Amount, TransactionType, Tag, TransactionDate, RawText);
}
