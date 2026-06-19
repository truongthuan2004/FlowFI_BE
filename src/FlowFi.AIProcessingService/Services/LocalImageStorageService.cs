using FlowFi.AIProcessingService.Config;
using FlowFi.AIProcessingService.Interface;
using Microsoft.Extensions.Options;

namespace FlowFi.AIProcessingService.Services;

public sealed class LocalImageStorageService(IOptions<ImageUploadOptions> options) : IImageStorageService
{
    private readonly ImageUploadOptions _options = options.Value;

    public async Task<string> SaveAsync(
        byte[] imageBytes,
        string fileName,
        string contentType,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_options.DataFolder);

        var extension = GetSafeExtension(fileName, contentType);
        var storedFileName = $"{requestId:N}{extension}";
        var relativePath = Path.Combine(_options.DataFolder, storedFileName);

        await File.WriteAllBytesAsync(relativePath, imageBytes, cancellationToken);
        return relativePath.Replace('\\', '/');
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
