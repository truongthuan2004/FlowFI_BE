using FlowFi.FinanceCoreService.Entities;
using FlowFi.FinanceCoreService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.FinanceCoreService.Controllers;

[ApiController]
[Route("transactions")]
public sealed class TransactionsController(IFinanceService financeService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Transaction>>> Get(Guid userId, CancellationToken cancellationToken)
    {
        return Ok(await financeService.GetTransactionsAsync(userId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Transaction>> Create(Transaction transaction, CancellationToken cancellationToken)
    {
        var created = await financeService.CreateTransactionAsync(transaction, cancellationToken);
        return Created($"/transactions/{created.Id}", created);
    }
}

