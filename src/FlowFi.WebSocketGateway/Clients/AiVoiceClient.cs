using System.Net.Http.Headers;
using System.Text.Json;
using FlowFi.WebSocketGateway.Config;
using Microsoft.Extensions.Options;

namespace FlowFi.WebSocketGateway.Clients;

public interface IAiVoiceClient
{
    Task<string> TranscribeAsync(
        byte[] audio,
        string fileName,
        string contentType,
        string bearerToken,
        CancellationToken cancellationToken);

    Task<JsonElement> CreateTransactionAsync(
        Guid walletId,
        byte[] audio,
        string fileName,
        string contentType,
        string bearerToken,
        CancellationToken cancellationToken);
}

public sealed class AiVoiceClient(
    HttpClient httpClient,
    IOptions<VoiceRealtimeOptions> options) : IAiVoiceClient
{
    private readonly VoiceRealtimeOptions _options = options.Value;

    public async Task<string> TranscribeAsync(
        byte[] audio,
        string fileName,
        string contentType,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        using var content = BuildMultipart(audio, fileName, contentType);
        using var request = BuildRequest(
            "/api/ai-processing/voices/transcriptions",
            content,
            bearerToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var root = await ReadResponseAsync(response, cancellationToken);
        var data = UnwrapData(root);
        return data.TryGetProperty("transcriptText", out var transcript)
            ? transcript.GetString() ?? string.Empty
            : throw new InvalidOperationException("AI_TRANSCRIPTION_RESPONSE_INVALID");
    }

    public async Task<JsonElement> CreateTransactionAsync(
        Guid walletId,
        byte[] audio,
        string fileName,
        string contentType,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        using var content = BuildMultipart(audio, fileName, contentType);
        content.Add(new StringContent(walletId.ToString()), "WalletId");
        using var request = BuildRequest(
            "/api/ai-processing/voices/transactions",
            content,
            bearerToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        return UnwrapData(await ReadResponseAsync(response, cancellationToken)).Clone();
    }

    private HttpRequestMessage BuildRequest(
        string path,
        HttpContent content,
        string bearerToken)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_options.AiServiceBaseUrl.TrimEnd('/')}{path}")
        {
            Content = content
        };
        request.Headers.Authorization = AuthenticationHeaderValue.Parse(bearerToken);
        return request;
    }

    private static MultipartFormDataContent BuildMultipart(
        byte[] audio,
        string fileName,
        string contentType)
    {
        var multipart = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(audio);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        multipart.Add(fileContent, "Voice", fileName);
        return multipart;
    }

    private static async Task<JsonElement> ReadResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"AI_SERVICE_{(int)response.StatusCode}:{responseText}");
        }

        using var document = JsonDocument.Parse(responseText);
        return document.RootElement.Clone();
    }

    private static JsonElement UnwrapData(JsonElement root)
        => root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var data)
            ? data
            : root;
}
