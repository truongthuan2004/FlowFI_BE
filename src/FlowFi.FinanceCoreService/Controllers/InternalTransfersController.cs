using FlowFi.FinanceCoreService.DTOs;
using FlowFi.FinanceCoreService.Interface;
using FlowFi.FinanceCoreService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.FinanceCoreService.Controllers;

[Authorize]
[ApiController]
[Route("api/internal-transfers")]
public class InternalTransfersController : ControllerBase
{
    private readonly IInternalTransferService _transferService;

    public InternalTransfersController(IInternalTransferService transferService)
    {
        _transferService = transferService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(InternalTransferDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InternalTransferDto>> Create(
        CreateTransferRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _transferService.CreateAsync(request, cancellationToken);

        return result.Status switch
        {
            CreateInternalTransferStatus.SameWallet =>
                BadRequest(new { message = "Source and destination wallets must be different." }),
            CreateInternalTransferStatus.InvalidAmount =>
                BadRequest(new { message = "Amount must be greater than zero." }),
            CreateInternalTransferStatus.SourceWalletNotFound =>
                NotFound(new { message = "Source wallet was not found." }),
            CreateInternalTransferStatus.DestinationWalletNotFound =>
                NotFound(new { message = "Destination wallet was not found." }),
            CreateInternalTransferStatus.InsufficientBalance =>
                Conflict(new { message = "Source wallet has insufficient balance." }),
            _ => StatusCode(StatusCodes.Status201Created, result.Transfer)
        };
    }
}
