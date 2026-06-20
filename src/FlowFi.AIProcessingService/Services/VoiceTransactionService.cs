using FlowFi.AIProcessingService.DTOs;
using FlowFi.AIProcessingService.Interface;

namespace FlowFi.AIProcessingService.Services;

public sealed class VoiceTransactionService(
    IAiProcessingService aiProcessingService,
    IFinanceTransactionCreationService transactionCreationService) : IVoiceTransactionService
{
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
        var financeResult = await transactionCreationService.CreateAsync(
            walletId,
            aiResult.ParsedData,
            bearerToken,
            cancellationToken);

        return new VoiceTransactionResponseDto(
            aiResult.RequestId,
            aiResult.ResultId,
            aiResult.VoiceUrl,
            aiResult.RawText,
            aiResult.ParsedData,
            financeResult.Tag,
            financeResult.TagCreated,
            financeResult.Transaction);
    }
}
