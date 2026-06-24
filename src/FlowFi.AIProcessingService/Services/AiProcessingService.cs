using FlowFi.AIProcessingService.DTOs;
using FlowFi.AIProcessingService.Entities;
using FlowFi.AIProcessingService.Interface;

namespace FlowFi.AIProcessingService.Services;

public sealed class AiProcessingService(
    IAiProcessingRepository repository,
    IUnitOfWork unitOfWork,
    IFileStorageService fileStorageService,
    IAiModelClient aiModelClient,
    IReceiptParserService receiptParserService) : IAiProcessingService
{
    public Task<IReadOnlyList<AiProcessingRequest>> GetRequestsAsync(Guid? userId, CancellationToken cancellationToken)
    {
        return repository.GetRequestsAsync(userId, cancellationToken);
    }

    public Task<AiProcessingRequest?> GetRequestAsync(Guid id, CancellationToken cancellationToken)
    {
        return repository.GetRequestAsync(id, cancellationToken);
    }

    public async Task<AiProcessingRequest> CreateRequestAsync(CreateAiProcessingRequestDto dto, CancellationToken cancellationToken)
    {
        var request = new AiProcessingRequest
        {
            UserId = dto.UserId,
            InputType = dto.InputType,
            RequestType = dto.RequestType,
            InputUrl = dto.InputUrl,
            Status = "PENDING",
            CreatedAt = DatabaseTimestampNow()
        };

        repository.AddRequest(request);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return request;
    }

    public Task<AiProcessingResult?> GetResultByRequestIdAsync(Guid requestId, CancellationToken cancellationToken)
    {
        return repository.GetResultByRequestIdAsync(requestId, cancellationToken);
    }

    public async Task<AiProcessingResult> CreateResultAsync(CreateAiProcessingResultDto dto, CancellationToken cancellationToken)
    {
        var result = new AiProcessingResult
        {
            RequestId = dto.RequestId,
            Amount = dto.Amount,
            TransactionType = dto.TransactionType,
            Tag = dto.Tag,
            TransactionDate = dto.TransactionDate,
            RawResponse = dto.RawResponse
        };

        repository.AddResult(result);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<ImageAiResponseDto> ProcessImageAsync(Guid userId, IFormFile image, string? mockExtractedText, CancellationToken cancellationToken)
    {
        await using var imageStream = new MemoryStream();
        await image.CopyToAsync(imageStream, cancellationToken);
        var imageBytes = imageStream.ToArray();

        var request = new AiProcessingRequest
        {
            UserId = userId,
            InputType = "IMAGE",
            RequestType = "OCR",
            Status = "PROCESSING",
            CreatedAt = DatabaseTimestampNow()
        };

        repository.AddRequest(request);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var extractionResult = await aiModelClient.ExtractTextFromImageAsync(
                imageBytes,
                image.ContentType,
                mockExtractedText,
                cancellationToken);
            var analysis = receiptParserService.ParseImageAnalysis(extractionResult.Text);
            var imagePath = await fileStorageService.SaveAsync(
                imageBytes,
                image.FileName,
                image.ContentType,
                request.Id,
                "images",
                cancellationToken);

            request.InputUrl = imagePath;
            request.Status = "COMPLETED";
            request.CompletedAt = DatabaseTimestampNow();

            repository.UpdateRequest(request);

            var result = new AiProcessingResult
            {
                RequestId = request.Id,
                Amount = SumAmounts(analysis.Transactions),
                TransactionType = SingleValueOrNull(analysis.Transactions.Select(item => item.TransactionType)),
                Tag = SingleValueOrNull(analysis.Transactions.Select(item => item.Tag)),
                TransactionDate = analysis.Transactions
                    .Where(item => item.TransactionDate.HasValue)
                    .Select(item => item.TransactionDate)
                    .FirstOrDefault(),
                RawResponse = analysis.RawResponse
            };

            repository.AddResult(result);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new ImageAiResponseDto(
                request.Id,
                result.Id,
                request.Status,
                imagePath,
                analysis);
        }
        catch
        {
            request.Status = "FAILED";
            request.CompletedAt = DatabaseTimestampNow();
            repository.UpdateRequest(request);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    public async Task<VoiceAiResponseDto> ProcessVoiceAsync(
        Guid userId,
        IFormFile voice,
        string? mockTranscribedText,
        CancellationToken cancellationToken)
    {
        await using var audioStream = new MemoryStream();
        await voice.CopyToAsync(audioStream, cancellationToken);
        var audioBytes = audioStream.ToArray();

        var request = new AiProcessingRequest
        {
            UserId = userId,
            InputType = "AUDIO",
            RequestType = "VOICE_TO_TRANSACTION",
            Status = "PENDING",
            CreatedAt = DatabaseTimestampNow()
        };

        repository.AddRequest(request);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            request.Status = "PROCESSING";
            repository.UpdateRequest(request);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var transcription = await aiModelClient.TranscribeVoiceAsync(
                audioBytes,
                voice.FileName,
                voice.ContentType,
                mockTranscribedText,
                cancellationToken);
            var analysisResult = await aiModelClient.AnalyzeVoiceTransactionAsync(
                transcription.Text,
                cancellationToken);
            var analysis = receiptParserService.ParseVoiceAnalysis(analysisResult.Text);
            var analyzedTransaction = analysis.Transactions.SingleOrDefault()
                ?? throw new InvalidOperationException("AI_NO_FINANCIAL_TRANSACTION_FOUND");
            var parsedData = analyzedTransaction.ToParsedTransaction();
            var voiceUrl = await fileStorageService.SaveAsync(
                audioBytes,
                voice.FileName,
                voice.ContentType,
                request.Id,
                "voices",
                cancellationToken);

            request.InputUrl = voiceUrl;
            request.Status = "COMPLETED";
            request.CompletedAt = DatabaseTimestampNow();
            repository.UpdateRequest(request);

            var result = new AiProcessingResult
            {
                RequestId = request.Id,
                Amount = parsedData.Amount,
                TransactionType = parsedData.TransactionType,
                Tag = parsedData.Tag,
                TransactionDate = parsedData.TransactionDate,
                RawResponse = analysis.RawResponse
            };

            repository.AddResult(result);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new VoiceAiResponseDto(
                request.Id,
                result.Id,
                request.Status,
                voiceUrl,
                transcription.Text.Trim(),
                parsedData,
                analysis);
        }
        catch
        {
            request.Status = "FAILED";
            request.CompletedAt = DatabaseTimestampNow();
            repository.UpdateRequest(request);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    private static DateTime DatabaseTimestampNow()
    {
        return DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
    }

    public async Task<string> TranscribeVoiceAsync(
        IFormFile voice,
        CancellationToken cancellationToken)
    {
        await using var audioStream = new MemoryStream();
        await voice.CopyToAsync(audioStream, cancellationToken);
        var transcription = await aiModelClient.TranscribeVoiceAsync(
            audioStream.ToArray(),
            voice.FileName,
            voice.ContentType,
            null,
            cancellationToken);
        return transcription.Text.Trim();
    }

    private static decimal? SumAmounts(IReadOnlyList<ImageAnalyzedTransactionDto> transactions)
    {
        var amounts = transactions
            .Where(item => item.Amount.HasValue)
            .Select(item => item.Amount!.Value)
            .ToArray();
        return amounts.Length == 0 ? null : amounts.Sum();
    }

    private static string? SingleValueOrNull(IEnumerable<string?> values)
    {
        var distinctValues = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return distinctValues.Length == 1 ? distinctValues[0] : null;
    }
}
