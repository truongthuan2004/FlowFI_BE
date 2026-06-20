using System.Globalization;
using System.Text;
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

    private static decimal? ExtractAmount(string text)
    {
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

    [GeneratedRegex(@"\b\d{1,2}[/-]\d{1,2}[/-]\d{4}\b")]
    private static partial Regex DateRegex();
}
