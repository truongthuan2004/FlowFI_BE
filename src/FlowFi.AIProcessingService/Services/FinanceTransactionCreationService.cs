using System.Globalization;
using System.Text;
using FlowFi.AIProcessingService.DTOs;
using FlowFi.AIProcessingService.Interface;

namespace FlowFi.AIProcessingService.Services;

public sealed class FinanceTransactionCreationService(
    IFinanceTransactionsClient financeClient) : IFinanceTransactionCreationService
{
    public async Task<FinanceTransactionCreationResultDto> CreateAsync(
        Guid walletId,
        ParsedAiTransactionDto parsedData,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        ValidateParsedData(parsedData);

        var transactionType = parsedData.TransactionType!.ToUpperInvariant();
        var suggestedTag = CanonicalTag(parsedData.Tag);
        var existingTags = await financeClient.ListTagsAsync(transactionType, bearerToken, cancellationToken);
        var tag = FindMatchingTag(existingTags, suggestedTag);
        var tagCreated = false;

        if (tag is null)
        {
            var presentation = GetTagPresentation(suggestedTag, transactionType);
            tag = await financeClient.CreateTagAsync(
                presentation.Name,
                transactionType,
                presentation.Icon,
                presentation.Color,
                bearerToken,
                cancellationToken);
            tagCreated = true;
        }

        var transaction = await financeClient.CreateTransactionAsync(
            walletId,
            tag.Id,
            parsedData,
            BuildTitle(tag.Name, parsedData.RawText),
            bearerToken,
            cancellationToken);

        return new FinanceTransactionCreationResultDto(tag, tagCreated, transaction);
    }

    private static void ValidateParsedData(ParsedAiTransactionDto parsed)
    {
        if (!parsed.Amount.HasValue || parsed.Amount <= 0)
        {
            throw new InvalidOperationException("AI_AMOUNT_NOT_FOUND");
        }

        if (parsed.TransactionType is not ("INCOME" or "EXPENSE"))
        {
            throw new InvalidOperationException("AI_TRANSACTION_TYPE_NOT_FOUND");
        }
    }

    private static FinanceTagDto? FindMatchingTag(IEnumerable<FinanceTagDto> tags, string suggestedTag)
    {
        var normalizedSuggestion = Normalize(suggestedTag);
        return tags
            .Select(tag => new { Tag = tag, Score = MatchScore(Normalize(tag.Name), normalizedSuggestion) })
            .Where(candidate => candidate.Score > 0)
            .OrderByDescending(candidate => candidate.Score)
            .Select(candidate => candidate.Tag)
            .FirstOrDefault();
    }

    private static int MatchScore(string tagName, string suggestion)
    {
        if (tagName == suggestion) return 100;
        if (CanonicalTag(tagName) == CanonicalTag(suggestion)) return 90;
        if (tagName.Contains(suggestion, StringComparison.Ordinal) ||
            suggestion.Contains(tagName, StringComparison.Ordinal)) return 60;

        return tagName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Intersect(suggestion.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Count() * 10;
    }

    private static string CanonicalTag(string? value)
    {
        var normalized = Normalize(value ?? string.Empty);
        if (ContainsAny(normalized, "food", "an uong", "restaurant", "cafe", "grocery")) return "FOOD";
        if (ContainsAny(normalized, "transport", "di chuyen", "xang", "taxi", "grab")) return "TRANSPORT";
        if (ContainsAny(normalized, "shopping", "mua sam", "store", "supermarket")) return "SHOPPING";
        if (ContainsAny(normalized, "education", "giao duc", "hoc phi", "sach")) return "EDUCATION";
        if (ContainsAny(normalized, "transfer", "chuyen khoan", "bank")) return "TRANSFER";
        if (ContainsAny(normalized, "salary", "luong", "income")) return "SALARY";
        if (ContainsAny(normalized, "health", "medical", "benh vien", "thuoc")) return "HEALTH";
        if (ContainsAny(normalized, "utilities", "dien", "nuoc", "internet")) return "UTILITIES";
        return string.IsNullOrWhiteSpace(normalized) ? "OTHER" : normalized.ToUpperInvariant();
    }

    private static (string Name, string Icon, string Color) GetTagPresentation(
        string canonicalTag,
        string transactionType)
        => canonicalTag switch
        {
            "FOOD" => ("Food", "utensils", "#F97316"),
            "TRANSPORT" => ("Transport", "car", "#3B82F6"),
            "SHOPPING" => ("Shopping", "shopping-cart", "#EC4899"),
            "EDUCATION" => ("Education", "graduation-cap", "#8B5CF6"),
            "TRANSFER" => ("Bank transfer", "arrow-left-right", "#0EA5E9"),
            "SALARY" => ("Salary", "wallet-cards", "#22C55E"),
            "HEALTH" => ("Health", "heart-pulse", "#EF4444"),
            "UTILITIES" => ("Utilities", "receipt", "#EAB308"),
            _ => (transactionType == "INCOME" ? "Other income" : "Other expense", "tag", "#64748B")
        };

    private static string BuildTitle(string tagName, string rawText)
    {
        var firstLine = rawText
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .FirstOrDefault(line => line.Length > 2);
        var title = string.IsNullOrWhiteSpace(firstLine) ? tagName : firstLine;
        return title.Length <= 150 ? title : title[..150];
    }

    private static string Normalize(string value)
    {
        var decomposed = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character == 'đ' ? 'd' : character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static bool ContainsAny(string value, params string[] candidates)
        => candidates.Any(value.Contains);
}
