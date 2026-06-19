namespace FlowFi.AIProcessingService.Services;

public static class AiPrompts
{
    public const string ImageTextExtraction = """
        Read all visible receipt or bill text from this image.
        Return only the raw text, preserving amounts, dates, merchant names, and Vietnamese accents when visible.
        Do not explain the image.
        """;
}
