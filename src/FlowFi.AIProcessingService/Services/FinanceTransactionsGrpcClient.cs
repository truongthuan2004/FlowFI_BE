using System.Globalization;
using FlowFi.AIProcessingService.DTOs;
using FlowFi.AIProcessingService.Interface;
using FlowFi.GrpcContracts.Finance;
using Grpc.Core;

namespace FlowFi.AIProcessingService.Services;

public sealed class FinanceTransactionsGrpcClient(
    FinanceTransactions.FinanceTransactionsClient client) : IFinanceTransactionsClient
{
    public async Task<IReadOnlyList<FinanceTagDto>> ListTagsAsync(
        string transactionType,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        var reply = await client.ListTagsAsync(
            new ListTagsRequest { TransactionType = transactionType },
            BuildCallOptions(bearerToken, cancellationToken));

        return reply.Tags.Select(MapTag).ToArray();
    }

    public async Task<FinanceTagDto> CreateTagAsync(
        string name,
        string transactionType,
        string icon,
        string color,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        var reply = await client.CreateTagAsync(
            new CreateTagRequest
            {
                Name = name,
                TransactionType = transactionType,
                Icon = icon,
                Color = color
            },
            BuildCallOptions(bearerToken, cancellationToken));

        return MapTag(reply);
    }

    public async Task<FinanceTransactionDto> CreateTransactionAsync(
        Guid walletId,
        Guid tagId,
        ParsedAiTransactionDto parsedData,
        string title,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        var amount = parsedData.Amount
            ?? throw new InvalidOperationException("AI_AMOUNT_NOT_FOUND");
        var transactionType = parsedData.TransactionType
            ?? throw new InvalidOperationException("AI_TRANSACTION_TYPE_NOT_FOUND");

        var reply = await client.CreateTransactionAsync(
            new CreateTransactionRequest
            {
                WalletId = walletId.ToString(),
                TagId = tagId.ToString(),
                Amount = amount.ToString(CultureInfo.InvariantCulture),
                TransactionType = transactionType,
                Title = title,
                Note = parsedData.RawText,
                TransactionDate = ToIsoTimestamp(parsedData.TransactionDate)
            },
            BuildCallOptions(bearerToken, cancellationToken));

        if (!reply.Success || reply.Transaction is null)
        {
            throw new InvalidOperationException($"FINANCE_{reply.ErrorCode}");
        }

        var transaction = reply.Transaction;
        return new FinanceTransactionDto(
            Guid.Parse(transaction.Id),
            Guid.Parse(transaction.WalletId),
            Guid.Parse(transaction.TagId),
            decimal.Parse(transaction.Amount, CultureInfo.InvariantCulture),
            transaction.TransactionType,
            transaction.Title,
            transaction.Note,
            transaction.Source,
            transaction.SyncStatus,
            DateTimeOffset.Parse(transaction.TransactionDate, CultureInfo.InvariantCulture),
            DateTimeOffset.Parse(transaction.CreatedAt, CultureInfo.InvariantCulture));
    }

    private static CallOptions BuildCallOptions(string bearerToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(bearerToken))
        {
            throw new InvalidOperationException("AUTHORIZATION_TOKEN_REQUIRED");
        }

        var value = bearerToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? bearerToken
            : $"Bearer {bearerToken}";
        return new CallOptions(
            new Metadata { { "authorization", value } },
            cancellationToken: cancellationToken);
    }

    private static FinanceTagDto MapTag(TagMessage tag)
        => new(
            Guid.Parse(tag.Id),
            tag.Name,
            tag.TransactionType,
            tag.Icon,
            tag.Color);

    private static string ToIsoTimestamp(DateTime? value)
    {
        if (!value.HasValue)
        {
            return string.Empty;
        }

        var localDate = DateTime.SpecifyKind(value.Value, DateTimeKind.Unspecified);
        return new DateTimeOffset(localDate, TimeSpan.FromHours(7)).ToString("O");
    }
}
