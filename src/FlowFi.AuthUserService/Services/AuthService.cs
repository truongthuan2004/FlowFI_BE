using FlowFi.AuthUserService.DTOs;
using FlowFi.AuthUserService.Entities;
using FlowFi.AuthUserService.Interface;
using FlowFi.Contracts.Events;
using FlowFi.EventBus.Messaging;

namespace FlowFi.AuthUserService.Services;

public sealed class AuthService(IUserRepository users, RabbitMqPublisher publisher) : IAuthService
{
    public async Task<User> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = new User(Guid.NewGuid(), request.Email, request.DisplayName, DateTimeOffset.UtcNow);
        await users.AddUserAsync(user, cancellationToken);
        await publisher.PublishAsync("user.created", new UserCreated(user.Id, user.Email), cancellationToken);
        return user;
    }

    public Task<User?> GetUserAsync(Guid id, CancellationToken cancellationToken)
    {
        return users.GetUserAsync(id, cancellationToken);
    }

    public Task<LoginDevice> RegisterLoginDeviceAsync(LoginDevice device, CancellationToken cancellationToken)
    {
        var entity = device with
        {
            Id = device.Id == Guid.Empty ? Guid.NewGuid() : device.Id,
            LastSeenAt = DateTimeOffset.UtcNow
        };

        return users.AddLoginDeviceAsync(entity, cancellationToken);
    }
}
