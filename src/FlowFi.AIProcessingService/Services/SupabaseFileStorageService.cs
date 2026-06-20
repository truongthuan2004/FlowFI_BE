using System.Net.Http.Headers;
using FlowFi.AIProcessingService.Config;
using FlowFi.AIProcessingService.Interface;
using Microsoft.Extensions.Options;

namespace FlowFi.AIProcessingService.Services;

public sealed class SupabaseFileStorageService(
    HttpClient httpClient,
    IOptions<SupabaseStorageOptions> options) : IFileStorageService
{
    private readonly SupabaseStorageOptions _options = options.Value;

    public async Task<string> SaveAsync(
        byte[] fileBytes,
        string fileName,
        string contentType,
        Guid requestId,
        string category,
        CancellationToken cancellationToken)
    {
        var extension = GetSafeExtension(fileName, contentType);
        var objectPath = BuildObjectPath(category, $"{requestId:N}{extension}");
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("object", objectPath));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Headers.TryAddWithoutValidation("apikey", _options.ApiKey);
        request.Headers.TryAddWithoutValidation("x-upsert", "true");
        request.Content = new ByteArrayContent(fileBytes);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Supabase Storage upload failed ({(int)response.StatusCode}): {error}");
        }

        return BuildUrl("object/public", objectPath);
    }

    private string BuildObjectPath(string category, string storedFileName)
    {
        var parts = new[] { _options.Folder, category, storedFileName }
            .Select(part => part.Trim('/'))
            .Where(part => !string.IsNullOrWhiteSpace(part));
        return string.Join('/', parts);
    }

    private string BuildUrl(string operation, string objectPath)
    {
        var encodedBucket = Uri.EscapeDataString(_options.BucketName);
        var encodedPath = string.Join('/', objectPath.Split('/').Select(Uri.EscapeDataString));
        return $"{_options.BaseUrl.TrimEnd('/')}/storage/v1/{operation}/{encodedBucket}/{encodedPath}";
    }

    private static string GetSafeExtension(string fileName, string contentType)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp",
            ".mp3", ".wav", ".m4a", ".mp4", ".aac", ".ogg", ".webm"
        };

        if (allowedExtensions.Contains(extension))
        {
            return extension;
        }

        return contentType.ToLowerInvariant() switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            "audio/wav" or "audio/x-wav" => ".wav",
            "audio/mp4" or "audio/x-m4a" => ".m4a",
            "audio/webm" => ".webm",
            "audio/ogg" => ".ogg",
            "audio/aac" => ".aac",
            "audio/mpeg" or "audio/mp3" => ".mp3",
            _ => ".bin"
        };
    }
}
