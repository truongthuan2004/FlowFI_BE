using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using FlowFi.AIProcessingService.DTOs;
using FlowFi.AIProcessingService.Interface;

namespace FlowFi.AIProcessingService.Services;

public sealed partial class ReceiptParserService : IReceiptParserService
{
    public ParsedAiTransactionDto Parse(string rawText)
    {
        var rawValue = rawText.Trim();
        var searchableValue = NormalizeForMatching(rawValue);
        return new ParsedAiTransactionDto(
            ExtractAmount(searchableValue),
            ExtractTransactionType(searchableValue),
            ExtractTag(searchableValue),
            ExtractDate(rawValue),
            rawValue);
    }

    public ImageAnalysisDto ParseImageAnalysis(string responseText)
    {
        var json = StripJsonCodeFence(responseText);
        if (!json.StartsWith('{'))
        {
            throw new InvalidOperationException("AI_INVALID_STRUCTURED_RESPONSE");
        }

        try
        {
            var analysis = JsonSerializer.Deserialize<StructuredImageAnalysis>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (analysis?.Transactions is null)
            {
                throw new InvalidOperationException("AI_INVALID_STRUCTURED_RESPONSE");
            }

            var imageType = analysis.ImageType?.Trim().ToUpperInvariant();
            if (imageType is not ("RECEIPT" or "BANK_TRANSFER" or "NOTE" or "UNKNOWN") ||
                analysis.Confidence is < 0 or > 1)
            {
                throw new InvalidOperationException("AI_INVALID_STRUCTURED_RESPONSE");
            }

            var transactions = analysis.Transactions
                .Select(transaction => new ImageAnalyzedTransactionDto(
                    transaction.Title?.Trim() ?? string.Empty,
                    transaction.Amount is > 0 ? transaction.Amount : null,
                    NormalizeStructuredTransactionType(transaction.Type, transaction.TagType),
                    transaction.TagName?.Trim() ?? string.Empty,
                    NormalizeStructuredTag(transaction.TagName, transaction.Type),
                    transaction.Note?.Trim() ?? string.Empty,
                    transaction.TransactionDate,
                    string.IsNullOrWhiteSpace(transaction.MerchantName) ? null : transaction.MerchantName.Trim(),
                    transaction.RawText?.Trim() ?? string.Empty,
                    transaction.Confidence))
                .ToArray();

            if (transactions.Any(transaction => transaction.Confidence is < 0 or > 1))
            {
                throw new InvalidOperationException("AI_INVALID_STRUCTURED_RESPONSE");
            }

            return new ImageAnalysisDto(
                imageType,
                analysis.Confidence,
                transactions,
                analysis.Warnings?.Where(item => !string.IsNullOrWhiteSpace(item)).ToArray() ?? [],
                json);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("AI_INVALID_STRUCTURED_RESPONSE", exception);
        }
    }

    public VoiceAnalysisDto ParseVoiceAnalysis(string responseText)
    {
        var json = StripJsonCodeFence(responseText);
        if (!json.StartsWith('{'))
        {
            throw new InvalidOperationException("AI_INVALID_STRUCTURED_RESPONSE");
        }

        try
        {
            var analysis = JsonSerializer.Deserialize<StructuredVoiceAnalysis>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (analysis?.Transactions is null ||
                !string.Equals(analysis.InputType?.Trim(), "VOICE", StringComparison.OrdinalIgnoreCase) ||
                analysis.Transactions.Count > 1)
            {
                throw new InvalidOperationException("AI_INVALID_STRUCTURED_RESPONSE");
            }

            var transactions = analysis.Transactions
                .Select(transaction => new VoiceAnalyzedTransactionDto(
                    transaction.Title?.Trim() ?? string.Empty,
                    transaction.Amount is > 0 ? transaction.Amount : null,
                    NormalizeStructuredTransactionType(transaction.Type, transaction.TagType),
                    transaction.TagName?.Trim() ?? string.Empty,
                    NormalizeStructuredTag(transaction.TagName, transaction.Type),
                    transaction.Note?.Trim() ?? string.Empty,
                    transaction.TransactionDate,
                    transaction.RawText?.Trim() ?? string.Empty,
                    transaction.Confidence))
                .ToArray();

            if (transactions.Any(transaction => transaction.Confidence is < 0 or > 1))
            {
                throw new InvalidOperationException("AI_INVALID_STRUCTURED_RESPONSE");
            }

            return new VoiceAnalysisDto(
                "VOICE",
                transactions,
                analysis.Warnings?.Where(item => !string.IsNullOrWhiteSpace(item)).ToArray() ?? [],
                json);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("AI_INVALID_STRUCTURED_RESPONSE", exception);
        }
    }

    private static string StripJsonCodeFence(string value)
    {
        var trimmed = value.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            return trimmed;
        }

        var firstLineEnd = trimmed.IndexOf('\n');
        var lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
        return firstLineEnd >= 0 && lastFence > firstLineEnd
            ? trimmed[(firstLineEnd + 1)..lastFence].Trim()
            : trimmed;
    }

    private static string? NormalizeStructuredTransactionType(string? type, string? tagType)
    {
        var normalizedType = type?.Trim().ToUpperInvariant();
        if (normalizedType is "INCOME" or "EXPENSE")
        {
            return normalizedType;
        }

        if (normalizedType == "UNKNOWN")
        {
            return null;
        }

        var normalizedTagType = tagType?.Trim().ToUpperInvariant();
        return normalizedType == "TRANSFER" && normalizedTagType is ("INCOME" or "EXPENSE")
            ? normalizedTagType
            : null;
    }

    private static string? NormalizeStructuredTag(string? tagName, string? type)
    {
        if (string.Equals(type?.Trim(), "TRANSFER", StringComparison.OrdinalIgnoreCase))
        {
            return "TRANSFER";
        }

        var normalized = NormalizeForMatching(tagName ?? string.Empty);
        if (ContainsAny(normalized, "an uong", "ca phe", "tra sua", "com", "bun", "pho")) return "FOOD";
        if (ContainsAny(normalized, "di chuyen", "xang", "grab", "taxi", "ve xe")) return "TRANSPORT";
        if (ContainsAny(normalized, "mua sam", "sieu thi", "tap hoa", "bach hoa xanh", "mua do")) return "SHOPPING";
        if (ContainsAny(normalized, "hoc tap", "hoc phi", "sach vo", "khoa hoc")) return "EDUCATION";
        if (ContainsAny(normalized, "hoa don", "dien", "nuoc", "internet", "dien thoai")) return "UTILITIES";
        if (ContainsAny(normalized, "thu nhap", "luong", "nhan tien", "hoan tien")) return "SALARY";
        return null;
    }

    private static decimal? ExtractAmount(string text)
    {
        var labeledAmounts = TotalAmountRegex().Matches(text)
            .Select(match => new
            {
                Amount = ParseAmount(match),
                Priority = TotalLabelPriority(match.Groups["label"].Value)
            })
            .Where(candidate => candidate.Amount is > 0 and <= 999_999_999_999_999.99m)
            .OrderBy(candidate => candidate.Priority)
            .ToArray();

        if (labeledAmounts.Length > 0)
        {
            return labeledAmounts[0].Amount;
        }

        var matches = AmountRegex().Matches(text).ToArray();
        var currencyMatches = matches
            .Where(match => !string.IsNullOrWhiteSpace(match.Groups["unit"].Value))
            .ToArray();
        var candidates = currencyMatches.Length > 0 ? currencyMatches : matches;
        var amounts = candidates
            .Select(ParseAmount)
            .Where(amount => amount is > 0 and <= 999_999_999_999_999.99m)
            .Select(amount => amount!.Value)
            .ToArray();

        return amounts.Length == 0 ? null : amounts.Max();
    }

    private static int TotalLabelPriority(string label)
    {
        return label.Trim().ToLowerInvariant() switch
        {
            "khach thanh toan" => 0,
            "tong thanh toan" or "so tien thanh toan" or "amount due" => 1,
            "grand total" or "tong cong" => 2,
            "tong tien" or "total" => 3,
            _ => 4
        };
    }

    private static decimal? ParseAmount(Match match)
    {
        var rawAmount = match.Groups["amount"].Value
            .Replace(".", string.Empty)
            .Replace(",", string.Empty);
        if (!decimal.TryParse(rawAmount, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        {
            return null;
        }

        var unit = match.Groups["unit"].Value.ToLowerInvariant();
        return unit is "k" or "nghin" ? amount * 1000 : amount;
    }

    private static string? ExtractTransactionType(string text)
    {
        if (ContainsAny(text, "luong", "nhan tien", "tien vao", "credited", "received", "income"))
        {
            return "INCOME";
        }

        if (ContainsAny(text, "chi", "mua", "tra", "thanh toan", "chuyen khoan", "total", "tong tien", "expense"))
        {
            return "EXPENSE";
        }

        return null;
    }

    private static string? ExtractTag(string text)
    {
        if (ContainsAny(text, "chuyen khoan", "bank transfer", "ngan hang")) return "TRANSFER";
        if (ContainsAny(text, "luong", "salary", "payroll")) return "SALARY";
        if (ContainsAny(text, "an", "food", "com", "pho", "cafe")) return "FOOD";
        if (ContainsAny(text, "xang", "xe", "grab", "taxi", "transport")) return "TRANSPORT";
        if (ContainsAny(text, "mua", "shopping", "hoa don", "sieu thi")) return "SHOPPING";
        if (ContainsAny(text, "hoc", "sach", "education")) return "EDUCATION";
        if (ContainsAny(text, "dien", "nuoc", "internet", "utilities")) return "UTILITIES";
        if (ContainsAny(text, "thuoc", "benh vien", "health", "medical")) return "HEALTH";
        return null;
    }

    private static DateTime? ExtractDate(string text)
    {
        var match = DateRegex().Match(text);
        if (!match.Success)
        {
            return null;
        }

        return DateTime.TryParseExact(
            match.Value,
            ["dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy"],
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsedDate)
            ? parsedDate
            : null;
    }

    private static string NormalizeForMatching(string value)
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

    private static bool ContainsAny(string text, params string[] keywords)
        => keywords.Any(keyword => Regex.IsMatch(
            text,
            $@"\b{Regex.Escape(keyword)}\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));

    [GeneratedRegex(@"(?<amount>\d{1,3}(?:[.,]\d{3})+|\d+)\s*(?<unit>k|nghin|vnd|d|dong)?", RegexOptions.IgnoreCase)]
    private static partial Regex AmountRegex();

    [GeneratedRegex(@"\b(?<label>khach thanh toan|tong thanh toan|so tien thanh toan|amount due|grand total|tong cong|tong tien|total)\b\s*:?\s*(?<amount>\d{1,3}(?:[.,]\d{3})+|\d+)\s*(?<unit>k|nghin|vnd|d|dong)?", RegexOptions.IgnoreCase)]
    private static partial Regex TotalAmountRegex();

    [GeneratedRegex(@"\b\d{1,2}[/-]\d{1,2}[/-]\d{4}\b")]
    private static partial Regex DateRegex();

    private sealed record StructuredImageAnalysis(
        string? ImageType,
        decimal Confidence,
        IReadOnlyList<StructuredImageTransaction>? Transactions,
        IReadOnlyList<string>? Warnings);

    private sealed record StructuredImageTransaction(
        string? Title,
        decimal? Amount,
        string? Type,
        string? TagName,
        string? TagType,
        string? Note,
        DateTime? TransactionDate,
        string? MerchantName,
        string? RawText,
        decimal Confidence);

    private sealed record StructuredVoiceAnalysis(
        string? InputType,
        IReadOnlyList<StructuredVoiceTransaction>? Transactions,
        IReadOnlyList<string>? Warnings);

    private sealed record StructuredVoiceTransaction(
        string? Title,
        decimal? Amount,
        string? Type,
        string? TagName,
        string? TagType,
        string? Note,
        DateTime? TransactionDate,
        string? RawText,
        decimal Confidence);
}
