using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FlowFi.AIProcessingService.Config;
using FlowFi.AIProcessingService.DTOs;
using FlowFi.AIProcessingService.Interface;
using Microsoft.Extensions.Options;

namespace FlowFi.AIProcessingService.Services;

public sealed class AiModelClient(HttpClient httpClient, IOptions<AiProviderOptions> options) : IAiModelClient
{
    private readonly AiProviderOptions _options = options.Value;

    public async Task<AiTextExtractionResultDto> ExtractTextFromImageAsync(
        byte[] imageBytes,
        string contentType,
        string? mockExtractedText,
        CancellationToken cancellationToken)
    {
        if (IsMeaningfulMockText(mockExtractedText))
        {
            return new AiTextExtractionResultDto(mockExtractedText!.Trim());
        }

        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new InvalidOperationException("AI provider base URL is not configured. Set AiProvider:BaseUrl.");
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("AI provider API key is not configured. Set AiProvider:ApiKey or environment variable AiProvider__ApiKey.");
        }

        httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

        var dataUrl = $"data:{NormalizeContentType(contentType)};base64,{Convert.ToBase64String(imageBytes)}";

        var payload = new
        {
            model = _options.Model,
            input = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "input_text", text = AiPrompts.ImageTextExtraction },
                        new { type = "input_image", image_url = dataUrl }
                    }
                }
            },
            reasoning = new { effort = _options.ReasoningEffort },
            text = new { verbosity = _options.Verbosity }
        };

        return await SendAsync(payload, cancellationToken);
    }

    public async Task<AiTextExtractionResultDto> TranscribeVoiceAsync(
        byte[] audioBytes,
        string fileName,
        string contentType,
        string? mockTranscribedText,
        CancellationToken cancellationToken)
    {
        if (IsMeaningfulMockText(mockTranscribedText))
        {
            return new AiTextExtractionResultDto(mockTranscribedText!.Trim());
        }

        EnsureProviderConfiguration();
        httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

        var payload = new
        {
            model = _options.Model,
            input = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "input_text", text = AiPrompts.VoiceTransactionTranscription },
                        new
                        {
                            type = "input_audio",
                            input_audio = new
                            {
                                data = Convert.ToBase64String(audioBytes),
                                format = NormalizeAudioFormat(fileName, contentType)
                            }
                        }
                    }
                }
            },
            reasoning = new { effort = _options.ReasoningEffort },
            text = new { verbosity = _options.Verbosity }
        };

        return await SendAsync(payload, cancellationToken);
    }

    private async Task<AiTextExtractionResultDto> SendAsync(object payload, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildResponsesEndpoint())
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
        return new AiTextExtractionResultDto(ExtractOutputText(document.RootElement));
    }

    private Uri BuildResponsesEndpoint()
    {
        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var path = _options.ResponsesPath.StartsWith('/') ? _options.ResponsesPath : $"/{_options.ResponsesPath}";
        return new Uri($"{baseUrl}{path}");
    }

    private static bool IsMeaningfulMockText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmedValue = value.Trim();
        return !string.Equals(trimmedValue, "string", StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractOutputText(JsonElement root)
    {
        if (root.TryGetProperty("output_text", out var outputTextElement))
        {
            return outputTextElement.GetString() ?? string.Empty;
        }

        if (!root.TryGetProperty("output", out var outputElement) || outputElement.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        foreach (var outputItem in outputElement.EnumerateArray())
        {
            if (!outputItem.TryGetProperty("content", out var contentElement) || contentElement.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var contentItem in contentElement.EnumerateArray())
            {
                if (contentItem.TryGetProperty("text", out var textElement))
                {
                    return textElement.GetString() ?? string.Empty;
                }
            }
        }

        return string.Empty;
    }

    private static string NormalizeContentType(string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            "image/png" => "image/png",
            "image/webp" => "image/webp",
            _ => "image/jpeg"
        };
    }

    private static string NormalizeAudioFormat(string fileName, string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            "audio/wav" or "audio/x-wav" => "wav",
            "audio/mp4" or "audio/x-m4a" => "m4a",
            "audio/webm" => "webm",
            "audio/ogg" => "ogg",
            _ => Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant() switch
            {
                "wav" or "m4a" or "webm" or "ogg" => Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant(),
                _ => "mp3"
            }
        };
    }

    private void EnsureProviderConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new InvalidOperationException("AI provider base URL is not configured. Set AiProvider:BaseUrl.");
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("AI provider API key is not configured. Set AiProvider:ApiKey.");
        }
    }
}
