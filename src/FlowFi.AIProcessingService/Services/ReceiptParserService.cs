using System.Globalization;
using System.Text.RegularExpressions;
using FlowFi.AIProcessingService.DTOs;
using FlowFi.AIProcessingService.Interface;

namespace FlowFi.AIProcessingService.Services;

public sealed partial class ReceiptParserService : IReceiptParserService
{
    public ParsedAiTransactionDto Parse(string rawText)
    {
        var normalizedText = rawText.Trim();
        return new ParsedAiTransactionDto(
            ExtractAmount(normalizedText),
            ExtractTransactionType(normalizedText),
            ExtractTag(normalizedText),
            ExtractDate(normalizedText),
            normalizedText);
    }

    private static decimal? ExtractAmount(string text)
    {
        var amounts = AmountRegex()
            .Matches(text)
            .Select(ParseAmount)
            .Where(amount => amount.HasValue)
            .Select(amount => amount!.Value)
            .ToArray();

        return amounts.Length == 0 ? null : amounts.Max();
    }

    private static decimal? ParseAmount(Match match)
    {
        var rawAmount = match.Groups["amount"].Value.Replace(".", string.Empty).Replace(",", string.Empty);
        if (!decimal.TryParse(rawAmount, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        {
            return null;
        }

        var unit = match.Groups["unit"].Value.ToLowerInvariant();
        return unit is "k" or "nghin" or "nghìn" ? amount * 1000 : amount;
    }

    private static string? ExtractTransactionType(string text)
    {
        var lowerText = text.ToLowerInvariant();
        if (ContainsAny(lowerText, "luong", "lương", "nhan", "nhận", "thu", "income"))
        {
            return "INCOME";
        }

        if (ContainsAny(lowerText, "chi", "mua", "tra", "trả", "ton", "tốn", "expense"))
        {
            return "EXPENSE";
        }

        return null;
    }

    private static string? ExtractTag(string text)
    {
        var lowerText = text.ToLowerInvariant();
        if (ContainsAny(lowerText, "an", "ăn", "food", "com", "cơm", "pho", "phở", "cafe"))
        {
            return "FOOD";
        }

        if (ContainsAny(lowerText, "xang", "xăng", "xe", "grab", "taxi", "transport"))
        {
            return "TRANSPORT";
        }

        if (ContainsAny(lowerText, "mua", "shopping", "hoa don", "hóa đơn", "sieuthi", "siêu thị"))
        {
            return "SHOPPING";
        }

        if (ContainsAny(lowerText, "hoc", "học", "sach", "sách", "education"))
        {
            return "EDUCATION";
        }

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

    private static bool ContainsAny(string text, params string[] keywords)
    {
        return keywords.Any(text.Contains);
    }

    [GeneratedRegex(@"(?<amount>\d{1,3}(?:[.,]\d{3})+|\d+)\s*(?<unit>k|nghin|nghìn|vnd|đ|dong|đồng)?", RegexOptions.IgnoreCase)]
    private static partial Regex AmountRegex();

    [GeneratedRegex(@"\b\d{1,2}[/-]\d{1,2}[/-]\d{4}\b")]
    private static partial Regex DateRegex();
}
