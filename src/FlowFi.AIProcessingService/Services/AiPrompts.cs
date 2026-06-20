namespace FlowFi.AIProcessingService.Services;

public static class AiPrompts
{
    public const string ImageTextExtraction = """
        Read all visible receipt or bill text from this image.
        Return only the raw text, preserving amounts, dates, merchant names, and Vietnamese accents when visible.
        Do not explain the image.
        """;

    public const string VoiceTransactionTranscription = """
        Transcribe this Vietnamese personal-finance voice note accurately.
        Preserve the amount, transaction type, merchant or purpose, and transaction date when spoken.
        Return only the spoken text. Do not explain or summarize it.
        """;
}
