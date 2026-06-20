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

        var objectPath = BuildObjectPath($"{requestId:N}{GetSafeExtension(fileName, contentType)}");
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("object", objectPath));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Headers.TryAddWithoutValidation("apikey", _options.ApiKey);
        request.Headers.TryAddWithoutValidation("x-upsert", "true");
        request.Content = new ByteArrayContent(imageBytes);
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

    private string BuildObjectPath(string fileName)
    {
        var folder = _options.Folder.Trim('/');
        return string.IsNullOrWhiteSpace(folder) ? fileName : $"{folder}/{fileName}";
    }

    private string BuildUrl(string operation, string objectPath)
    {
        var encodedBucket = Uri.EscapeDataString(_options.BucketName);
        var encodedPath = string.Join('/', objectPath.Split('/').Select(Uri.EscapeDataString));
        return $"{_options.BaseUrl.TrimEnd('/')}/storage/v1/{operation}/{encodedBucket}/{encodedPath}";
    }

    private void ValidateConfiguration()
    {
        if (!Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("SupabaseStorage:BaseUrl is invalid.");
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
