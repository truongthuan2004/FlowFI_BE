using FlowFi.AuthUserService.Entities;

namespace FlowFi.AuthUserService.Interfaces;

public interface IUserRepository
{
    Task AddUserAsync(User user, CancellationToken cancellationToken);
    Task<User?> GetUserAsync(Guid id, CancellationToken cancellationToken);
    Task<LoginDevice> AddLoginDeviceAsync(LoginDevice device, CancellationToken cancellationToken);
}

