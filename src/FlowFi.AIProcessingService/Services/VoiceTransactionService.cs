using FlowFi.AIProcessingService.DTOs;
using FlowFi.AIProcessingService.Interface;
using FlowFi.AIProcessingService.Config;
using Microsoft.Extensions.Options;

namespace FlowFi.AIProcessingService.Services;

public sealed class VoiceTransactionService(
    IAiProcessingService aiProcessingService,
    IFinanceTransactionCreationService transactionCreationService,
    IOptions<AiProviderOptions> options) : IVoiceTransactionService
{
    private readonly AiProviderOptions _options = options.Value;

    public async Task<VoiceTransactionResponseDto> CreateFromVoiceAsync(
        Guid userId,
        Guid walletId,
        IFormFile voice,
        string? mockTranscribedText,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        var aiResult = await aiProcessingService.ProcessVoiceAsync(
            userId,
            voice,
            mockTranscribedText,
            cancellationToken);
        var analyzedTransaction = aiResult.Analysis.Transactions.Single();
        if (analyzedTransaction.Confidence < _options.MinimumTransactionConfidence)
        {
            throw new InvalidOperationException("AI_LOW_TRANSACTION_CONFIDENCE");
        }

        var financeResult = await transactionCreationService.CreateAsync(
            walletId,
            aiResult.ParsedData,
            analyzedTransaction.Title,
            bearerToken,
            cancellationToken);

        return new VoiceTransactionResponseDto(
            aiResult.RequestId,
            aiResult.ResultId,
            aiResult.VoiceUrl,
            aiResult.RawText,
            aiResult.ParsedData,
            aiResult.Analysis,
            financeResult.Tag,
            financeResult.TagCreated,
            financeResult.Transaction);
    }
}
