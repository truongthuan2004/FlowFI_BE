namespace FlowFi.AIProcessingService.Config;

public sealed class AiProviderOptions
{
    public string Provider { get; set; } = "GetNexAI";
    public string BaseUrl { get; set; } = "https://getnexai.net/api/v1";
    public string ResponsesPath { get; set; } = "/responses";
    public string Model { get; set; } = "gpt-5.5";
    public string? ApiKey { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public string ReasoningEffort { get; set; } = "low";
    public string Verbosity { get; set; } = "low";
    public decimal MinimumImageConfidence { get; set; } = 0.70m;
    public decimal MinimumTransactionConfidence { get; set; } = 0.70m;
}
