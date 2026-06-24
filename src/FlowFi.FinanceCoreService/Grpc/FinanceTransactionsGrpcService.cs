using System.Globalization;
using FlowFi.Common.Api;
using FlowFi.FinanceCoreService.DTOs;
using FlowFi.FinanceCoreService.Interface;
using FlowFi.FinanceCoreService.Services;
using FlowFi.GrpcContracts.Finance;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace FlowFi.FinanceCoreService.Grpc;

[Authorize]
public sealed class FinanceTransactionsGrpcService(
    ITagService tagService,
    ITransactionService transactionService) : FinanceTransactions.FinanceTransactionsBase
{
    public override async Task<ListTagsReply> ListTags(
        ListTagsRequest request,
        ServerCallContext context)
    {
        var userId = GetUserId(context);
        var transactionType = NormalizeTransactionType(request.TransactionType);
        var tags = await tagService.GetByUserAndTypeAsync(
            userId,
            transactionType,
            context.CancellationToken);

        var reply = new ListTagsReply();
        reply.Tags.AddRange(tags.Select(ToMessage));
        return reply;
    }

    public override async Task<TagMessage> CreateTag(
        CreateTagRequest request,
        ServerCallContext context)
    {
        var userId = GetUserId(context);
        var transactionType = NormalizeTransactionType(request.TransactionType);
        var name = RequiredText(request.Name, nameof(request.Name), 100);

        var tag = await tagService.GetOrCreateAsync(
            new CreateTagDto
        {
            UserId = userId,
            Name = name,
            Type = transactionType,
            Icon = OptionalText(request.Icon, "receipt", 100),
            Color = OptionalText(request.Color, "#64748B", 20)
        }, context.CancellationToken);
        return ToMessage(tag);
    }

    public override async Task<CreateTransactionReply> CreateTransaction(
        FlowFi.GrpcContracts.Finance.CreateTransactionRequest request,
        ServerCallContext context)
    {
        var userId = GetUserId(context);
        if (!Guid.TryParse(request.WalletId, out var walletId) || walletId == Guid.Empty)
        {
            throw InvalidArgument("wallet_id must be a valid UUID.");
        }

        if (!Guid.TryParse(request.TagId, out var tagId) || tagId == Guid.Empty)
        {
            throw InvalidArgument("tag_id must be a valid UUID.");
        }

        if (!decimal.TryParse(request.Amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        {
            throw InvalidArgument("amount must be greater than zero.");
        }

        var transactionType = NormalizeTransactionType(request.TransactionType);
        DateTimeOffset? transactionDate = null;
        if (!string.IsNullOrWhiteSpace(request.TransactionDate))
        {
            if (!DateTimeOffset.TryParse(
                    request.TransactionDate,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out var parsedDate))
            {
                throw InvalidArgument("transaction_date must be a valid ISO-8601 timestamp.");
            }

            transactionDate = parsedDate.ToUniversalTime();
        }

        var result = await transactionService.CreateAsync(
            userId,
            walletId,
            new CreateTransactionDto
            {
                TagId = tagId,
                Amount = amount,
                Type = transactionType,
                Title = RequiredText(request.Title, nameof(request.Title), 150),
                Note = OptionalText(request.Note, "Created from an AI-processed image.", 2000),
                Source = "AI"
            },
            transactionDate,
            context.CancellationToken);

        if (result.Status != CreateTransactionStatus.Success || result.Transaction is null)
        {
            return new CreateTransactionReply
            {
                Success = false,
                ErrorCode = result.Status.ToString()
            };
        }

        var transaction = result.Transaction;
        return new CreateTransactionReply
        {
            Success = true,
            Transaction = new TransactionMessage
            {
                Id = transaction.Id.ToString(),
                WalletId = transaction.WalletId.ToString(),
                TagId = transaction.TagId.ToString(),
                Amount = transaction.Amount.ToString(CultureInfo.InvariantCulture),
                TransactionType = transaction.Type,
                Title = transaction.Title,
                Note = transaction.Note,
                Source = transaction.Source,
                SyncStatus = transaction.SyncStatus,
                TransactionDate = transaction.TransactionDate.ToString("O"),
                CreatedAt = transaction.CreatedAt.ToString("O")
            }
        };
    }

    private static Guid GetUserId(ServerCallContext context)
    {
        var userId = context.GetHttpContext().User.UserId();
        return userId == Guid.Empty
            ? throw new RpcException(new Status(StatusCode.Unauthenticated, "A valid user claim is required."))
            : userId;
    }

    private static string NormalizeTransactionType(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();
        return normalized is "INCOME" or "EXPENSE"
            ? normalized
            : throw InvalidArgument("transaction_type must be INCOME or EXPENSE.");
    }

    private static string RequiredText(string value, string field, int maxLength)
    {
        var normalized = value.Trim();
        if (normalized.Length == 0 || normalized.Length > maxLength)
        {
            throw InvalidArgument($"{field} is required and must not exceed {maxLength} characters.");
        }

        return normalized;
    }

    private static string OptionalText(string? value, string defaultValue, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static RpcException InvalidArgument(string message)
        => new(new Status(StatusCode.InvalidArgument, message));

    private static TagMessage ToMessage(TagDto tag)
        => new()
        {
            Id = tag.Id.ToString(),
            Name = tag.Name,
            TransactionType = tag.Type,
            Icon = tag.Icon,
            Color = tag.Color
        };
}
