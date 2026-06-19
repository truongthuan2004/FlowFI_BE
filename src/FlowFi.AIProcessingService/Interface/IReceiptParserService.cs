using FlowFi.AIProcessingService.DTOs;

namespace FlowFi.AIProcessingService.Interface;

public interface IReceiptParserService
{
    ParsedAiTransactionDto Parse(string rawText);
}
