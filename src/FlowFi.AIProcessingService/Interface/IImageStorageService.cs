namespace FlowFi.AIProcessingService.Interface;

public interface IImageStorageService
{
    Task<string> SaveAsync(
        byte[] imageBytes,
        string fileName,
        string contentType,
        Guid requestId,
        CancellationToken cancellationToken);
}
