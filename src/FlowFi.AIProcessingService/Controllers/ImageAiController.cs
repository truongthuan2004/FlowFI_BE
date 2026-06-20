using FlowFi.AIProcessingService.Config;
using FlowFi.AIProcessingService.DTOs;
using FlowFi.AIProcessingService.Interface;
using FlowFi.Common.Api;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FlowFi.AIProcessingService.Controllers;

[Authorize]
[ApiController]
[Route("api/ai-processing/images")]
public sealed class ImageAiController(
    IAiProcessingService aiProcessingService,
    IImageTransactionService imageTransactionService,
    IOptions<ImageUploadOptions> imageUploadOptions) : ControllerBase
{
    private readonly ImageUploadOptions _imageUploadOptions = imageUploadOptions.Value;

    [HttpPost("extract-text")]
    [HttpPost("ocr")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> ExtractText(
        [FromForm] ImageAiUploadDto dto,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateImage(dto.Image);
        if (validationError is not null)
        {
            return BadRequest(new { message = validationError });
        }

        var userId = User.UserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "The access token does not contain a valid user identifier." });
        }

        var result = await aiProcessingService.ProcessImageAsync(userId, dto.Image, dto.MockExtractedText, cancellationToken);
        return Ok(result);
    }

    [HttpPost("transactions")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [ProducesResponseType(typeof(ImageTransactionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> CreateTransaction(
        [FromForm] ImageTransactionUploadDto dto,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateImage(dto.Image);
        if (validationError is not null)
        {
            return BadRequest(new { message = validationError });
        }

        var userId = User.UserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new { message = "The access token does not contain a valid user identifier." });
        }

        try
        {
            var result = await imageTransactionService.CreateFromImageAsync(
                userId,
                dto.WalletId,
                dto.Image,
                dto.MockExtractedText,
                Request.Headers.Authorization.ToString(),
                cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (InvalidOperationException exception) when (
            exception.Message.StartsWith("AI_", StringComparison.Ordinal) ||
            exception.Message.StartsWith("FINANCE_", StringComparison.Ordinal))
        {
            return UnprocessableEntity(new { message = exception.Message });
        }
        catch (RpcException exception)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = "Finance Core gRPC request failed.",
                grpcStatus = exception.StatusCode.ToString(),
                detail = exception.Status.Detail
            });
        }
    }

    private string? ValidateImage(IFormFile? image)
    {
        if (image is null || image.Length == 0)
        {
            return "Image file is required.";
        }

        if (image.Length > _imageUploadOptions.MaxFileSizeBytes)
        {
            return $"Image size must be less than {_imageUploadOptions.MaxFileSizeBytes} bytes.";
        }

        if (!_imageUploadOptions.AllowedContentTypes.Contains(image.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return $"Image content type must be one of: {string.Join(", ", _imageUploadOptions.AllowedContentTypes)}.";
        }

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (extension is not ".jpg" and not ".jpeg" and not ".png" and not ".webp")
        {
            return "Image extension must be .jpg, .jpeg, .png, or .webp.";
        }

        return null;
    }
}
