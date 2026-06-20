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
[Route("api/ai-processing/voices")]
public sealed class VoiceAiController(
    IVoiceTransactionService voiceTransactionService,
    IOptions<AudioUploadOptions> audioUploadOptions) : ControllerBase
{
    private readonly AudioUploadOptions _audioUploadOptions = audioUploadOptions.Value;

    [HttpPost("transactions")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(25 * 1024 * 1024)]
    [ProducesResponseType(typeof(VoiceTransactionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> CreateTransaction(
        [FromForm] VoiceTransactionUploadDto dto,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateVoice(dto.Voice);
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
            var result = await voiceTransactionService.CreateFromVoiceAsync(
                userId,
                dto.WalletId,
                dto.Voice,
                dto.MockTranscribedText,
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

    private string? ValidateVoice(IFormFile? voice)
    {
        if (voice is null || voice.Length == 0)
        {
            return "Voice file is required.";
        }

        if (voice.Length > _audioUploadOptions.MaxFileSizeBytes)
        {
            return $"Voice size must be less than {_audioUploadOptions.MaxFileSizeBytes} bytes.";
        }

        if (!_audioUploadOptions.AllowedContentTypes.Contains(voice.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return $"Voice content type must be one of: {string.Join(", ", _audioUploadOptions.AllowedContentTypes)}.";
        }

        var extension = Path.GetExtension(voice.FileName).ToLowerInvariant();
        if (extension is not ".mp3" and not ".wav" and not ".m4a" and
            not ".mp4" and not ".ogg" and not ".webm")
        {
            return "Voice extension must be .mp3, .wav, .m4a, .mp4, .ogg, or .webm.";
        }

        return null;
    }
}
