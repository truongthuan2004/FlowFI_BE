using System.Net.Http.Headers;
using FlowFi.AIProcessingService.Config;
using FlowFi.AIProcessingService.Interface;
using Microsoft.Extensions.Options;

namespace FlowFi.AIProcessingService.Services;

public sealed class SupabaseImageStorageService(
    HttpClient httpClient,
    IOptions<SupabaseStorageOptions> options) : IImageStorageService
{
    private readonly SupabaseStorageOptions _options = options.Value;

    public async Task<string> SaveAsync(
        byte[] imageBytes,
        string fileName,
        string contentType,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        ValidateConfiguration();

        var extension = GetSafeExtension(fileName, contentType);
        var objectPath = BuildObjectPath($"{requestId:N}{extension}");
        var uploadUrl = BuildStorageUrl("object", objectPath);

        using var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Headers.TryAddWithoutValidation("apikey", _options.ApiKey);
        request.Headers.TryAddWithoutValidation("x-upsert", "true");

        request.Content = new ByteArrayContent(imageBytes);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Supabase Storage upload failed with status {(int)response.StatusCode}: {responseBody}");
        }

        return BuildStorageUrl("object/public", objectPath);
    }

    private string BuildObjectPath(string storedFileName)
    {
        var folder = _options.Folder.Trim('/');
        return string.IsNullOrWhiteSpace(folder)
            ? storedFileName
            : $"{folder}/{storedFileName}";
    }

    private string BuildStorageUrl(string operation, string objectPath)
    {
        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var bucket = Uri.EscapeDataString(_options.BucketName);
        var encodedPath = string.Join('/', objectPath.Split('/').Select(Uri.EscapeDataString));
        return $"{baseUrl}/storage/v1/{operation}/{bucket}/{encodedPath}";
    }

    private void ValidateConfiguration()
    {
        if (!Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("SupabaseStorage:BaseUrl must be a valid absolute URL.");
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("SupabaseStorage:ApiKey is required.");
        }

        if (string.IsNullOrWhiteSpace(_options.BucketName))
        {
            throw new InvalidOperationException("SupabaseStorage:BucketName is required.");
        }
    }

    private static string GetSafeExtension(string fileName, string contentType)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension is ".jpg" or ".jpeg" or ".png" or ".webp")
        {
            return extension;
        }

        return contentType.ToLowerInvariant() switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };
    }
}
