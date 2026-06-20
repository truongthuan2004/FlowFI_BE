namespace FlowFi.AIProcessingService.Interface;

public interface IFileStorageService
{
    Task<string> SaveAsync(
        byte[] fileBytes,
        string fileName,
        string contentType,
        Guid requestId,
        string category,
        CancellationToken cancellationToken);
}
