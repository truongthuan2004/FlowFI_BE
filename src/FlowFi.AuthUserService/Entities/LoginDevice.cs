namespace FlowFi.AuthUserService.Entities;

public sealed record LoginDevice(Guid Id, Guid UserId, string DeviceName, string IpAddress, string UserAgent, DateTimeOffset LastSeenAt);

