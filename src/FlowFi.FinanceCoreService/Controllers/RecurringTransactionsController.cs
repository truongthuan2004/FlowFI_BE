using FlowFi.FinanceCoreService.DTOs;
using FlowFi.FinanceCoreService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.FinanceCoreService.Controllers;

[Authorize]
[ApiController]
[Route("api/recurring-transactions")]
public class RecurringTransactionsController : ControllerBase
{
    private readonly IRecurringTransactionService _service;

    public RecurringTransactionsController(IRecurringTransactionService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RecurringTransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RecurringTransactionDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var transactions = await _service.GetAllAsync(cancellationToken);
        return Ok(transactions);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RecurringTransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RecurringTransactionDto>> Create(
        CreateRecurringTransactionDto request,
        CancellationToken cancellationToken)
    {
        var transaction = await _service.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, transaction);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RecurringTransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecurringTransactionDto>> Update(
        Guid id,
        UpdateRecurringTransactionDto request,
        CancellationToken cancellationToken)
    {
        var transaction = await _service.UpdateAsync(id, request, cancellationToken);
        return transaction is null ? NotFound() : Ok(transaction);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}

