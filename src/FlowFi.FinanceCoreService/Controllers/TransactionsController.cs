using FlowFi.Common.Api;
using FlowFi.FinanceCoreService.DTOs;
using FlowFi.FinanceCoreService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.FinanceCoreService.Controllers;

[Authorize]
[ApiController]
[Route("api/wallets/{walletId:guid}/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TransactionDto>> Create(
        Guid walletId,
        CreateTransactionDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.UserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(new
            {
                message = "The access token does not contain a valid user identifier."
            });
        }

        if (walletId == Guid.Empty)
        {
            return BadRequest(new { message = "WalletId must be a valid non-empty UUID." });
        }

        var result = await _transactionService.CreateAsync(
            userId,
            walletId,
            request,
            null,
            cancellationToken);

        return result.Status switch
        {
            CreateTransactionStatus.InvalidWalletId =>
                BadRequest(new { message = "WalletId must be a valid non-empty UUID." }),
            CreateTransactionStatus.InvalidAmount =>
                BadRequest(new { message = "Amount must be greater than zero." }),
            CreateTransactionStatus.InvalidTransactionType =>
                BadRequest(new { message = "Transaction type must be INCOME or EXPENSE." }),
            CreateTransactionStatus.WalletNotFound =>
                NotFound(new { message = "Wallet was not found." }),
            CreateTransactionStatus.WalletInactive =>
                Conflict(new { message = "Wallet is inactive." }),
            CreateTransactionStatus.TagNotFound =>
                NotFound(new { message = "Tag was not found." }),
            CreateTransactionStatus.TagTypeMismatch =>
                BadRequest(new { message = "Tag type must match transaction type." }),
            CreateTransactionStatus.InsufficientBalance =>
                Conflict(new { message = "Wallet balance is insufficient." }),
            _ => StatusCode(StatusCodes.Status201Created, result.Transaction)
        };
    }
}
