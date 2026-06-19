using FlowFi.FinanceCoreService.DTOs;
using FlowFi.FinanceCoreService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.FinanceCoreService.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionDto>> Create(
        CreateTransactionDto request,
        CancellationToken cancellationToken)
    {
        var result = await _transactionService.CreateAsync(request, cancellationToken);

        return result.Status switch
        {
            CreateTransactionStatus.WalletNotFound =>
                NotFound(new { message = "Wallet was not found." }),
            CreateTransactionStatus.TagNotFound =>
                NotFound(new { message = "Tag was not found." }),
            _ => StatusCode(StatusCodes.Status201Created, result.Transaction)
        };
    }
}

