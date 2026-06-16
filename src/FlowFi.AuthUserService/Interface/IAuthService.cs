using FlowFi.AuthUserService.DTOs;
using FlowFi.AuthUserService.Entities;

namespace FlowFi.AuthUserService.Interface;

public interface IAuthService
{
    Task<User> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<User?> GetUserAsync(Guid id, CancellationToken cancellationToken);
    Task<LoginDevice> RegisterLoginDeviceAsync(LoginDevice device, CancellationToken cancellationToken);
}

