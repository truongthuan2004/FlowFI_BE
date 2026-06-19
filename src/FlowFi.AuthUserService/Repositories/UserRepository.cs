using FlowFi.AuthUserService.Database;
using FlowFi.AuthUserService.Entities;
using FlowFi.AuthUserService.Interfaces;

namespace FlowFi.AuthUserService.Repositories;

public sealed class UserRepository(AuthDbContext db) : IUserRepository
{
    public async Task AddUserAsync(User user, CancellationToken cancellationToken)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<User?> GetUserAsync(Guid id, CancellationToken cancellationToken)
    {
        return await db.Users.FindAsync([id], cancellationToken);
    }

    public async Task<LoginDevice> AddLoginDeviceAsync(LoginDevice device, CancellationToken cancellationToken)
    {
        db.LoginDevices.Add(device);
        await db.SaveChangesAsync(cancellationToken);
        return device;
    }
}

