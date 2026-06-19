using FlowFi.AIProcessingService.DTOs;
using FlowFi.AIProcessingService.Entities;
using FlowFi.AIProcessingService.Interface;

namespace FlowFi.AIProcessingService.Services;

public sealed class AiProcessingService(
    IAiProcessingRepository repository,
    IImageStorageService imageStorageService,
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

    public Task<AiProcessingRequest> CreateRequestAsync(CreateAiProcessingRequestDto dto, CancellationToken cancellationToken)
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

        return repository.AddRequestAsync(request, cancellationToken);
    }

    public Task<AiProcessingResult?> GetResultByRequestIdAsync(Guid requestId, CancellationToken cancellationToken)
    {
        return repository.GetResultByRequestIdAsync(requestId, cancellationToken);
    }

    public Task<AiProcessingResult> CreateResultAsync(CreateAiProcessingResultDto dto, CancellationToken cancellationToken)
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

        return repository.AddResultAsync(result, cancellationToken);
    }

    public async Task<ImageAiResponseDto> ProcessImageAsync(Guid userId, IFormFile image, string? mockExtractedText, CancellationToken cancellationToken)
    {
        await using var imageStream = new MemoryStream();
        await image.CopyToAsync(imageStream, cancellationToken);
        var imageBytes = imageStream.ToArray();

        var request = await repository.AddRequestAsync(new AiProcessingRequest
        {
            UserId = userId,
            InputType = "IMAGE",
            RequestType = "OCR",
            Status = "PROCESSING",
            CreatedAt = DatabaseTimestampNow()
        }, cancellationToken);

        try
        {
            var extractionResult = await aiModelClient.ExtractTextFromImageAsync(
                imageBytes,
                image.ContentType,
                mockExtractedText,
                cancellationToken);
            var parsedData = receiptParserService.Parse(extractionResult.Text);
            var imagePath = await imageStorageService.SaveAsync(
                imageBytes,
                image.FileName,
                image.ContentType,
                request.Id,
                cancellationToken);

            request.InputUrl = imagePath;
            request.Status = "COMPLETED";
            request.CompletedAt = DatabaseTimestampNow();

            await repository.UpdateRequestAsync(request, cancellationToken);

            var result = await repository.AddResultAsync(new AiProcessingResult
            {
                RequestId = request.Id,
                Amount = parsedData.Amount,
                TransactionType = parsedData.TransactionType,
                Tag = parsedData.Tag,
                TransactionDate = parsedData.TransactionDate,
                RawResponse = parsedData.RawText
            }, cancellationToken);

            return new ImageAiResponseDto(
                request.Id,
                result.Id,
                request.Status,
                imagePath,
                parsedData.RawText,
                parsedData);
        }
        catch
        {
            request.Status = "FAILED";
            request.CompletedAt = DatabaseTimestampNow();
            await repository.UpdateRequestAsync(request, cancellationToken);
            throw;
        }
    }

    private static DateTime DatabaseTimestampNow()
    {
        return DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
    }
}
