using FlowFi.FinanceCoreService.DTOs;
using FlowFi.FinanceCoreService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.FinanceCoreService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletsController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WalletDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WalletDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var wallets = await _walletService.GetAllAsync(cancellationToken);
        return Ok(wallets);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WalletDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var wallet = await _walletService.GetByIdAsync(id, cancellationToken);
        return wallet is null ? NotFound() : Ok(wallet);
    }

    [HttpPost("{userId:guid}")]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WalletDto>> Create(
        Guid userId,
        CreateWalletDto request,
        CancellationToken cancellationToken)
    {
        var wallet = await _walletService.CreateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = wallet.Id }, wallet);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WalletDto>> Update(
        Guid id,
        UpdateWalletDto request,
        CancellationToken cancellationToken)
    {
        var wallet = await _walletService.UpdateAsync(id, request, cancellationToken);
        return wallet is null ? NotFound() : Ok(wallet);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await _walletService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}

