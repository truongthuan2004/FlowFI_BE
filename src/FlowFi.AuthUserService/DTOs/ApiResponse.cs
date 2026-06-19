namespace FlowFi.AuthUserService.DTOs;

public sealed class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public ApiErrors? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Thành công")
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Ok(string message = "Thành công")
        => new() { Success = true, Message = message };

    public static ApiResponse<T> Error(string message, string code = "ERROR", string? details = null)
        => new()
        {
            Success = false,
            Message = message,
            Errors = new ApiErrors { Code = code, Details = details }
        };
}

public class ApiErrors
{
    public string Code { get; set; } = string.Empty;
    public string? Details { get; set; }
}

public sealed class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ApiErrors? Errors { get; set; }

    public static ApiResponse Ok(string message = "Thành công")
        => new() { Success = true, Message = message };

    public static ApiResponse Error(string message, string code = "ERROR", string? details = null)
        => new()
        {
            Success = false,
            Message = message,
            Errors = new ApiErrors { Code = code, Details = details }
        };
}
