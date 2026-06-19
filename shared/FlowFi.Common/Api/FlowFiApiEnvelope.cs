namespace FlowFi.Common.Api;

public sealed record FlowFiApiEnvelope(
    bool Success,
    string? Message,
    object? Data,
    IReadOnlyDictionary<string, string[]>? Errors,
    string TraceId)
{
    public static FlowFiApiEnvelope Ok(object? data, string? message, string traceId)
    {
        return new FlowFiApiEnvelope(true, message, data, null, traceId);
    }

    public static FlowFiApiEnvelope Fail(
        string message,
        IReadOnlyDictionary<string, string[]>? errors,
        string traceId)
    {
        return new FlowFiApiEnvelope(false, message, null, errors, traceId);
    }
}
