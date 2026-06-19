using FlowFi.FinanceCoreService.DTOs;
using FlowFi.FinanceCoreService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.FinanceCoreService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TagDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TagDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var tags = await _tagService.GetAllAsync(cancellationToken);
        return Ok(tags);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TagDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tag = await _tagService.GetByIdAsync(id, cancellationToken);
        return tag is null ? NotFound() : Ok(tag);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TagDto>> Create(
        CreateTagDto request,
        CancellationToken cancellationToken)
    {
        var tag = await _tagService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = tag.Id }, tag);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TagDto>> Update(
        Guid id,
        UpdateTagDto request,
        CancellationToken cancellationToken)
    {
        var tag = await _tagService.UpdateAsync(id, request, cancellationToken);
        return tag is null ? NotFound() : Ok(tag);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await _tagService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}

