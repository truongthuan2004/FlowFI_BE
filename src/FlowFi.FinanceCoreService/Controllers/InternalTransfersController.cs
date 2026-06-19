using FlowFi.FinanceCoreService.DTOs;
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
    public async Task<ActionResult<InternalTransferDto>> Create(
        CreateInternalTransferDto request,
        CancellationToken cancellationToken)
    {
        var result = await _transferService.CreateAsync(request, cancellationToken);

        return result.Status switch
        {
            CreateInternalTransferStatus.SameWallet =>
                BadRequest(new { message = "Source and destination wallets must be different." }),
            CreateInternalTransferStatus.SourceWalletNotFound =>
                NotFound(new { message = "Source wallet was not found." }),
            CreateInternalTransferStatus.DestinationWalletNotFound =>
                NotFound(new { message = "Destination wallet was not found." }),
            CreateInternalTransferStatus.InsufficientBalance =>
                BadRequest(new { message = "Source wallet has insufficient balance." }),
            _ => StatusCode(StatusCodes.Status201Created, result.Transfer)
        };
    }
}

