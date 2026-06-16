using FlowFi.AuthUserService.Entities;

namespace FlowFi.AuthUserService.Interface;

public interface IUserRepository
{
    Task AddUserAsync(User user, CancellationToken cancellationToken);
    Task<User?> GetUserAsync(Guid id, CancellationToken cancellationToken);
    Task<LoginDevice> AddLoginDeviceAsync(LoginDevice device, CancellationToken cancellationToken);
}

