using FlowFi.FinanceCoreService.Entities;
using FlowFi.FinanceCoreService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.FinanceCoreService.Controllers;

[ApiController]
[Route("wallets")]
public sealed class WalletsController(IFinanceService financeService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Wallet>>> Get(Guid userId, CancellationToken cancellationToken)
    {
        return Ok(await financeService.GetWalletsAsync(userId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Wallet>> Create(Wallet wallet, CancellationToken cancellationToken)
    {
        var created = await financeService.CreateWalletAsync(wallet, cancellationToken);
        return Created($"/wallets/{created.Id}", created);
    }
}

