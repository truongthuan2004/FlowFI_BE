using FlowFi.AIProcessingService.DTOs;

namespace FlowFi.AIProcessingService.Interface;

public interface IReceiptParserService
{
    ParsedAiTransactionDto Parse(string rawText);
    ImageAnalysisDto ParseImageAnalysis(string responseText);
    VoiceAnalysisDto ParseVoiceAnalysis(string responseText);
}
