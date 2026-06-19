using FlowFi.AuthUserService.DTOs;
using FlowFi.AuthUserService.Entities;
using FlowFi.AuthUserService.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FlowFi.AuthUserService.Controllers;

[ApiController]
[Route("users")]
public sealed class UsersController(IAuthService authService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<User>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await authService.CreateUserAsync(request, cancellationToken);
        return Created($"/users/{user.Id}", user);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<User>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await authService.GetUserAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }
}

