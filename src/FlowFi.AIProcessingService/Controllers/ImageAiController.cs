using FlowFi.AIProcessingService.Config;
using FlowFi.AIProcessingService.DTOs;
using FlowFi.AIProcessingService.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FlowFi.AIProcessingService.Controllers;

[ApiController]
[Route("api/ai-processing/images")]
public sealed class ImageAiController(
    IAiProcessingService aiProcessingService,
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

        var result = await aiProcessingService.ProcessImageAsync(dto.UserId, dto.Image, dto.MockExtractedText, cancellationToken);
        return Ok(result);
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
