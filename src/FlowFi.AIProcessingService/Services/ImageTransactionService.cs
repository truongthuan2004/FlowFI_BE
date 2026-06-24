using FlowFi.AIProcessingService.Config;
using FlowFi.AIProcessingService.DTOs;
using FlowFi.AIProcessingService.Interface;
using Microsoft.Extensions.Options;

namespace FlowFi.AIProcessingService.Services;

public sealed class ImageTransactionService(
    IAiProcessingService aiProcessingService,
    IFinanceTransactionCreationService transactionCreationService,
    IOptions<AiProviderOptions> options) : IImageTransactionService
{
    private readonly AiProviderOptions _options = options.Value;

    public async Task<ImageTransactionResponseDto> CreateFromImageAsync(
        Guid userId,
        Guid walletId,
        IFormFile image,
        string? mockExtractedText,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        var aiResult = await aiProcessingService.ProcessImageAsync(
            userId,
            image,
            mockExtractedText,
            cancellationToken);
        ValidateAnalysis(aiResult.Analysis);

        var createdTransactions = new List<FinanceTransactionCreationResultDto>();
        foreach (var analyzedTransaction in aiResult.Analysis.Transactions)
        {
            ValidateTransaction(analyzedTransaction);
            var financeResult = await transactionCreationService.CreateAsync(
                walletId,
                analyzedTransaction.ToParsedTransaction(),
                analyzedTransaction.Title,
                bearerToken,
                cancellationToken);
            createdTransactions.Add(financeResult);
        }

        return new ImageTransactionResponseDto(
            aiResult.RequestId,
            aiResult.ResultId,
            aiResult.ImageUrl,
            aiResult.Analysis,
            createdTransactions);
    }

    private void ValidateAnalysis(ImageAnalysisDto analysis)
    {
        if (analysis.ImageType == "UNKNOWN" || analysis.Transactions.Count == 0)
        {
            throw new InvalidOperationException("AI_NO_FINANCIAL_TRANSACTION_FOUND");
        }

        if (analysis.Confidence < _options.MinimumImageConfidence)
        {
            throw new InvalidOperationException("AI_LOW_IMAGE_CONFIDENCE");
        }
    }

    private void ValidateTransaction(ImageAnalyzedTransactionDto transaction)
    {
        if (transaction.Confidence < _options.MinimumTransactionConfidence)
        {
            throw new InvalidOperationException("AI_LOW_TRANSACTION_CONFIDENCE");
        }

        if (!transaction.Amount.HasValue || transaction.Amount <= 0)
        {
            throw new InvalidOperationException("AI_AMOUNT_NOT_FOUND");
        }

        if (transaction.TransactionType is not ("INCOME" or "EXPENSE"))
        {
            throw new InvalidOperationException("AI_TRANSACTION_TYPE_NOT_FOUND");
        }
    }
}
